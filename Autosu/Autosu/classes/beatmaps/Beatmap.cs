using Autosu.classes;
using Autosu.Classes;
using Autosu.Enums;
using Autosu.Exceptions;
using Autosu.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
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
        public int overallDifficulty;
        public int previewStartTime;
        public string bgFileName;
        public string audioPath;
        public List<HitObject> objects = new();

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
            var difficulty = CommonUtil.SerializeStringSection(CommonUtil.ExtractStringSection(file, "[Difficulty]"));

            bgFileName = CommonUtil.ExtractStringSection(file, "[Events]")[2].Split(',')[2].Replace("\"", "");

            mode = (EBeatmapMode) int.Parse(general["Mode"]);
            previewStartTime = int.Parse(general["PreviewTime"]);
            audioPath = $@"{Path.GetDirectoryName(path)}\{general["AudioFilename"].Substring(1)}";

            overallDifficulty = (int) (float.Parse(difficulty["OverallDifficulty"]) * 10);

            title = metadata["Title"];
            variation = metadata["Version"];

            // load hitobjects
            var hitObjects = CommonUtil.ExtractStringSection(file, "[HitObjects]");
            foreach (var hitObject in hitObjects.GetRange(1, hitObjects.Count - 1)) {
                string[] args = hitObject.Split(',');

                /*1: Circle
                2: Slider
                5: Circle(New Combo)
                6: Slider(New Combo)
                12: Spinner*/
                /*420,56,34096,6,0,L|395:231,1,160,4|2,3:0|0:0,0:0:0:0:
                320,192,34704,1,2,0:0:0:0:
                244,165,34906,2,0,L | 69:190,1,160,8 | 2,0:0 | 0:0,0:0:0:0:*/
                int x = int.Parse(args[0]);
                int y = int.Parse(args[1]);
                int time = int.Parse(args[2]);
                int type = int.Parse(args[3]);
                
                switch (type) {
                    // regular circle
                    //{"pos":{"X":254.0,"Y":271.0},"time":9408,"type":0}
                    case 1: case 5:
                        objects.Add(new HitObject() {
                            pos = new Vector2(x, y),
                            time = time,
                            type = EHitObjectType.CIRCLE
                        }); break;

                    // slider
                    case 2: case 6:
                        string sliderInfo = args[5];

                        objects.Add(new SliderObject(sliderInfo) {
                            pos = new Vector2(x, y),
                            time = time,
                            repeats = int.Parse(args[6]),
                            type = EHitObjectType.SLIDER
                        }); break;

                    // spinner
                    case 12:
                        objects.Add(new SpinnerObject() {
                            pos = new Vector2(x, y),
                            time = time,
                            type = EHitObjectType.SPINNER,
                            endTime = int.Parse(args[5])
                        }); break;

                }

            }

        }

    }

}
