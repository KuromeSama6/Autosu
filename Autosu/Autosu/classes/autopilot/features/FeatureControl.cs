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
                    if (enable) return;
                    if (status == EAutopilotMasterState.OFF && armState == EAutopilotArmState.NOT_ARMED) {
                        status = EAutopilotMasterState.ARM;
                        armState = EAutopilotArmState.START_LISTEN;

                        return;
                    }
                    break;

                case "n1":
                    if (status <= EAutopilotMasterState.ARM) config.features.n1 = !config.features.n1;
                    break;

            }
        }

    }
}
