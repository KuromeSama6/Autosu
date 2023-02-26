using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            stepMs = Math.Max(stepMs, 1);

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


            //Debug.WriteLine($"Path: {currentPos} -> {targetPos} under {durationMs}ms with {stepMs} move time. {positions.Count} nodes");

            // Convert the list to an array and return it
            return positions.ToArray();

        }

        public static Vector2[] GetLinearPathFake(Vector2 currentPos, Vector2[] positions, int durationMs, int stepMs) {
            stepMs = Math.Max(stepMs, 1);
            List<Vector2> ret = new();
            ret.Add(currentPos);

            int timeEachLeg = durationMs / positions.Length;

            Vector2 startFrom = currentPos;
            foreach (var pos in positions) {
                ret.AddRange(GetLinearPath(startFrom, pos, timeEachLeg, stepMs));
                startFrom = pos;
            }

            return ret.ToArray();
        }

        public static Vector2[] GetBezierPath(Vector2 startPoint, Vector2[] controlPoints, int durationMs, int stepMs) {
            stepMs = Math.Max(stepMs, 1);

            List<Vector2> positions = new List<Vector2>();

            // Calculate the number of steps required
            int numSteps = (int) Math.Ceiling(durationMs / (float) stepMs);

            // Calculate the time interval for each step
            float interval = 1.0f / numSteps;

            // Loop through each step and calculate the position on the curve
            for (int i = 0; i <= numSteps; i++) {
                float t = i * interval;
                Vector2 position = CalculateBezierPoint(t, startPoint, controlPoints);
                positions.Add(position);
            }

            // Convert the list to an array and return it
            return positions.ToArray();
        }

        public static Vector2[] GetLinearPathMulti(Vector2 currentPos, Vector2[] path, int durationMs, int stepMs) {
            stepMs = Math.Max(stepMs, 1);
            List<Vector2> positions = new List<Vector2>();

            // Add the current position to the list of positions
            positions.Add(currentPos);

            // Calculate the intermediate positions
            for (int i = 0; i < path.Length - 1; i++) {
                Vector2 start = positions[positions.Count - 1];
                Vector2 end = path[i + 1];
                float distance = (float) Math.Sqrt((end - start).LengthSquared());
                int numSteps = (int) Math.Ceiling(durationMs * distance / ((float) stepMs * path.Length));

                float directionX = (end.X - start.X) / distance;
                float directionY = (end.Y - start.Y) / distance;
                float stepSize = distance / numSteps;

                for (int j = 0; j < numSteps; j++) {
                    float nextPosX = start.X + stepSize * directionX;
                    float nextPosY = start.Y + stepSize * directionY;
                    Vector2 nextPosition = new Vector2(nextPosX, nextPosY);
                    positions.Add(nextPosition);
                    start = nextPosition;
                }
            }

            // Add the target position to the list of positions
            positions.Add(path[path.Length - 1]);

            // Convert the list to an array and return it
            return positions.ToArray();
        }

        private static Vector2 CalculateBezierPoint(float t, Vector2 startPoint, Vector2[] controlPoints) {
            // Calculate the number of control points
            int numControlPoints = controlPoints.Length;

            // Calculate the order of the curve
            int order = numControlPoints + 1;

            // Initialize the temporary arrays
            Vector2[] tempPoints1 = new Vector2[numControlPoints + 1];
            Vector2[] tempPoints2 = new Vector2[numControlPoints];

            // Copy the control points to the temporary array
            Array.Copy(controlPoints, tempPoints1, numControlPoints);

            // Loop through each order of the curve and calculate the points
            for (int i = 1; i < order; i++) {
                for (int j = 0; j < order - i; j++) {
                    if (i == 1) {
                        // Calculate the first order point
                        tempPoints2[j] = (1 - t) * tempPoints1[j] + t * tempPoints1[j + 1];
                    } else {
                        // Calculate the higher order points
                        tempPoints2[j] = (1 - t) * tempPoints2[j] + t * tempPoints2[j + 1];
                    }
                }
            }

            // Return the final point on the curve
            return (1 - t) * startPoint + t * tempPoints2[0];
        }

        public static Vector2[] GetCircularPath(Vector2 startPos, Vector2 midPos, Vector2 endPos, int durationMs, int stepMs) {
            stepMs = Math.Max(stepMs, 1);

            List<Vector2> positions = new List<Vector2>();

            // Calculate the radius of the circle based on the distance between start and mid points
            float radius = Vector2.Distance(startPos, midPos);

            // Calculate the center of the circle based on the midpoint
            Vector2 centerPos = midPos;

            // Calculate the angle between the start and end points
            float startAngle = (float) Math.Atan2(startPos.Y - centerPos.Y, startPos.X - centerPos.X);
            float endAngle = (float) Math.Atan2(endPos.Y - centerPos.Y, endPos.X - centerPos.X);
            float angleDiff = endAngle - startAngle;

            if (angleDiff < 0) {
                angleDiff += (float) (2 * Math.PI);
            }

            // Calculate the number of steps required
            int numSteps = (int) Math.Ceiling(durationMs / (float) stepMs);

            // Calculate the angle increment for each step
            float angleIncrement = angleDiff / numSteps;

            // Calculate the initial position on the circle
            float currentAngle = startAngle;
            Vector2 currentPosition = new Vector2(
                centerPos.X + radius * (float) Math.Cos(currentAngle),
                centerPos.Y + radius * (float) Math.Sin(currentAngle)
            );

            // Add the initial position to the list of positions
            positions.Add(currentPosition);

            // Calculate the intermediate positions
            for (int i = 0; i < numSteps; i++) {
                // Calculate the next position
                currentAngle += angleIncrement;
                Vector2 nextPosition = new Vector2(
                    centerPos.X + radius * (float) Math.Cos(currentAngle),
                    centerPos.Y + radius * (float) Math.Sin(currentAngle)
                );

                // Add the next position to the list of positions
                positions.Add(nextPosition);

                // Update the current position
                currentPosition = nextPosition;
            }

            // Add the end position to the list of positions
            positions.Add(endPos);

            // Convert the list to an array and return it
            return positions.ToArray();
        }

        public static Vector2[] GetSpinnerPath(Vector2 center, float majorAxis, float minorAxis, float rpm, float duration, float stepMs) {
            stepMs = Math.Max(stepMs, 1);

            List<Vector2> positions = new List<Vector2>();

            float distance = (float) Math.PI * (majorAxis + minorAxis);
            int numSteps = (int) Math.Ceiling(duration / stepMs);
            float stepSize = distance / numSteps;

            float angleStep = 2 * (float) Math.PI * rpm / 60 / (1000 / stepMs);
            float currentAngle = 0;

            for (int i = 0; i < numSteps; i++) {
                float x = center.X + majorAxis * (float) Math.Cos(currentAngle);
                float y = center.Y + minorAxis * (float) Math.Sin(currentAngle);
                Vector2 currentPosition = new Vector2(x, y);
                positions.Add(currentPosition);

                currentAngle += angleStep;
                if (currentAngle >= 2 * (float) Math.PI) {
                    currentAngle -= 2 * (float) Math.PI;
                }
            }

            return positions.ToArray();
        }

        public static bool SetCursor(Point position) {
            if (position.X <= 1 && position.Y <= 1) return false;
            Cursor.Position = new(position.X, position.Y);

            return true;
        }

    }
}
