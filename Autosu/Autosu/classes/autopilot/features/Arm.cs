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
        public EAutopilotArmState armState { get; private set; }
        private WindowsMediaPlayer testPlayer;

        public void Arm() {
            if (armState == EAutopilotArmState.START_LISTEN) {
                Vector2 pos = MouseUtil.RelativeMousePosition(new Vector2(Cursor.Position.X, Cursor.Position.Y));
                if (!(pos.X > 0.80f && pos.Y > 0.80f)) return;
                StartSong();

                //APUtil.PlayAnnunciatorAlert();

                armState = EAutopilotArmState.ARMED;
                return;
            }

        }

        private void ArmUpdate() {

        }
    }
}
