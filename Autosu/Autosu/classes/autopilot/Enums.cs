using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.classes.autopilot {
    public enum EAutopilotMasterState {
        OFF,
        DISENGAGE_WARN,
        ARM,
        ON,
        FULL
    }

    public enum EAutopilotArmState {
        NOT_ARMED,
        START_LISTEN,
        MOUSE_LISTEN,
        ARMED
    }
    
    public enum EAnnunciatorState {
        OFF,
        AMBER,
        GREEN
    }

    public enum EPathingControlMode {
        NONE,
        MOUSE,
        KEYBOARD
    }

}
