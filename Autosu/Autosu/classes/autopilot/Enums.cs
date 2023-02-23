using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.classes.autopilot {
    public enum EAutopilotMasterState {
        OFF,
        DISENGAGE_WARN,
        ARMED,
        ON
    }

    public enum EAutopilotArmState {
        NOT_ARMED,
        START_LISTEN,
        MOUSE_LISTEN,
        ARMED
    }

}
