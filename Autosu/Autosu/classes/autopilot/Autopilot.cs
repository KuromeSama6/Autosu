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
using System.Media;
using Newtonsoft.Json;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public static Autopilot i = new();

        public Beatmap beatmap;
        public APConfig config = new();

        #region Fields - Threading and Timing related
        private Stopwatch playhead = new();
        public int time => (int) playhead.ElapsedMilliseconds + n1Offset + calibOffset;
        private HighAccuracyTimer highAccuracyTimer;
        private HighAccuracyTimer normalCycleTimer;
        private Thread thread;
        private readonly Stopwatch cycleTimer = new();
        private System.Threading.Timer threadTimer;
        private long nextUpdateTime;
        private bool keepGoing = true;
        public Stopwatch sysLatencyTimer = new();
        public int sysLatency { get; private set; }
        public SoundPlayer apDisconnectPlayer = new (CommonUtil.ParsePath("resources/audio/ap_disconnect.wav"));
        #endregion

        #region Fields - Status Related
        public EAutopilotMasterState status;
        public HitObject firstNote;
        #endregion

        #region Fields - Misc
        [DllImport("user32.dll")]
        static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, out int lpnLengthNeeded);
        #endregion

        public Autopilot() {
            highAccuracyTimer = new(NavMouseUpdate, 1);
            normalCycleTimer = new(Update, 1);

            // start the main cycle
            thread = new Thread(new ThreadStart(() => {
                nextUpdateTime = cycleTimer.ElapsedMilliseconds;
                threadTimer = new System.Threading.Timer((object? _) => {
                    if (!keepGoing) {
                        threadTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        return;
                    }

                    nextUpdateTime += 1;

                    //Update();

                    while (cycleTimer.ElapsedMilliseconds < nextUpdateTime) {
                        Thread.Sleep(0);
                    }

                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            }));
            cycleTimer.Start();
            thread.Start();

        }

        public void Init(Beatmap beatmap) {
            this.beatmap = beatmap;
            keepGoing = true;

            firstNote = beatmap.objects.ToArray()[0];

        }

        // Main autopilot cycle
        // Time: 1ms
        public void Update() {
            if (AutopilotPage.instance == null) return;

            // sys latency
            /*sysLatencyTimer.Stop();
            sysLatency = (int)sysLatencyTimer.ElapsedMilliseconds;
            sysLatencyTimer.Restart();*/

            switch (status) {
                case EAutopilotMasterState.ON:
                    ArmUpdate();
                    break;
            }

            // nav update
            if (status >= EAutopilotMasterState.ON) {
                NavUpdate();
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


        public void Disengage(bool silent = false) {
            apDisconnectPlayer.Stop();
            status = EAutopilotMasterState.OFF;
            Dispose();

            // reset autopilot instance
            i = new();
            i.Init(Beatmap.GetOne(beatmap.title, beatmap.variation));
            i.config = config;

            // sync warnings
            i.hnavFaultWarn = hnavFaultWarn;
            i.mnavDesyncWarn = mnavDesyncWarn;
            AutopilotPage.instance.Invoke(() => AutopilotPage.instance.SetOverlay(true, true));


            if (!silent) {
                i.status = EAutopilotMasterState.DISENGAGE_WARN;
                apDisconnectPlayer.PlayLooping();
            } else {
                AutopilotPage.instance.visible = true;
            }

        }

        public void Dispose() {
            highAccuracyTimer.Stop();
            keepGoing = false;
            cycleTimer.Stop();
            playhead.Stop();
            highAccuracyTimer.Stop();
            normalCycleTimer.Stop();
            sysLatencyTimer.Stop();
            threadTimer.Dispose();
            apDisconnectPlayer.Dispose();
            thread.Join();
        }
    }
}
