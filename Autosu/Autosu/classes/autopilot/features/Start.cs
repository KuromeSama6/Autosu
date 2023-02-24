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
using System.Windows.Forms;
using WMPLib;

namespace Autosu.classes.autopilot {
    public partial class Autopilot {
        // Start Song
        public static void StartSong() {
            // delay is 178 frames @ 60fps
            CommonUtil.DelayedCall(() => {
                /*testPlayer = new WindowsMediaPlayer();
                testPlayer.URL = beatmap.audioPath;
                testPlayer.settings.volume = 5;
                testPlayer.controls.play();*/

                status = EAutopilotMasterState.ON;
                if (AutopilotPage.instance.visible) AutopilotPage.instance.visible = true;
                AutopilotPage.instance.SetOverlay(true, false);
                playhead.Start();

                /*Vector2[] path = MouseUtil.GetLinearPath(new(Cursor.Position.X, Cursor.Position.Y), APUtil.OsuPixelToScreen(navTarget.pos), new Random().Next(200, 500));
                foreach (var pos in path) pointsQueue.Add(pos);*/

            }, 1f / 60f * 151f);

            APUtil.PlayAnnunciatorAlert();

            // deep copy hitobjects to navqueue - populate navqueue
            mnavQueue.Clear();
            mnavQueue = new(beatmap.objects.ToArray());
        }

    }
}       
