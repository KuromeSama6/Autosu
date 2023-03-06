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
        
        private int cooldown = 0;
        public EOpMode opMode => config.features.autoSwitch && cooldown > 0 ? EOpMode.MANUAL : EOpMode.AUTO;

        public void ReportManualMouseMove() { 
        
        }

    }
}
