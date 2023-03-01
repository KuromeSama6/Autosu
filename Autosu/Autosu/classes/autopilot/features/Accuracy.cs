using Autosu.Classes;
using Autosu.Hooks;
using Autosu.Utils;
using Indieteur.GlobalHooks;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMPLib;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {

        public List<HitResult> results = new();

        public float currentAccuracy {
            get {
                if (results.Count == 0) return 100f;

                float ret = 0f;
                foreach (HitResult result in results.ToArray()) ret += result.result switch {
                    EHitResult.THREEHUNDRED => 100f,
                    EHitResult.ONEHUNDRED => 100f / 3f,
                    EHitResult.FIFTY => 100f / 6f,
                    _ => 0f
                };

                return ret / results.Count;
            }
        }

        public float lastAccuracyRandom = 0f;

        public EAnnunciatorState accuracyAnnunciatorState {
            get {
                float maxAcc = Math.Min(100f, config.inputs.minimumAcc + 7f * 0.25f);
                if (!config.features.accuracySelect) return EAnnunciatorState.OFF;
                else if (currentAccuracy < config.inputs.minimumAcc) return EAnnunciatorState.AMBER;
                else if (currentAccuracy > maxAcc) return EAnnunciatorState.AMBER;
                else return EAnnunciatorState.GREEN;
            }
        }

        public bool accuracyLock = false;

        private void AccuracyUpdate() {
            lastAccuracyRandom = 0f;

            // return if not in buffer
            if (!config.features.accuracySelect) return;

            float maxAcc = Math.Min(100f, config.inputs.minimumAcc + 7f);
            float minAcc = config.inputs.minimumAcc;
            float targetAcc = (maxAcc + minAcc) / 2f;

            // start measuring target acc

            if ((currentAccuracy < maxAcc || playheadTime > 15000) && !accuracyLock) accuracyLock = true;
            if (currentAccuracy > maxAcc && accuracyLock && config.features.playDetent) {
                Disengage();
                return;
            }

            if (Math.Abs(currentAccuracy - targetAcc) > 0.5f) {
                if (currentAccuracy < targetAcc) {
                    lastAccuracyRandom = new Random().Next(0, beatmap.hitWindowBoundary300 / 2);
                    lastAccuracyRandom *= new Random().Next(0, 2) == 0 ? 1 : -1;
                } else if (currentAccuracy > targetAcc && accuracyLock) {
                    lastAccuracyRandom = new Random().Next(beatmap.hitWindowBoundary300 - 4, beatmap.hitWindowBoundary100 + 5);
                    lastAccuracyRandom *= new Random().Next(0, 2) == 0 ? 1 : -1;

                } else lastAccuracyRandom = new Random().Next(0, beatmap.hitWindowBoundary300 - 2);
            }

        }

        public void PadAccuracy(bool goesUp) {
            if (status < EAutopilotMasterState.FULL) return;

            if (goesUp) for (int i = 0; i < 8; i++) results.Add(new(0));
            else for (int i = 0; i < 1; i++) results.Add(new(beatmap.hitWindowBoundary100 + 2));

        }

    }
}
