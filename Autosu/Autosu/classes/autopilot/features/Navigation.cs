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

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public bool mnavDesyncWarn;
        public bool hnavFaultWarn;

        public bool isHumanInput;
        public List<HitObject> navQueue = new();
        public List<Vector2> mouseMoveQueue = new();
        public HitObject navTarget => navQueue[0];

        public EPathingControlMode pathingMode = EPathingControlMode.MOUSE;
        private bool nextKeyPressAlt;
        public bool mouseMoveQueueFinished = false;
        public int abortCount = 0;
        public bool abortCatch;

        // premature call will abort current path
        public void NextNav() {
            if (abortCatch) return;

            mouseMoveQueue.Clear();
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
            if (mouseMoveQueue.Count > 0) {
                var pos = mouseMoveQueue[0];
                if (status == EAutopilotMasterState.FULL) MouseUtil.SetCursor(new((int) Math.Round(pos.X), (int) Math.Round(pos.Y)));
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


        public void NavUpdate() {
            // mouse
            const int mnavThreshold = 1000;
            const int hnavThreshold = 0;

            if (pathingMode == EPathingControlMode.KEYBOARD && navTarget.time - time < hnavThreshold) {
                // keyboard

                // movement target clears after key press
                float distance = Vector2.Distance(new(Cursor.Position.X, Cursor.Position.Y), APUtil.OsuPixelToScreen(navTarget.pos));    
                if (mouseMoveQueueFinished) {
                    if (mouseMoveQueueFinished || status == EAutopilotMasterState.FULL && !abortCatch) PressHitKey();
                    NextNav();

                } else {
                    // nav failed, abort
                    if (status == EAutopilotMasterState.FULL) hnavFaultWarn = true;
                }

                // return control to the mouse
                pathingMode = EPathingControlMode.MOUSE;
            } 

            if (pathingMode == EPathingControlMode.MOUSE) {
                // start movin when time diff is less than 0.7 sec, or if to the next note is less than that
                if (navTarget.time - time < mnavThreshold) {

                    // mnav desync warning (threshold = 60)
                    if (mouseMoveQueue.Count > 60) mnavDesyncWarn = true;

                    Vector2 cursorPos = new(Cursor.Position.X, Cursor.Position.Y);
                    Vector2 targetPos = APUtil.OsuPixelToScreen(navTarget.pos);
                    //targetPos = navTarget.pos;

                    Vector2[] path = MouseUtil.GetLinearPath(cursorPos, targetPos, Math.Min(navTarget.time - time, 250), sysLatency);
                    foreach (var pos in path) mouseMoveQueue.Add(pos);

                    // give control to keyboard
                    pathingMode = EPathingControlMode.KEYBOARD;
                }
            }

        }

        public void PressHitKey() {
            nextKeyPressAlt = !nextKeyPressAlt;

            VirtualKeyCode key = nextKeyPressAlt ? VirtualKeyCode.VK_X : VirtualKeyCode.VK_Z;

            var t = new Thread(() => {
                new InputSimulator().Keyboard.KeyDown(key);

                Thread.Sleep(new Random().Next(25, 75));

                new InputSimulator().Keyboard.KeyUp(key);
            });

            t.Start();
        }

    }
}
