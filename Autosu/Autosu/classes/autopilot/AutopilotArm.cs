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
        public static EAutopilotArmState armState { get; private set; }
        private static WindowsMediaPlayer testPlayer;

        public static void Arm() {
            if (armState == EAutopilotArmState.START_LISTEN) {
                Vector2 pos = MouseUtil.RelativeMousePosition(new Vector2(Cursor.Position.X, Cursor.Position.Y));
                if (!(pos.X > 0.80f && pos.Y > 0.80f)) return;

                //APUtil.PlayAnnunciatorAlert();
                StartSong();

                // test
                if (testPlayer == null) {
                    // delay is 178 frames @ 60fps
                    CommonUtil.DelayedCall(() => {
                        testPlayer = new WindowsMediaPlayer();
                        testPlayer.URL = beatmap.audioPath;
                        testPlayer.settings.volume = 5;
                        testPlayer.controls.play();

                        AutopilotPage.instance.SetOverlay(true, false);
                        if (AutopilotPage.instance.visible) AutopilotPage.instance.visible = true;

                    }, 1f / 60f * 151f);
                }

                //APUtil.PlayAnnunciatorAlert();

                armState = EAutopilotArmState.ARMED;
                status = EAutopilotMasterState.ON;
                return;
            }

        }

        private static void ArmUpdate() {
        
        }
    }
}
