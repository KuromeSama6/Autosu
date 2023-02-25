using Autosu.Classes;
using Autosu.Utils;
using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        private bool n1Init = false;
        private int n1Offset = 0;

        public void TryCalib() {
            if (status < EAutopilotMasterState.ON) return;

            PressHitKey();
            if (status == EAutopilotMasterState.ON) status = EAutopilotMasterState.FULL;

            if (!n1Init) {
                APUtil.PlayAnnunciatorAlert();
            }

            // 15ms sysclock delay, offset by adding time
            int currentTime = firstNote.time + 50;
            n1Offset = currentTime - time;

            n1Init = true;
        }


    }
}
