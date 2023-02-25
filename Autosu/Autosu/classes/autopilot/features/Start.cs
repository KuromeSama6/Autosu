using Autosu.Classes;
using Autosu.Hooks;
using Autosu.Utils;
using Indieteur.GlobalHooks;
using Newtonsoft.Json;
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
        public void StartSong() {
            // delay is 178 frames @ 60fps
            CommonUtil.DelayedCall(() => {
                /*testPlayer = new WindowsMediaPlayer();
                testPlayer.URL = beatmap.audioPath;
                testPlayer.settings.volume = 5;
                testPlayer.controls.play();*/

                status = config.features.n1 ? EAutopilotMasterState.ON : EAutopilotMasterState.FULL;
                if (AutopilotPage.instance.visible) AutopilotPage.instance.visible = true;
                AutopilotPage.instance.SetOverlay(true, false);
                playhead.Start();

                /*Vector2[] path = MouseUtil.GetLinearPath(new(Cursor.Position.X, Cursor.Position.Y), APUtil.OsuPixelToScreen(navTarget.pos), new Random().Next(200, 500));
                foreach (var pos in path) pointsQueue.Add(pos);*/

            }, 1f / 60f * 151f);

            APUtil.PlayAnnunciatorAlert();

            // deep copy hitobjects to navqueue - populate navqueue
            navQueue.Clear();
            navQueue = new(beatmap.objects.ToArray());

            /*TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int) t.TotalSeconds;
            File.WriteAllText(CommonUtil.ParsePath($"userdata/debug/hnav-snapshot-{secondsSinceEpoch}.txt"), JsonConvert.SerializeObject(Autopilot.hnavQueue));
            File.WriteAllText(CommonUtil.ParsePath($"userdata/debug/mnav-snapshot-{secondsSinceEpoch}.txt"), JsonConvert.SerializeObject(Autopilot.mnavQueue));*/
        }

    }
}       
