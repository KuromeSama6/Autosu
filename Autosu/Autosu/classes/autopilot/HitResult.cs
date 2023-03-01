using Autosu.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Autosu.classes.autopilot {
    public class HitResult {

        public int realDelay;
        public int delay => Math.Abs(realDelay);

        public EHitResult result {
            get {
                if (Autopilot.i == null) return EHitResult.THREEHUNDRED;
                Beatmap beatmap = Autopilot.i.beatmap;
                if (beatmap == null) return EHitResult.THREEHUNDRED;

                // calib offset: + is late, - is early

                if (delay < beatmap.hitWindowBoundary300) return EHitResult.THREEHUNDRED;
                else if (delay < beatmap.hitWindowBoundary100) return EHitResult.ONEHUNDRED;
                else if (delay < beatmap.hitWindowBoundary50) return EHitResult.FIFTY;
                else return EHitResult.BAD;
            }
        }

        public HitResult(int delay) {
            this.realDelay = delay;

        }

    }
}
