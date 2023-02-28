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

        public static Vector2[] GetLinearPathSlider(Vector2 currentPos, Vector2[] positions, int durationMs, int stepMs, int haltDist = -1) {
            stepMs = Math.Max(stepMs, 1);
            List<Vector2> ret = new();
            ret.Add(currentPos);

            // each leg should have a different timing
            float totalDist = 0f;
            List<Vector2> totalPoints = new List<Vector2> { currentPos};
            totalPoints.AddRange(positions);
            for (int i = 0; i < totalPoints.Count - 1; i++) totalDist += Vector2.Distance(totalPoints[i], totalPoints[i + 1]);

            // give placeholder
            if (totalDist < haltDist) return GetPlaceholderPath(durationMs, stepMs);

            Vector2 startFrom = currentPos;
            foreach (var pos in positions) {
                float dist = Vector2.Distance(startFrom, pos);
                ret.AddRange(GetLinearPath(startFrom, pos, (int)(dist / totalDist * durationMs), stepMs));
                startFrom = pos;
            }

            return ret.ToArray();
        }

        public static Vector2[] GetBezierPath(Vector2 currentPos, Vector2[] positions, int durationMs, int stepMs) {
            // Calculate the total distance of the path
            float totalDistance = 0f;
            for (int i = 0; i < positions.Length - 1; i++) {
                totalDistance += Vector2.Distance(positions[i], positions[i + 1]);
            }

            // Calculate the control points for the Bezier curve
            Vector2[] controlPoints = new Vector2[positions.Length + 2];
            controlPoints[0] = currentPos;
            controlPoints[controlPoints.Length - 1] = positions[positions.Length - 1];
            for (int i = 0; i < positions.Length; i++) {
                float t = Vector2.Distance(currentPos, positions[i]) / totalDistance;
                controlPoints[i + 1] = Vector2.Lerp(currentPos, positions[i], t);
            }

            // Calculate the intermediate points on the Bezier curve
            List<Vector2> intermediatePoints = new List<Vector2>();
            int numSteps = (int) Math.Ceiling(durationMs / (float) stepMs);
            for (int i = 0; i <= numSteps; i++) {
                float t = i / (float) numSteps;
                Vector2 point = CalculateBezierPoint(t, controlPoints);
                intermediatePoints.Add(point);
            }

            return intermediatePoints.ToArray();
        }

        public static Vector2[] GetPlaceholderPath(int durationMs, int stepMs) {
            stepMs = Math.Max(stepMs, 1);
            List<Vector2> ret = new();
            int stepCount = durationMs / stepMs;
            for (int i = 0; i < stepCount; i++) ret.Add(Vector2.Zero);

            return ret.ToArray();
        }

        private static Vector2 CalculateBezierPoint(float t, Vector2[] controlPoints) {
            int n = controlPoints.Length - 1;
            Vector2 point = Vector2.Zero;
            for (int i = 0; i <= n; i++) {
                float b = CalculateBernsteinPolynomial(n, i, t);
                point += b * controlPoints[i];
            }
            return point;
        }

        private static float CalculateBernsteinPolynomial(int n, int i, float t) {
            float ti = 1 - t;
            float a = Factorial(n) / (Factorial(i) * Factorial(n - i));
            float b = (float) Math.Pow(t, i);
            float c = (float) Math.Pow(ti, n - i);
            return a * b * c;
        }

        private static int Factorial(int n) {
            int result = 1;
            for (int i = 1; i <= n; i++) {
                result *= i;
            }
            return result;
        }

        public static Vector2[] GetCircularPath(Vector2 startPos, Vector2 midPoint, Vector2 endPos, int durationMs, int stepMs, int haltDist) {
            stepMs = Math.Max(stepMs, 1);

            List<Vector2> positions = new List<Vector2>();

            // Calculate the radius and center of the circle that passes through the three points
            float A = startPos.X - midPoint.X;
            float B = startPos.Y - midPoint.Y;
            float C = endPos.X - midPoint.X;
            float D = endPos.Y - midPoint.Y;

            float E = A * (midPoint.X + startPos.X) + B * (midPoint.Y + startPos.Y);
            float F = C * (midPoint.X + endPos.X) + D * (midPoint.Y + endPos.Y);

            float G = 2 * (A * (endPos.Y - midPoint.Y) - B * (endPos.X - midPoint.X));
            if (G == 0) {
                // Points are collinear, cannot construct a circle
                return new Vector2[0];
            }

            float centerX = (D * E - B * F) / G;
            float centerY = (A * F - C * E) / G;

            float radius = (float) Math.Sqrt(Math.Pow(midPoint.X - centerX, 2) + Math.Pow(midPoint.Y - centerY, 2));

            // Calculate the angles from the center to the start and end points
            float startAngle = (float) Math.Atan2(startPos.Y - centerY, startPos.X - centerX);
            float endAngle = (float) Math.Atan2(endPos.Y - centerY, endPos.X - centerX);

            // Calculate the total angle to sweep and the number of steps required
            float sweepAngle = endAngle - startAngle;

            if (sweepAngle < 0f) {
                sweepAngle += (float) (2 * Math.PI);
            }

            Vector2 startToEnd = endPos - startPos;
            float oppositeAngle = (float) Math.Atan2(startToEnd.Y, startToEnd.X) + (float) Math.PI;

            /*if (sweepAngle > Math.PI) {
                sweepAngle = -((float) Math.PI * 2 - sweepAngle);
            }*/

            // Check direction of sweep angle and reverse it if it is greater than pi
            if (Math.Abs(sweepAngle) > Math.PI) {
                if (sweepAngle > 0) {
                    sweepAngle -= (float) (2 * Math.PI);
                } else {
                    sweepAngle += (float) (2 * Math.PI);
                }
            }


            if (MathF.Abs(sweepAngle) < (MathF.PI / 2f) && Vector2.Distance(startPos, endPos) < haltDist) return GetPlaceholderPath(durationMs, stepMs);


            int numSteps = (int) Math.Ceiling(durationMs / (float) stepMs);
            float stepAngle = sweepAngle / numSteps;

            // Calculate the positions along the circular path
            Vector2 currentPosition = startPos;
            positions.Add(currentPosition);
            for (int i = 0; i < numSteps; i++) {
                // Calculate the next angle
                float nextAngle = startAngle + stepAngle * (i + 1);

                // Calculate the next position
                float nextPosX = centerX + radius * (float) Math.Cos(nextAngle);
                float nextPosY = centerY + radius * (float) Math.Sin(nextAngle);
                Vector2 nextPosition = new Vector2(nextPosX, nextPosY);

                // Add the next position to the list of positions
                positions.Add(nextPosition);

                // Update the current position
                currentPosition = nextPosition;
            }

            //Debug.WriteLine($"Circular Path: {startPos} -> {midPoint} -> {endPos} under {durationMs}ms with {stepMs} move time. {positions.Count} nodes");

            // Convert the list to an array and return it
            return positions.ToArray();
        }

        private static float GetAngle(Vector2 vector) {
            return (float) Math.Atan2(vector.Y, vector.X);
        }

        public static Vector2[] GetSpinnerPath(Vector2 center, float majorAxis, float minorAxis, float rpm, float duration, float stepMs, float maximumOffset = 0) {
            stepMs = Math.Max(stepMs, 1);

            List<Vector2> positions = new List<Vector2>();

            float distance = (float) Math.PI * (majorAxis + minorAxis);
            int numSteps = (int) Math.Ceiling(duration / stepMs);
            float stepSize = distance / numSteps;

            float angleStep = 2 * (float) Math.PI * rpm / 60 / (1000 / stepMs);
            float currentAngle = 0;

            Random rand = new Random();

            for (int i = 0; i < numSteps; i++) {
                float xOffset = maximumOffset * (float) rand.NextDouble() * 2 - maximumOffset;
                float yOffset = maximumOffset * (float) rand.NextDouble() * 2 - maximumOffset;

                float majorAxisOffset = maximumOffset * (float) rand.NextDouble() * 2 - maximumOffset;
                float minorAxisOffset = maximumOffset * (float) rand.NextDouble() * 2 - maximumOffset;

                float majorAxisCurrent = majorAxis + majorAxisOffset;
                float minorAxisCurrent = minorAxis + minorAxisOffset;

                float x = center.X + majorAxisCurrent * (float) Math.Cos(currentAngle) + xOffset;
                float y = center.Y + minorAxisCurrent * (float) Math.Sin(currentAngle) + yOffset;

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
