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
                status = config.features.n1 ? EAutopilotMasterState.ON : EAutopilotMasterState.FULL;
                AutopilotPage.instance.SetOverlay(true, config.features.n1);
                if (!config.features.n1) {
                    if (AutopilotPage.instance.visible) AutopilotPage.instance.visible = true;
                    playhead.Start();
                }


                // hardrock: translate the hits
                if (config.features.hardrock) {
                    // circleRadius +30%
                    beatmap.circleSize *= 1.3f;
                    // approach +40%
                    beatmap.approachRate *= 1.4f;
                    // overallDifficulty +40%
                    beatmap.overallDifficulty = (int)(beatmap.overallDifficulty * 1.4f);

                }

                // double time
                if (config.features.doubletime) {
                    foreach (var obj in navQueue) {
                        obj.time = (int)(obj.time / 1.5f);

                        if (obj is SpinnerObject) {
                            var spinner = (SpinnerObject)obj;
                            spinner.endTime = (int) (spinner.endTime / 1.5f);
                        }

                    }
                }


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
