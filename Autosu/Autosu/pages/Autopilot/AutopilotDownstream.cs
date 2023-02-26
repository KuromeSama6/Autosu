using CefSharp.WinForms;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autosu.Classes;
using Autosu.Utils;
using Autosu.classes;
using Newtonsoft.Json;
using WMPLib;
using Autosu.classes.autopilot;
using System.Xml.Linq;
using System.Numerics;
using System.Diagnostics;

namespace Autosu.Pages.Bot {
    public class AutopilotDownsream : Downstream {
        public AutopilotDownsream(ChromiumWebBrowser browser, Form form) : base(browser, form) { }
        public new AutopilotPage form => AutopilotPage.instance;

        public override void BrowserReady() {
            string path = Config.instance.osuPath;

        }

        public object InitAutopilot() {
            Beatmap bm = Autopilot.i.beatmap;

            // return the data to put into autopilot

            return new {
                bgPath = $@"{Path.GetDirectoryName(bm.path)}\{bm.bgFileName}",
                bmTitle = bm.title,
                bmDifficulty = bm.variation,
            };
        }

        public void FeatureControlChange(string name, bool enable) {
            Autopilot.i.ProcessFeatureChange(name, enable);
        }

        public void InputChange(string name, int value) {
            //Debug.WriteLine($"{name}, {value}");
            switch (name) {
                case "targetloc-offset": Autopilot.i.config.inputs.targetOffsetAmount = value; break;
                case "targetloc-thresh": Autopilot.i.config.inputs.targetOffsetThreshold = value; break;
            }
        }


        public object RequestAnnunciatorStatus() {
            return new {
                ap_main = Autopilot.i.status switch {
                    EAutopilotMasterState.ON => EAnnunciatorState.AMBER,
                    EAutopilotMasterState.FULL => EAnnunciatorState.GREEN,
                    _ => EAnnunciatorState.OFF
                },
                ap_arm = Autopilot.i.armState switch {
                    EAutopilotArmState.NOT_ARMED => EAnnunciatorState.OFF,
                    EAutopilotArmState.ARMED => EAnnunciatorState.GREEN,
                    _ => EAnnunciatorState.AMBER
                },
                ap_warn = Autopilot.i.status == EAutopilotMasterState.DISENGAGE_WARN,
                opmode_switch = Autopilot.i.config.features.autoSwitch ? Autopilot.i.isHumanInput ? EAnnunciatorState.GREEN : EAnnunciatorState.AMBER : EAnnunciatorState.OFF,
                acc = Autopilot.i.status == EAutopilotMasterState.FULL ? EAnnunciatorState.GREEN : EAnnunciatorState.OFF,
                mnav_desync = Autopilot.i.mnavDesyncWarn,
                hnav_fault = Autopilot.i.hnavFaultWarn,
                nav_mode = Autopilot.i.status == EAutopilotMasterState.FULL ? Autopilot.i.pathingMode : 0,
                mnav_queue = Autopilot.i.status == EAutopilotMasterState.FULL ? Autopilot.i.mouseMoveQueueFinished ? EAnnunciatorState.GREEN : EAnnunciatorState.AMBER : EAnnunciatorState.OFF,
            };
        }

        public void SuppressAnnunciator(string type) {
            switch (type) {
                case "mnav-desync":
                    Autopilot.i.mnavDesyncWarn = false;
                    break;
                case "hnav-fault":
                    Autopilot.i.hnavFaultWarn = false;
                    break;
                case "ap-warn":
                    if (Autopilot.i.status != EAutopilotMasterState.DISENGAGE_WARN) return;

                    if (Autopilot.i.apDisconnectPlayer != null) {
                        Autopilot.i.apDisconnectPlayer.Stop();
                        Autopilot.i.status = EAutopilotMasterState.OFF;
                    }
                    break;
            }
        }

        public object RequestCursorPosition() {
            Vector2 pos = MouseUtil.RelativeMousePosition(new(Cursor.Position.X, Cursor.Position.Y));
            return new { 
                x = pos.X,
                y = pos.Y
            };
        }

        public object GetNextObject() {
            if (Autopilot.i.status < EAutopilotMasterState.ON) return null;
            return new {
                time = Autopilot.i.navTarget.time - Autopilot.i.time,
                //extraData = Autopilot.i.abortCount,
                extraData = Autopilot.i.navTarget is SliderObject ? $"{Autopilot.i.navTarget.time} {((SliderObject) Autopilot.i.navTarget).type}" : Autopilot.i.abortCount.ToString(),
                //extraData = Autopilot.i.sysLatency,
                x = APUtil.OsuPixelToScreen(Autopilot.i.navTarget.pos).X,
                y = APUtil.OsuPixelToScreen(Autopilot.i.navTarget.pos).Y,
                xc = Cursor.Position.X,
                yc = Cursor.Position.Y,
                queueLength = Autopilot.i.mouseMoveQueue.Count,
                sysLatency = Autopilot.i.sysLatency,
            };
        }

        public string RequestTogglebtnStatus() {
            return JsonConvert.SerializeObject(new {
                features = Autopilot.i.config.features,
                cmdEnable = Autopilot.i.status > EAutopilotMasterState.DISENGAGE_WARN
            });
        }

        public string RequestInputValues() {
            return JsonConvert.SerializeObject(new {
                inputs = Autopilot.i.config.inputs,
                calib = -Autopilot.i.calibOffset,
                enableCalib = Autopilot.i.status == EAutopilotMasterState.FULL
            });
        }

        public object ReadProfile(string name) {
            bool suc = false;
            string msg = "";
            APConfig cfg = APConfig.Load(name.ToLower());

            if (Autopilot.i.status > EAutopilotMasterState.ARM) msg = "A/P LOCK";
            else if (cfg == null) msg = "NOT FOUND";

            else {
                Autopilot.i.config = cfg;
                suc = true;
            }

            return new {
                ok = suc,
                msg
            };
        }

        public object WriteProfile(string name) {
            string msg = "OK";
            name = name.ToLower();            
            string path = CommonUtil.ParsePath($@"userdata/profiles/{name.ToLower()}.aosu");

            if (File.Exists(path)) msg = "OK OVERWRITE";
            Autopilot.i.config.name = name;
            Autopilot.i.config.Save();

            return msg;
        }

        public void ReturnToMenu() {
            if (Autopilot.i.status > EAutopilotMasterState.OFF) Autopilot.i.Disengage();
            else form.ReturnToMenu();
        }

    }
}
