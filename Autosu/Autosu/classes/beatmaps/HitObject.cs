using Autosu.Enums;
using System;
using System.Collections.Generic;
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

    }

    [Serializable]
    public class SliderObject : HitObject {
        public ESliderCurveType curveType;
        public List<Vector2> points = new();
        public int repeats;

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

    }

    [Serializable]
    public class SpinnerObject : HitObject {
        public int endTime;

    }

}
