using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Enums {
    public enum EHitObjectType {
        CIRCLE = 0,
        SLIDER = 1,
        SPINNER = 3,
        NEWCOMBO = 2,
        COMBOSKIP1 = 4,
        COMBOSKIP2 = 5,
        COMBOSKIP3 = 6,
        OSUMANIA_HOLD = 7
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
