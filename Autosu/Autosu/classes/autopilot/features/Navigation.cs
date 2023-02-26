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
                if (status == EAutopilotMasterState.FULL && config.features.mnav) MouseUtil.SetCursor(new((int) Math.Round(pos.X), (int) Math.Round(pos.Y)));
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
            const int hnavThreshold = 0;

            if (pathingMode == EPathingControlMode.KEYBOARD && navTarget.time - time < hnavThreshold) {
                // keyboard

                // movement target clears after key press
                float distance = Vector2.Distance(new(Cursor.Position.X, Cursor.Position.Y), APUtil.OsuPixelToScreen(navTarget.pos));    
                if (mouseMoveQueueFinished) {
                    if (navTarget is SliderObject) {
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
                        if (config.features.hnav && (mouseMoveQueueFinished || status == EAutopilotMasterState.FULL && !abortCatch)) PressHitKey();
                        NextNav();
                        // return control to the mouse
                        pathingMode = EPathingControlMode.MOUSE;
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
                if (navTarget.time - time < mnavThreshold) {
                    int timeDiff = navTarget.time - time;

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
                    if (config.features.targetOffset && timeDiff > config.inputs.targetOffsetThreshold) {
                        float distanceThreshold = config.inputs.targetOffsetAmount;
                        float distMultiplier = dist > distanceThreshold ? 1.2f : 1f;
                        float timeMultiplier = timeDiff > 300 ? timeDiff > 50 ? 0.7f : 0f : 1f;

                        targetPos = APUtil.OffsetLocation(targetPos, (float) config.inputs.targetOffsetAmount / 100f * (distMultiplier * timeMultiplier) * beatmap.realCircleRadius);
                    }

                    //targetPos = navTarget.pos;

                    float circleRadius = APUtil.OsuPixelDistance(54.5f - 4.48f * beatmap.circleSize);
                    Vector2[] path = MouseUtil.GetLinearPath(
                        cursorPos, 
                        targetPos, 
                        Math.Min(navTarget.time - time, mnavThreshold), 
                        sysLatency
                    );

                    foreach (var pos in path) mouseMoveQueue.Add(pos);

                    // slider
                    if (navTarget is SliderObject) {
                        extendMoveQueue.Clear();
                        SliderObject slider = (SliderObject)navTarget;
                        int duration = (int) Math.Round(slider.GetDuration(beatmap));
                        Vector2 startPos = APUtil.OsuPixelToScreen(slider.pos);

                        // path is for a single slide
                        Vector2[] singlePath = slider.curveType switch {
                            ESliderCurveType.BEZIER => MouseUtil.GetLinearPathSlider(startPos, slider.actualPoints.ToArray(), duration, sysLatency),

                            ESliderCurveType.PERFECT => MouseUtil.GetCircularPath(
                                startPos,
                                slider.actualPoints[0],
                                slider.actualPoints[1],
                                duration,
                                sysLatency
                            ),

                            _ => MouseUtil.GetLinearPath(
                                startPos, slider.actualPoints[slider.actualPoints.Length - 1], duration, sysLatency
                            )
                        };

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
