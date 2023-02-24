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

namespace Autosu.Pages.Bot {
    public class AutopilotDownsream : Downstream {
        public AutopilotDownsream(ChromiumWebBrowser browser, Form form) : base(browser, form) { }
        public new AutopilotPage form => AutopilotPage.instance;

        public override void BrowserReady() {
            string path = Config.instance.osuPath;

        }

        public object InitAutopilot() {
            Beatmap bm = Autopilot.beatmap;

            // return the data to put into autopilot

            return new {
                bgPath = $@"{Path.GetDirectoryName(bm.path)}\{bm.bgFileName}",
                bmTitle = bm.title,
                bmDifficulty = bm.variation,
            };
        }

        public bool FeatureControlChange(string name, bool enable) {
            return Autopilot.ProcessFeatureChange(name, enable);
        }
        
        public object RequestAnnunciatorStatus() {
            return new {
                ap_main = Autopilot.status switch {
                    EAutopilotMasterState.ON => EAnnunciatorState.AMBER,
                    EAutopilotMasterState.FULL => EAnnunciatorState.GREEN,
                    _ => EAnnunciatorState.OFF
                },
                ap_arm = Autopilot.armState switch {
                    EAutopilotArmState.NOT_ARMED => EAnnunciatorState.OFF,
                    EAutopilotArmState.ARMED => EAnnunciatorState.GREEN,
                    _ => EAnnunciatorState.AMBER
                },
                ap_warn = Autopilot.status == EAutopilotMasterState.DISENGAGE_WARN,
                opmode_switch = Autopilot.config.features.autoSwitch ? Autopilot.isHumanInput ? EAnnunciatorState.GREEN : EAnnunciatorState.AMBER : EAnnunciatorState.OFF,
                acc = Autopilot.status == EAutopilotMasterState.FULL ? EAnnunciatorState.GREEN : EAnnunciatorState.OFF,
                mnav_desync = Autopilot.mnavDesyncWarn,
            };
        }

        public void SuppressAnnunciator(string type) {
            switch (type) {
                case "mnav-desync":
                    Autopilot.mnavDesyncWarn = false;
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
            if (Autopilot.status < EAutopilotMasterState.ON) return null;
            return new {
                time = Autopilot.mnavTarget.time - Autopilot.time,
                x = APUtil.OsuPixelToScreen(Autopilot.mnavTarget.pos).X,
                y = APUtil.OsuPixelToScreen(Autopilot.mnavTarget.pos).Y,
                xc = Cursor.Position.X,
                yc = Cursor.Position.Y,
                queueLength = Autopilot.mouseMoveQueue.Count,
                sysLatency = Autopilot.sysLatency,
            };
        }

        public void ReturnToMenu() {
            form.ReturnToMenu();
        }

    }
}
