using Autosu.Classes;
using Autosu.Hooks;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMPLib;
using Indieteur.GlobalHooks;
using System.Runtime.InteropServices;
using System.Numerics;
using CefSharp;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public static Beatmap beatmap;
        public static APConfig config = new();

        #region Fields - Threading and Timing related
        private static Stopwatch playhead = new();
        public static int time => (int) playhead.ElapsedMilliseconds;
        private static Thread thread;
        private static readonly Stopwatch cycleTimer = new();
        private static System.Threading.Timer threadTimer;
        private static long nextUpdateTime;
        private static bool keepGoing = true;
        private static HighAccuracyTimer highAccuracyTimer = new(NavMouseUpdate, 1);
        public static Stopwatch sysLatencyTimer = new();
        public static int sysLatency { get; private set; }
        #endregion

        #region Fields - Status Related
        public static EAutopilotMasterState status { get; private set; }
        #endregion

        #region Fields - Misc
        [DllImport("user32.dll")]
        static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, out int lpnLengthNeeded);
        #endregion

        public static void Init(Beatmap beatmap) {
            Autopilot.beatmap = beatmap;
            keepGoing = true;

            // start the main cycle
            thread = new Thread(new ThreadStart(() => {
                nextUpdateTime = cycleTimer.ElapsedMilliseconds;
                threadTimer = new System.Threading.Timer((object? _) => {
                    if (!keepGoing) {
                        threadTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        return;
                    }

                    nextUpdateTime += 1;

                    Update();

                    while (cycleTimer.ElapsedMilliseconds < nextUpdateTime) {
                        Thread.Sleep(0);
                    }

                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            }));
            cycleTimer.Start();
            thread.Start();

            /*globalKeyHook.OnKeyDown += (object sender, GlobalKeyEventArgs e) => {
                MessageBox.Show("press");
            };*/

        }

        // Main autopilot cycle
        // Time: 1ms
        public static void Update() {
            if (AutopilotPage.instance == null) return;

            // sys latency
            sysLatencyTimer.Stop();
            sysLatency = (int)sysLatencyTimer.ElapsedMilliseconds;
            sysLatencyTimer.Restart();

            switch (status) {
                case EAutopilotMasterState.ON:
                    ArmUpdate();
                    break;
            }

            // nav update
            if (status >= EAutopilotMasterState.ON) {
                MnavUpdate();
            }

            // check for game start
            Process[] procs = Process.GetProcessesByName("osu!");
            if (!AutopilotPage.gameHasLaunched && procs.Length == 1) {
                Process proc = procs[0];
                AutopilotPage.instance.Invoke(() => {
                    AutopilotPage.instance.SetOverlay(true, false);

                    AutopilotPage.gameHasLaunched = true;
                });
                AutopilotPage.gameHasLaunched = true;

            } else if (AutopilotPage.gameHasLaunched && procs.Length != 1) {
                AutopilotPage.instance.Invoke(() => {
                    AutopilotPage.instance.SetOverlay(false, true);
                    AutopilotPage.instance.visible = true;

                    AutopilotPage.gameHasLaunched = false;
                });
            }

        }


        public static void Disengage(bool silent = false) {

        }

        public static void Dispose() {
            highAccuracyTimer.Stop();
            keepGoing = false;
            threadTimer.Dispose();
            thread.Join();
        }
    }
}
