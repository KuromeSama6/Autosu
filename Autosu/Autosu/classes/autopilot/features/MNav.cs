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

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public static bool mnavDesyncWarn;

        public static bool isHumanInput;
        public static List<HitObject> mnavQueue = new();
        public static List<Vector2> mouseMoveQueue = new();
        public static HitObject mnavTarget => mnavQueue[0];

        /// <summary>
        /// Aborts navigation to the current navTarget and removes it from navQueue.
        /// </summary>
        /// <returns>A HitObject representing the HitObject that had been just removed.</returns>
        public static HitObject NextMnav() {
            HitObject ret = mnavTarget;
            mnavQueue.RemoveAt(0);
            return ret;
        }

        /// <summary>
        /// Shorthand to skip multiple HitObjects.
        /// </summary>
        /// <param name="count">Amount of HitObjects to skip</param>
        /// <returns>A HitObject[] containing all HitObjects that has been skipped.</returns>
        public static HitObject[] NextMnav(int count) {
            List<HitObject> ret = new();
            for (int i = 0; i < count; i++) ret.Add(NextMnav());
            return ret.ToArray();

        }

        private static void NavMouseUpdate() {
            // execute the movement queue
            if (mouseMoveQueue.Count > 0) {
                var pos = mouseMoveQueue[0];
                MouseUtil.SetCursor(new((int) Math.Round(pos.X), (int) Math.Round(pos.Y)));
                mouseMoveQueue.RemoveAt(0);
            }
        }


        public static void MnavUpdate() {

            const int threshold = 1000;

            // start movin when time diff is less than 0.7 sec, or if to the next note is less than that
            if (mnavTarget.time - time < threshold) {

                // mnav desync warning (threshold = 60)
                if (mouseMoveQueue.Count > 60) mnavDesyncWarn = true;

                Vector2 cursorPos = new(Cursor.Position.X, Cursor.Position.Y);
                Vector2 targetPos = APUtil.OsuPixelToScreen(mnavTarget.pos);
                //targetPos = navTarget.pos;

                Vector2[] path = MouseUtil.GetLinearPath(cursorPos, targetPos, mnavTarget.time - time, sysLatency);
                foreach (var pos in path) mouseMoveQueue.Add(pos);
                //pointsQueue.Add(targetPos);

                NextMnav();
            }

        }

    }
}
