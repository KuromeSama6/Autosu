using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public void ProcessFeatureChange(string name, bool enable) {
            switch (name) {
                // main cmd
                case "cmd":
                    if (enable || status == EAutopilotMasterState.DISENGAGE_WARN) return;
                    if (status == EAutopilotMasterState.OFF && armState == EAutopilotArmState.NOT_ARMED) {
                        status = EAutopilotMasterState.ARM;
                        armState = EAutopilotArmState.START_LISTEN;

                        return;
                    }
                    break;

                case "n1":
                    if (status <= EAutopilotMasterState.ARM) config.features.n1 = !config.features.n1;
                    break;

                case "mnav": config.features.mnav = !config.features.mnav; break;
                case "hnav": config.features.hnav = !config.features.hnav; break;
                case "targetoffset": config.features.targetOffset = !config.features.targetOffset; break;

                case "spinoffset":
                    config.features.spinnerOffset = !config.features.spinnerOffset;
                    if (!config.features.spinnerOffset) config.features.spinnerRandom = false;
                    break;

                case "spinrandom": config.features.spinnerRandom = !config.features.spinnerRandom && config.features.spinnerOffset; break;
                case "sliderhalt": config.features.shortSliderHalt = !config.features.shortSliderHalt; break;


            }
        }

    }
}
