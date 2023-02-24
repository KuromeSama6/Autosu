using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Enums {
    public enum EHitObjectType {
        CIRCLE,
        SLIDER,
        SPINNER
    }

    public enum ESliderCurveType {
        BEZIER,
        CCR,
        LINEAR,
        PERFECT

    }

    public enum EBeatmapMode {
        STD = 0,
        TAIKO = 1,
        CATCH = 2,
        MANIA = 3
    }

}
