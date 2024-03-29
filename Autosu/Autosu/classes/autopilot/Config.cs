﻿using Autosu.Classes;
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
            
        }

    }

    [Serializable]
    public class APFeatureControl {
        public bool hnav = true;
        public bool mnav = true;
        public bool n1 = true;
        public bool hitDelay = true;
        public bool accuracySelect = false;
        public bool moveDelay = true;
        public bool targetOffset = true;
        public bool blankAddMouse = false;
        public bool humanSlider = true;
        public bool shortSliderHalt = true;
        public bool spinnerOffset = true;
        public bool spinnerRandom = true;
        public bool humanEndurance = false;
        public bool humanNervous = false;
        public bool humanDistraction = false;
        public bool autoSwitch = false;
        public bool takeoverStandby = true;
        public bool playDetent = false;
        public bool hardrock;
        public bool doubletime;
    }

    [Serializable]
    public class APValueInput {
        public int hnavDelayRef = 5;
        public int mnavDelayRef = 35;
        public int minimumAcc = 95;
        public int targetOffsetAmount = 25;
        public int targetOffsetThreshold = 90;
        public int spinnerRandomAmount = 70;
        public int sliderHaltThreshold = 230;
        public int targetSizeMultiplier = 150;
    }

}
