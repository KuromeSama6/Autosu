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
    public class SilderObject : HitObject {
        public ESliderCurveType curveType;
        public List<Vector2> points;
        public int repeats;

    }

    [Serializable]
    public class SpinnerObject : HitObject {
        public int endTime;

    }

}
