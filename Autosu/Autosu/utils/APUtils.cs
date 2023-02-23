using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Ttils {
    public static class APUtils {
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
    }
}
