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
        public static List<HitObject> hnavQueue = new();
        public static HitObject hnavTarget => hnavQueue[0];

        /// <summary>
        /// Aborts navigation to the current navTarget and removes it from navQueue.
        /// </summary>
        /// <returns>A HitObject representing the HitObject that had been just removed.</returns>
        public static HitObject NextHnav() {
            HitObject ret = hnavTarget;
            hnavQueue.RemoveAt(0);
            return ret;
        }

        /// <summary>
        /// Shorthand to skip multiple HitObjects.
        /// </summary>
        /// <param name="count">Amount of HitObjects to skip</param>
        /// <returns>A HitObject[] containing all HitObjects that has been skipped.</returns>
        public static HitObject[] NextHnav(int count) {
            List<HitObject> ret = new();
            for (int i = 0; i < count; i++) ret.Add(NextHnav());
            return ret.ToArray();

        }

    }
}
