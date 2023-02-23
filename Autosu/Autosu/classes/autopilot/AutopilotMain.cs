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

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public static Beatmap beatmap;

        #region Fields - Threading and Timing related
        public static Thread thread;
        public static readonly Stopwatch cycleTimer = new();
        public static System.Threading.Timer threadTimer;
        public static long nextUpdateTime;
        public static bool keepGoing { get; private set; }
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

            status = EAutopilotMasterState.ARMED;

            /*globalKeyHook.OnKeyDown += (object sender, GlobalKeyEventArgs e) => {
                MessageBox.Show("press");
            };*/

        }

        // Start Song
        public static void StartSong() {
            var audio = new WindowsMediaPlayer();
            audio.URL = CommonUtil.ParsePath("resources/audio/alert.wav");
            audio.settings.volume = 5;
            audio.controls.play();
        }

        // Main autopilot cycle
        // Time: 1ms
        public static void Update() {
            if (AutopilotPage.instance == null) return;

            switch (status) {
                case EAutopilotMasterState.ARMED:
                    break;
            }
            

            // check for game start
            if (!AutopilotPage.gameHasLaunched) {
                Process[] procs = Process.GetProcessesByName("osu!");
                if (procs.Length == 1) {
                    Process proc = procs[0];
                    AutopilotPage.instance.Invoke(() => {
                        AutopilotPage.instance.SetOverlay(true, true);

                        AutopilotPage.gameHasLaunched = true;
                    });
                    AutopilotPage.gameHasLaunched = true;
                }
            }

        }

        public static void Disengage(bool silent = false) {

        }

        public static void Dispose() {
            threadTimer.Dispose();
            thread.Join();
        }
    }
}
