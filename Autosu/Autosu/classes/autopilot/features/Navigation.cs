using Autosu.Classes;
using Autosu.Utils;
using CefSharp;
using Indieteur.GlobalHooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput.Native;
using WindowsInput;
using Microsoft.VisualBasic.Devices;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Autosu.Enums;
using Newtonsoft.Json;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public bool mnavDesyncWarn;
        public bool hnavFaultWarn;

        public bool isHumanInput;
        public List<HitObject> navQueue = new();
        public List<Vector2> mouseMoveQueue = new();
        public List<Vector2> extendMoveQueue = new();

        private bool vKeyboardLock = false;

        public HitObject navTarget {
            get {
                if (navQueue.Count <= 0 && status > EAutopilotMasterState.ON) Disengage(true);
                return navQueue.Count > 0 ? navQueue[0] : new();
            }
        }

        public EPathingControlMode pathingMode = EPathingControlMode.MOUSE;
        private bool nextKeyPressAlt;
        public bool mouseMoveQueueFinished = false;
        public int abortCount = 0;
        public bool abortCatch;
        private bool extendQueueStarted;
        private int nextMoveDelay = 0;
        private int nextHitDelay = 0;

        // premature call will abort current path
        public void NextNav() {
            if (abortCatch) return;

            mouseMoveQueue.Clear();
            var obj = navQueue[0];
            //Debug.WriteLine($"Aborted target #{obj.time} at {time}.");
            navQueue.RemoveAt(0);
            abortCatch = true;
            abortCount++;
        }

        public void NextNav(int count) {
            for (int i = 0; i < count; i++) NextNav();

        }

        private void NavMouseUpdate() {
            sysLatencyTimer.Stop();
            sysLatency = (int) sysLatencyTimer.ElapsedMilliseconds;
            sysLatencyTimer.Restart();

            // execute the movement queue
            // do not execute movement queue if extend movement queue is started
            if (!extendQueueStarted) {
                if (mouseMoveQueue.Count > 0) {
                    var pos = mouseMoveQueue[0];
                    if (status == EAutopilotMasterState.FULL && config.features.mnav) MouseUtil.SetCursor(new((int) Math.Round(pos.X), (int) Math.Round(pos.Y)));
                    mouseMoveQueue.RemoveAt(0);
                    mouseMoveQueueFinished = false;
                    abortCatch = true;

                } else if (!mouseMoveQueueFinished) {
                    // give control to keyboard when queue clears
                    if (pathingMode == EPathingControlMode.MOUSE) pathingMode = EPathingControlMode.KEYBOARD;
                    mouseMoveQueueFinished = true;
                    abortCatch = false;
                }
            }

            // execute the extend queue
            if (extendMoveQueue.Count > 0 && extendQueueStarted) {
                var pos = extendMoveQueue[0];
                //Debug.WriteLine(pos.ToString());
                extendMoveQueue.RemoveAt(0);

                // will refuse to execute zero positions (placeholder positions)
                if (status == EAutopilotMasterState.FULL && config.features.mnav && pos != Vector2.Zero) MouseUtil.SetCursor(new((int) Math.Round(pos.X), (int) Math.Round(pos.Y)));
            
            } else if (extendMoveQueue.Count <= 0 && extendQueueStarted) {
                extendQueueStarted = false;
                //Debug.Print("************SLIDER END");
                NextNav();
                // return control to the mouse
                pathingMode = EPathingControlMode.MOUSE;
            }

        }


        public void NavUpdate() {
            // mouse
            int mnavThreshold = Math.Min(1000, beatmap.visualPeriod);
            if (lastAccuracyRandom != 0) nextHitDelay = (int)lastAccuracyRandom;
            int hnavThreshold = 0 + Math.Min(0, nextHitDelay);

            if (pathingMode == EPathingControlMode.KEYBOARD && nextHitDelay > 0 && navTarget.time - time < nextHitDelay && navTarget.type == EHitObjectType.CIRCLE) {
                PressHitKeySafe();
                navTarget.keyPressed = true;
            }

            if (pathingMode == EPathingControlMode.KEYBOARD && navTarget.time - time < hnavThreshold) {
                // keyboard

                // movement target clears after key press
                float distance = Vector2.Distance(new(Cursor.Position.X, Cursor.Position.Y), APUtil.OsuPixelToScreen(navTarget.pos));    
                if (mouseMoveQueueFinished || (hnavThreshold > 0 && mouseMoveQueue.Count <= Math.Abs(nextHitDelay))) {
                    int delay = -nextHitDelay;

                    // record the delay

                    mouseMoveQueue.Clear();
                    mouseMoveQueueFinished = true;

                    if (navTarget is SliderObject) {
                        if (!vKeyboardLock) results.Add(new(delay));
                        SliderObject slider = (SliderObject) navTarget;
                        extendQueueStarted = true;
                        //Debug.Print($"************SLIDER START: Type: {((SliderObject) navTarget).curveType.ToString()}");
                        int totalDuration = (int) (slider.GetDuration(beatmap) * slider.repeats);
                        if (config.features.hnav && (mouseMoveQueueFinished || status == EAutopilotMasterState.FULL && !abortCatch)) PressSliderKey(totalDuration);
                        // control is returned when extendMoveQueue clears and unlocks

                    } else if (navTarget is SpinnerObject) {
                        SpinnerObject spinner = (SpinnerObject) navTarget;
                        extendQueueStarted = true;
                        //Debug.Print($"************SLIDER START: Type: {((SliderObject) navTarget).curveType.ToString()}");
                        int totalDuration = spinner.endTime - spinner.time;
                        if (config.features.hnav && (mouseMoveQueueFinished || status == EAutopilotMasterState.FULL && !abortCatch)) PressSliderKey(totalDuration);
                        // control is returned when extendMoveQueue clears and unlocks

                    } else {
                        results.Add(new(delay));
                        if (config.features.hnav && (mouseMoveQueueFinished || status == EAutopilotMasterState.FULL && !abortCatch) && !navTarget.keyPressed) PressHitKey();
                        NextNav();
                        // return control to the mouse
                        pathingMode = EPathingControlMode.MOUSE;

                        // calculate the next movement delay
                        nextMoveDelay = config.features.moveDelay ? new Random().Next(-config.inputs.mnavDelayRef, config.inputs.mnavDelayRef) : 0;

                    }

                } else {
                    // nav fault
                    if (status == EAutopilotMasterState.FULL) {
                        if (mouseMoveQueue.Count > 1) {
                            hnavFaultWarn = true;
                        }
                        if (mouseMoveQueue.Count > 3) {
                            hnavFaultWarn = true;
                            Disengage();
                        }
                    }
                }
            } 

            if (pathingMode == EPathingControlMode.MOUSE) {
                int timeDiff = navTarget.time - time - nextMoveDelay;
                if (timeDiff < mnavThreshold - nextMoveDelay) {

                    // mnav desync warning (threshold = 60)
                    if (mouseMoveQueue.Count > 60) {
                        mnavDesyncWarn = true;
                        Disengage();
                    }

                    Vector2 cursorPos = new(Cursor.Position.X, Cursor.Position.Y);
                    Vector2 targetPos = APUtil.OsuPixelToScreen(navTarget.pos);
                    float dist = Vector2.Distance(cursorPos, targetPos);

                    // target loc offset
                    /// less time + less distance = medium offset
                    /// less time + more distance = big offset
                    /// more time + less distance = small offset
                    /// more time + more distance = medium offset
                    /// Distance threshold defines less or more distance
                    float offsetDistance = 0f;
                    if (config.features.targetOffset && timeDiff > config.inputs.targetOffsetThreshold) {
                        float distanceThreshold = config.inputs.targetOffsetAmount;
                        float distMultiplier = dist > distanceThreshold ? 1.2f : 1f;
                        float timeMultiplier = timeDiff > 300 ? timeDiff > 50 ? 0.7f : 0f : 1f;
                        offsetDistance = (float) config.inputs.targetOffsetAmount / 100f * (distMultiplier * timeMultiplier) * beatmap.realCircleRadius;
                        targetPos = APUtil.OffsetLocation(targetPos, offsetDistance);
                    }

                    // get the move to target path

                    float circleRadius = beatmap.realCircleRadius;
                    Vector2[] path;

                    // calculate the total move time
                    int totalMoveTime = Math.Min(navTarget.time - time, mnavThreshold);
                    int holdAtTargetTime = timeDiff < 300 ? new Random().Next(0, 10) : new Random().Next(0, (int)(totalMoveTime / 2)); 

                    if (navQueue.Count > 1 && Vector2.Distance(cursorPos, APUtil.OsuPixelToScreen(navQueue[1].pos)) >= circleRadius / 2f) {
                        path = MouseUtil.GetLinearPath(
                            cursorPos,
                            targetPos,
                            totalMoveTime - holdAtTargetTime,
                            sysLatency
                        );
                    } else path = MouseUtil.GetPlaceholderPath(totalMoveTime - holdAtTargetTime, sysLatency);
                    mouseMoveQueue.AddRange(path);

                    if (holdAtTargetTime > 0) mouseMoveQueue.AddRange(MouseUtil.GetPlaceholderPath(holdAtTargetTime, sysLatency));

                    nextHitDelay = config.features.hitDelay ? new Random().Next(-config.inputs.hnavDelayRef, config.inputs.hnavDelayRef) : 0;

                    // slider
                    if (navTarget is SliderObject) {
                        extendMoveQueue.Clear();
                        SliderObject slider = (SliderObject)navTarget;
                        int duration = (int) Math.Round(slider.GetDuration(beatmap));
                        Vector2 startPos = APUtil.OsuPixelToScreen(slider.pos);

                        // path is for a single slide
                        // remember mouse halt
                        int approachCircleRadius = (int)(circleRadius * (187f / 106f));

                        // halt on short slider
                        float distThresh = config.inputs.sliderHaltThreshold / 10f;
                        // take random target offset into account
                        int circleDist = config.features.shortSliderHalt ? (int) ((approachCircleRadius - circleRadius) * distThresh) : -1;

                        Vector2[] singlePath;
                        switch (slider.curveType) {
                            case ESliderCurveType.BEZIER:
                                singlePath = MouseUtil.GetLinearPathSlider(startPos, slider.actualPoints.ToArray(), duration, sysLatency, circleDist);
                                break;

                            case ESliderCurveType.PERFECT:
                                singlePath = MouseUtil.GetCircularPath(startPos, slider.actualPoints[0], slider.actualPoints[1], duration, sysLatency, circleDist);
                                break;

                            default:
                                float linearDistance = Vector2.Distance(startPos, slider.actualPoints[slider.actualPoints.Length - 1]);
                                singlePath = linearDistance > circleDist ? MouseUtil.GetLinearPath(startPos, slider.actualPoints[slider.actualPoints.Length - 1], duration, sysLatency):
                                    MouseUtil.GetPlaceholderPath(duration, sysLatency);
                                break;
                        }

                        List<Vector2> finalPath = new();
                        bool reversePath = false;
                        for (int i = 0; i < slider.repeats; i++) {
                            if (reversePath) {
                                var reversed = singlePath.ToArray().Reverse();
                                finalPath.AddRange(reversed);
                            } else finalPath.AddRange(singlePath.ToArray());
                            reversePath = !reversePath;
                        }

                        // add the extend queue
                        foreach (var pos in finalPath) extendMoveQueue.Add(pos);

                    }

                    // spinner
                    if (navTarget is SpinnerObject) {
                        extendMoveQueue.Clear();
                        SpinnerObject spinner = (SpinnerObject) navTarget;
                        int duration = spinner.endTime - spinner.time;
                        Vector2 startPos = new(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);

                        // path is for a single slide
                        Vector2 axisLength = new Vector2(70, 70);
                        if (config.features.spinnerOffset) {
                            axisLength.X = new Random().Next(50, 90);
                            axisLength.Y = new Random().Next(50, 90);
                        }
                        Vector2[] spinPath = MouseUtil.GetSpinnerPath(
                            startPos,
                            axisLength.X,
                            axisLength.Y,
                            new Random().Next(290, 370),
                            duration,
                            sysLatency,
                            config.features.spinnerRandom ? (float)config.inputs.spinnerRandomAmount / 10f : 0f
                        );

                        // add the extend queue
                        foreach (var pos in spinPath) extendMoveQueue.Add(pos);
                    }

                    // give control to keyboard
                    pathingMode = EPathingControlMode.KEYBOARD;
                }
            }

        }

        public void PressHitKeySafe() {
            if (vKeyboardLock) return;

            nextKeyPressAlt = !nextKeyPressAlt;

            VirtualKeyCode key = nextKeyPressAlt ? VirtualKeyCode.VK_X : VirtualKeyCode.VK_Z;

            var t = new Thread(() => {
                new InputSimulator().Keyboard.KeyDown(key);

                Thread.Sleep(new Random().Next(20, 35));

                vKeyboardLock = false;
                new InputSimulator().Keyboard.KeyUp(key);
            });

            t.Start();
            vKeyboardLock = true;
        }

        public void PressHitKey() {
            if (vKeyboardLock) return;

            nextKeyPressAlt = !nextKeyPressAlt;

            VirtualKeyCode key = nextKeyPressAlt ? VirtualKeyCode.VK_X : VirtualKeyCode.VK_Z;

            var t = new Thread(() => {
                new InputSimulator().Keyboard.KeyDown(key);

                Thread.Sleep(new Random().Next(20, 35));

                new InputSimulator().Keyboard.KeyUp(key);
            });

            t.Start();
        }

        public void PressSliderKey(int duration) {
            if (vKeyboardLock) return;

            VirtualKeyCode key = new Random().Next(0, 2) == 0 ? VirtualKeyCode.VK_X : VirtualKeyCode.VK_Z;

            var t = new Thread(() => {
                new InputSimulator().Keyboard.KeyDown(key);

                Thread.Sleep(new Random().Next(20, 35) + duration);

                vKeyboardLock = false;
                new InputSimulator().Keyboard.KeyUp(key);
            });

            t.Start();
            vKeyboardLock = true;
        }

    }
}
