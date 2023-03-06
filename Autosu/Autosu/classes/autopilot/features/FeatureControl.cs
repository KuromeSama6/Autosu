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

                case "delayhit":
                    config.features.hitDelay = !config.features.hitDelay;
                    if (config.features.hitDelay) config.features.accuracySelect = false;
                    break;

                case "delaymove":
                    config.features.moveDelay = !config.features.moveDelay;
                    if (config.features.moveDelay) config.features.accuracySelect = false;
                    break;

                case "accsel":
                    config.features.accuracySelect = !config.features.accuracySelect;
                    if (config.features.accuracySelect) {
                        config.features.moveDelay = false;
                        config.features.hitDelay = false;
                    }
                    break;

                case "hr": if (status <= EAutopilotMasterState.ARM) config.features.hardrock = !config.features.hardrock; break;
                case "dt": if (status <= EAutopilotMasterState.ARM) config.features.doubletime = !config.features.doubletime; break;

            }
        }

    }
}
