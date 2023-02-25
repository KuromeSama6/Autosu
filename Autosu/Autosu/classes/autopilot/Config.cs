using Autosu.Classes;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.classes.autopilot {
    [Serializable]
    public class APConfig {
        public string name = "default";
        public APFeatureControl features;
        public APValueInput inputs;

        public static APConfig Load(string name) {
            string path = CommonUtil.ParsePath($@"userdata/profiles/{name.ToLower()}.aosu");
            return SerializationUtil.Load<APConfig>(path);
        }

        public APConfig() {
            features = new();
            inputs = new();
        }

        public void Save() {
            string path = CommonUtil.ParsePath($@"userdata/profiles/{name.ToLower()}.aosu");
            SerializationUtil.Save(path, this);

        }

        public void LoadBeatmap(Beatmap beatmap) {
            inputs.difficultyOverload = beatmap.overallDifficulty;
        }

    }

    [Serializable]
    public class APFeatureControl {
        public bool hnav = true;
        public bool mnav = true;
        public bool n1 = true;
        public bool hitDelay = true;
        public bool accuracySelect = true;
        public bool moveDelay = true;
        public bool targetOffset = true;
        public bool blankAddMouse = true;
        public bool humanSlider = true;
        public bool shortSliderHalt = true;
        public bool spinnerOffset = true;
        public bool spinnerRandom = true;
        public bool humanEndurance = true;
        public bool humanNervous = true;
        public bool humanDistraction = true;
        public bool autoSwitch = false;
        public bool takeoverStandby = true;
        public bool panic = false;
    }

    [Serializable]
    public class APValueInput {
        public int hnavDelayRef;
        public int mnavDelayRef;
        public int minimumAcc;
        public int targetOffsetAmount;
        public int spinnerRandomAmount;
        public int difficultyOverload;
    }

}
