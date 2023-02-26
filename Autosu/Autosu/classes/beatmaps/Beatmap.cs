using Autosu.classes;
using Autosu.Classes;
using Autosu.Enums;
using Autosu.Exceptions;
using Autosu.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;
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
        public float circleSize;
        public float sliderMultiplier;
        public List<TimingSection> timings = new();

        #endregion

        public float realCircleRadius => APUtil.OsuPixelDistance(54.5f - 4.48f * circleSize);

        public static Beatmap GetOne(string title, string variation) {
            Beatmap? ret = null;
            ret = Config.instance.beatmaps.Find((bm) => bm.title == title && bm.variation == variation);

            if (ret == null) throw new BeatmapNotFoundException();
            else return ret;
        }

        public TimingSection? GetTimingSection(int positionInMs) {
            TimingSection section = null;
            for (int i = 0; i < timings.Count; i++) {
                if (timings[i].time <= positionInMs && (i == timings.Count - 1 || timings[i + 1].time > positionInMs)) {
                    section = timings[i];
                    break;
                }
            }
            return section;
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
            circleSize = float.Parse(difficulty["CircleSize"]);
            sliderMultiplier = float.Parse(difficulty["SliderMultiplier"]);

            title = metadata["Title"];
            variation = metadata["Version"];
            // load hitobjects
            var hitObjects = CommonUtil.ExtractStringSection(file, "[HitObjects]");
            foreach (var hitObject in hitObjects.GetRange(1, hitObjects.Count - 1)) {
                string[] args = hitObject.Split(',');
                //Debug.WriteLine($"{path} ({args[0]})");

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
                string type = args[3];

                if (type == "12") objects.Add(new SpinnerObject() {
                    pos = new Vector2(x, y),
                    time = time,
                    type = EHitObjectType.SPINNER,
                    endTime = int.Parse(args[5])
                });
                else if (args[5].Contains("|")) objects.Add(new SliderObject(args[5]) {
                    pos = new Vector2(x, y),
                    time = time,
                    repeats = int.Parse(args[6]),
                    type = EHitObjectType.SLIDER,
                    length = float.Parse(args[7])
                });
                else objects.Add(new HitObject() {
                    pos = new Vector2(x, y),
                    time = time,
                    type = EHitObjectType.CIRCLE
                });

            }

            var timings = CommonUtil.ExtractStringSection(file, "[TimingPoints]");
            TimingSection? inheritanceParent = null;
            foreach (var timing in timings.GetRange(1, timings.Count - 1)) {
                string[] args = timing.Split(',');
                //Debug.WriteLine($"{args[0]}: {path}");

                //42,405.405405405405,4,2,1,60,1,0
                int time = (int) Math.Round(float.Parse(args[0]));
                float beatLength = float.Parse(args[1]);
                bool inherited = args[6] == "0";

                if (inherited && inheritanceParent == null) throw new NothingToInheritException($"{path}: First TimingPoint of a beatmap cannot be inherited.");
                TimingSection obj = new() {
                    time = time,
                    beatLength = beatLength,
                    inherited = inherited
                };

                if (inherited) {
                    obj.sliderSpeed = -100f / beatLength;
                    obj.beatLength = inheritanceParent.beatLength;
                } else {
                    // uninherited
                    obj.sliderSpeed = 1f;
                    inheritanceParent = obj;
                }

                this.timings.Add(obj);

            }

            //File.WriteAllText(CommonUtil.ParsePath($"userdata/debug/{title}-{variation}.aosbmp"), JsonConvert.SerializeObject(this.objects));

        }

    }

}
