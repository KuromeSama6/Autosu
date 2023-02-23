using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Utils {
    public static class MouseUtil {
        public static Vector2 RelativeMousePosition(Vector2 mousePosition) {
            Vector2 screenSize = new Vector2(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            return mousePosition / screenSize;
        }
    }
}
