using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Utils {
    public static class MouseUtil {
        public static Vector2 PlayareaClampPos(Vector2 boxSize, Vector2 mouseScreenPosition) {
            // Get the center position of the screen
            Vector2 screenCenter = new(Screen.PrimaryScreen.Bounds.Width / 2f, Screen.PrimaryScreen.Bounds.Height / 2f);

            // Calculate the position of the box's center
            Vector2 boxCenter = new(screenCenter.X - boxSize.X / 2f, screenCenter.Y - boxSize.Y / 2f);

            // Calculate the position of the mouse relative to the box
            Vector2 mouseRelativeToBox = new(mouseScreenPosition.X - boxCenter.X, mouseScreenPosition.Y - boxCenter.Y);

            // Clamp the mouse position to the box if it's outside
            mouseRelativeToBox.X = Math.Max(0, Math.Min(mouseRelativeToBox.X, boxSize.X));
            mouseRelativeToBox.Y = Math.Max(0, Math.Min(mouseRelativeToBox.Y, boxSize.Y));

            // Normalize the mouse position to be between 0 and 1
            mouseRelativeToBox.X /= boxSize.X;
            mouseRelativeToBox.Y /= boxSize.Y;

            return mouseRelativeToBox;
        }

        public static Vector2 RelativeMousePosition(Vector2 mousePosition) {
            return RelativeMousePosition(mousePosition, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        public static Vector2 RelativeMousePosition(Vector2 mousePosition, int width, int height) {
            Vector2 screenSize = new Vector2(width, height);
            return mousePosition / screenSize;
        }

        public static Vector2[] GetLinearPath(Vector2 currentPos, Vector2 targetPos, int durationMs, int stepMs) {
            List<Vector2> positions = new List<Vector2>();

            // Calculate the distance to move in each axis
            float deltaX = targetPos.X - currentPos.X;
            float deltaY = targetPos.Y - currentPos.Y;

            // Calculate the total distance to move and the number of steps required
            float distance = (float) Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            int numSteps = (int) Math.Ceiling(durationMs / (float) stepMs);
            float stepSize = distance / numSteps;

            // Calculate the direction of movement and the initial position
            float directionX = deltaX / distance;
            float directionY = deltaY / distance;
            Vector2 currentPosition = currentPos;

            // Add the initial position to the list of positions
            positions.Add(currentPosition);

            // Calculate the intermediate positions
            for (int i = 0; i < numSteps; i++) {
                // Calculate the next position
                float nextPosX = currentPosition.X + stepSize * directionX;
                float nextPosY = currentPosition.Y + stepSize * directionY;
                Vector2 nextPosition = new Vector2(nextPosX, nextPosY);

                // Add the next position to the list of positions
                positions.Add(nextPosition);

                // Update the current position
                currentPosition = nextPosition;
            }

            // Convert the list to an array and return it
            return positions.ToArray();

        }


        public static bool SetCursor(Point position) {
            Cursor.Position = new(position.X, position.Y);

            return true;
        }

    }
}
