﻿using Autosu.classes.autopilot;
using Autosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WMPLib;

namespace Autosu.Utils {
    public static class APUtil {
        public static Vector2 OsuPixelToScreen(Vector2 osuPixel) {
            // playfield height = 80% resln height
            // width = 4/3 * playfield height
            // Osupixel ref 640x840 is based on playfield not screen

            // Calculate the dimensions of the playingArea based on screen resolution
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            int playingAreaHeight = (int) (screenHeight * 0.8);
            int playingAreaWidth = (int) (playingAreaHeight * 4 / 3);

            int playingAreaY = (int) (screenHeight * 0.02);

            // Calculate the scale factor to convert osuPixels to actual screen pixels
            float scaleX = (float) playingAreaWidth / 512f;
            float scaleY = (float) playingAreaHeight / 384f;

            // Calculate the actual screen position of the osuPixel
            float actualX = osuPixel.X * scaleX + (screenWidth - playingAreaWidth) / 2f;
            float actualY = osuPixel.Y * scaleY + playingAreaY + (screenHeight - playingAreaHeight) / 2f;

            var ret = new Vector2(actualX, actualY);

            // hardrock
            if (Autopilot.i.config.features.hardrock) {
                ret.Y = screenHeight - ret.Y;
                ret.Y += playingAreaY * 2;
            }

            return ret;
        }

        public static float OsuPixelDistance(float dist) {
            return Vector2.Distance(OsuPixelToScreen(new(0, 0)), OsuPixelToScreen(new(dist, dist)));
        }

        public static void PlayAnnunciatorAlert() {
            var audio = new WindowsMediaPlayer();
            audio.URL = CommonUtil.ParsePath("resources/audio/alert.wav");
            audio.settings.volume = 20;
            audio.controls.play();

            using (var player = new SoundPlayer()) {
                player.SoundLocation = CommonUtil.ParsePath("resources/audio/alert.wav");
                player.Play();
            }

        }


        public static Vector2 OffsetLocation(Vector2 loc, float distance) {
            Tuple<float, float> location = new(loc.X, loc.Y);
            // Generate a random angle using the built-in Random class
            double randomAngle = new Random().NextDouble() * Math.PI * 2;

            // Calculate the new location by adding the offset to the original location
            float offsetX = (float) Math.Cos(randomAngle) * distance;
            float offsetY = (float) Math.Sin(randomAngle) * distance;

            float newX = location.Item1 + offsetX;
            float newY = location.Item2 + offsetY;

            return new(newX, newY);
        }

    }
}
