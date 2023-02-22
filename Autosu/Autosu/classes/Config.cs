using Autosu.Classes;
using Autosu.Enums;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.classes {
    [Serializable]
    public class Config {
        [NonSerialized] public static Config instance = SerializationUtil.LoadOrNew<Config>(CommonUtil.ParsePath("userdata/config.aosu"));

        #region General Config
        public string beatmapPath = "";

        #endregion

        #region Dynamic Fields
        public List<Beatmap> beatmaps {
            get {
                if (_beatmapsCache != null) return _beatmapsCache;

                List<Beatmap> ret = new();
                try {
                    foreach (var path in Directory.GetFiles(beatmapPath, "*.osu", SearchOption.AllDirectories)) {
                        Beatmap bm = new Beatmap(path);
                        if (bm.mode == EBeatmapMode.STD) ret.Add(bm);
                    }
                    _beatmapsCache = ret;
                } catch { }

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

    }
}
