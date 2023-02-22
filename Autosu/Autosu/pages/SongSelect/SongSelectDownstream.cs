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

namespace Autosu.Pages.SongSelect {
    public class SongSelectDownstream : Downstream {
        public SongSelectDownstream(ChromiumWebBrowser browser, Form form) : base(browser, form) { }
        public new SongSelectPage form => SongSelectPage.instance;

        public WindowsMediaPlayer currentPreviewAudio;

        public string TestReturn(string hwid) {
            return "";
        }

        public override void BrowserReady() {
            string path = Config.instance.beatmapPath;
            if (Directory.Exists(path)) browser.ExecuteScriptAsync($"selectFiles(`{JsonConvert.SerializeObject(Config.instance.songData)}`, true)");
        }

        public object SelectBeatmapDirectory(string path) {
            bool exists = Directory.Exists(path);
            if (!exists) return null;

            // write to cfg
            Config.instance.beatmapPath = path;
            SerializationUtil.Save<Config>(CommonUtil.ParsePath("userdata/config.aosu"), Config.instance);
            return JsonConvert.SerializeObject(Config.instance.songData);

        }

        public object LoadBeatmapPreview(string title, string variation) {
            Beatmap? bm = Config.instance.beatmaps.Find(candidate => candidate.title == title && candidate.variation == variation);
            if (bm == null) return null;

            // start processing
            string? bmDir = Path.GetDirectoryName(bm.path);
            string bgName = "";
            try {
                // get the path of the background image
                bgName = bm.bgFileName;

            } catch { }

            if (currentPreviewAudio != null) currentPreviewAudio.close();
            currentPreviewAudio = new WindowsMediaPlayer();
            currentPreviewAudio.URL = bm.audioPath;
            currentPreviewAudio.controls.currentPosition = bm.previewStartTime / 1000f;
            currentPreviewAudio.settings.volume = 5;
            currentPreviewAudio.controls.play();

            return new {
                bgPath = $@"{bmDir}\{bgName}",
            };

        }

    }
}
