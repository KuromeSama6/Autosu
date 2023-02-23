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

        public bool FeatureControlChanged(string name, bool enable) {

            return true;
        }
        
        public object RequestCursorPosition() {
            Vector2 pos = MouseUtil.RelativeMousePosition(new(Cursor.Position.X, Cursor.Position.Y));
            return new { 
                x = pos.X,
                y = pos.Y
            };
        }

        public void ReturnToMenu() {
            form.ReturnToMenu();
        }

    }
}
