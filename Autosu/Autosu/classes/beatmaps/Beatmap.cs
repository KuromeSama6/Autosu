using Autosu.classes;
using Autosu.Classes;
using Autosu.Enums;
using Autosu.Exceptions;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Autosu.Classes {
    [Serializable]
    public class Beatmap {
        #region Reference Fields
        public string path;
        public List<string> content;

        #endregion

        #region Beatmap Info
        public EBeatmapMode mode;
        public string title;
        public string variation;
        public int previewStartTime;
        public string bgFileName;
        public string audioPath;
        public List<HitObject> objects;

        #endregion

        public static Beatmap GetOne(string title, string variation) {
            Beatmap? ret = null;
            ret = Config.instance.beatmaps.Find((bm) => bm.title == title && bm.variation == variation);

            if (ret == null) throw new BeatmapNotFoundException();
            else return ret;
        }

        public Beatmap(string path) {
            this.path = path;
            
            // open the beatmap file
            string file = File.ReadAllText(path);
            content = new(file.Split(Environment.NewLine));

            // load beatmap info
            var metadata = CommonUtil.SerializeStringSection(CommonUtil.ExtractStringSection(file, "[Metadata]"));
            var general = CommonUtil.SerializeStringSection(CommonUtil.ExtractStringSection(file, "[General]"));

            bgFileName = CommonUtil.ExtractStringSection(file, "[Events]")[2].Split(',')[2].Replace("\"", "");

            mode = (EBeatmapMode) int.Parse(general["Mode"]);
            previewStartTime = int.Parse(general["PreviewTime"]);
            audioPath = $@"{Path.GetDirectoryName(path)}\{general["AudioFilename"].Substring(1)}";

            title = metadata["Title"];
            variation = metadata["Version"];

        }

    }

}
