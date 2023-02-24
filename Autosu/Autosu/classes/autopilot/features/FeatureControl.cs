using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        public static bool ProcessFeatureChange(string name, bool enable) {
            switch (name) {
                // main cmd
                case "cmd":
                    if (enable) return false;
                    if (status == EAutopilotMasterState.OFF && armState == EAutopilotArmState.NOT_ARMED) {
                        status = EAutopilotMasterState.ARM;
                        armState = EAutopilotArmState.START_LISTEN;

                        return true;
                    }
                    break;

            }

            return false;
        }

    }
}
