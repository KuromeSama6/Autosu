using Autosu.Classes;
using Autosu.Enums;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Autosu.classes {
    [Serializable]
    public class Config {
        [NonSerialized] public static Config instance = SerializationUtil.LoadOrNew<Config>(CommonUtil.ParsePath("userdata/config.aosu"));

        #region General Config
        public string osuPath = "";
        public string beatmapPath => $@"{osuPath}\Songs";

        #endregion

        [NonSerialized] public List<string> failedPaths = new();

        #region Dynamic Fields
        public List<Beatmap> beatmaps {
            get {
                if (_beatmapsCache != null) return _beatmapsCache;

                failedPaths = new();
                List<Beatmap> ret = new();
                foreach (var path in Directory.GetFiles(beatmapPath, "*.osu", SearchOption.AllDirectories)) {
                    try {
                        Beatmap bm = new Beatmap(path);
                        if (bm.mode == EBeatmapMode.STD) ret.Add(bm);
                    } catch (Exception e) {
                        // Get stack trace for the exception with source file information
                        var st = new StackTrace(e, true);
                        string lineMsg = "";

                        for (int i = 0; i < st.FrameCount; i++) lineMsg += $"{st.GetFrame(i).GetMethod()} @ line {st.GetFrame(i).GetFileLineNumber()}:";
                        Debug.WriteLine($"Failed to load {path}: {e.Message}: {lineMsg}");
                        failedPaths.Add(path);
                    }
                }
                _beatmapsCache = ret;
                

                return ret;
            }
        }
        [NonSerialized] private List<Beatmap> _beatmapsCache;

        #endregion

        public object songData {
            get {
                Dictionary<string, List<string>> titles = new();

                foreach (var beatmap in beatmaps) {
                    if (titles.ContainsKey(beatmap.title)) titles[beatmap.title].Add(beatmap.variation);
                    else titles[beatmap.title] = new List<string> { beatmap.variation };

                }

                return new { 
                    beatmaps = titles,
                    path = beatmapPath.Replace("\\", "\\\\")
                };
            }
        }

        public void FlushBeatmapCache() {
            _beatmapsCache = null;
        }

    }
}
