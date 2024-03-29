﻿using Autosu.Classes;
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
        public int calibOffset = 0;

        public void TryN1Calib() {
            if (status < EAutopilotMasterState.ON) return;

            PressHitKey();
            if (status == EAutopilotMasterState.ON) status = EAutopilotMasterState.FULL;

            if (!n1Init && config.features.n1) {
                lastMovePos = Cursor.Position;
                playhead.Start();
                APUtil.PlayAnnunciatorAlert();
                if (AutopilotPage.instance.visible) AutopilotPage.instance.visible = true;
                AutopilotPage.instance.SetOverlay(true, false);
            }

            // 15ms sysclock delay, offset by adding time
            int currentTime = firstNote.time;
            n1Offset = currentTime - time;

            n1Init = true;
        }

        public void TryRegularCalib(bool laterDirection) {
            if (status != EAutopilotMasterState.FULL) return;

            int offset = 2 * (laterDirection ? -1 : 1);
            calibOffset += offset;

            if (playheadTime < 15000) {
                foreach (var result in results) {
                    result.realDelay += offset;
                }
            }

        }

    }
}
