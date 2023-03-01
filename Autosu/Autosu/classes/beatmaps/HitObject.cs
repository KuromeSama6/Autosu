using Autosu.Enums;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Classes {

    [Serializable]
    public class HitObject {
        public Vector2 pos;
        public int time;
        public EHitObjectType type;

        public bool keyPressed = false;

    }

    [Serializable]
    public class SliderObject : HitObject {
        public ESliderCurveType curveType;
        public List<Vector2> points = new();
        public int repeats;
        public float length;

        public Vector2[] actualPoints {
            get {
                List<Vector2> ret = new();
                foreach (var item in points) {
                    ret.Add(APUtil.OsuPixelToScreen(item));
                }
                return ret.ToArray();
            }
        }

        // parse the slider curve
        // P|270:245|307:271
        public SliderObject(string sliderInfo) {
            string type = sliderInfo.Substring(0, 1);
            sliderInfo = sliderInfo.Substring(2);

            curveType = type switch {
                "B" => ESliderCurveType.BEZIER,
                "C" => ESliderCurveType.CCR,
                "L" => ESliderCurveType.LINEAR,
                "P" => ESliderCurveType.PERFECT,
                _ => ESliderCurveType.BEZIER
            };

            // start parsing
            foreach (string point in sliderInfo.Split("|")) {
                int x = int.Parse(point.Split(":")[0]);
                int y = int.Parse(point.Split(':')[1]);
                points.Add(new(x, y));
            }

        }

        public float GetDuration(Beatmap beatmap) {
            /// length / (SliderMultiplier * 100 * SV) * beatLength
            /// SliderMultipler comes from song difficulty
            /// 

            float sm = beatmap.sliderMultiplier;
            TimingSection timingSection = beatmap.GetTimingSection(time);
            float beatLength = timingSection.beatLength;
            float sv = timingSection.sliderSpeed;

            //Debug.WriteLine($"TimingPoint {timingSection.time}: SliderMultiplier={sm} BeatLength={beatLength} SV={sv}");

            return (length / (sm * 100f * sv) * beatLength);

        }

    }

    [Serializable]
    public class SpinnerObject : HitObject {
        public int endTime;

    }

    [Serializable]
    public class TimingSection {
        public int time;
        public float sliderSpeed;
        public float beatLength;
        public bool inherited;
    }

}
