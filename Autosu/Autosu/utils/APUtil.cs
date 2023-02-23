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
            int osuScreenWidth = 640;
            int osuScreenHeight = 480;

            Rectangle screenBounds = Screen.GetBounds(Point.Empty);
            int screenWidth = screenBounds.Width;
            int screenHeight = screenBounds.Height;

            float xScale = (float) screenWidth / osuScreenWidth;
            float yScale = (float) screenHeight / osuScreenHeight;

            int screenX = (int) (osuPixel.X * xScale);
            int screenY = (int) (osuPixel.Y * yScale);

            return new(screenX, screenY);
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

    }
}
