using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class XMath
    {
        public const float Epsilon = 0.00001f;
        public const float Sqrt2 = 1.4142135623730950488016887242097f;
        public const float Sqrt3 = 1.7320508075688772935274463415059f;

        public static bool FloatEqual(float a, float b)
        {
            return (a - b > -XMath.Epsilon) && (a - b < XMath.Epsilon);
        }

        public static bool FloatEqual(Vector2 a, Vector2 b)
        {
            return XMath.FloatEqual(a.x, b.x)
                && XMath.FloatEqual(a.y, b.y);
        }

        public static bool FloatEqual(Vector3 a, Vector3 b)
        {
            return XMath.FloatEqual(a.x, b.x)
                && XMath.FloatEqual(a.y, b.y)
                && XMath.FloatEqual(a.z, b.z);
        }

        public static Vector2 RandomRange(Vector2 a, Vector2 b)
        {
            float x = Random.Range(a.x, b.x);
            float y = Random.Range(a.y, b.y);
            Vector2 result = Vector2.zero;
            result.Set(x, y);
            return result;
        }

        public static Vector3 RandomRange(Vector3 a, Vector3 b)
        {
            float x = Random.Range(a.x, b.x);
            float y = Random.Range(a.y, b.y);
            float z = Random.Range(a.z, b.z);
            Vector3 result = Vector3.zero;
            result.Set(x, y, z);
            return result;
        }

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            float x = Mathf.Clamp(value.x, min.x, max.x);
            float y = Mathf.Clamp(value.y, min.y, max.y);
            float z = Mathf.Clamp(value.z, min.z, max.z);
            Vector3 result = Vector3.zero;
            result.Set(x, y, z);
            return result;
        }

        public static float AngleToRadian(float angle)
        {
            return angle * Mathf.PI / 180.0f;
        }

        public static float RadianToAngle(float radians)
        {
            return radians * 180.0f / Mathf.PI;
        }

        /// <summary>
        /// Normalize radians to [0 ~ 2pi)
        /// </summary>
        public static float NormalizeRadian(float radians)
        {
            float result = radians;
            while (result >= Mathf.PI * 2)
                result = result - Mathf.PI * 2;
            while (result < 0)
                result = result + Mathf.PI * 2;
            return result;
        }

        /// <summary>
        /// Normalize angle to [0 ~ 360)
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            float result = angle;
            while (result >= 360)
                result = result - 360;
            while (result < 0)
                result = result + 360;
            return result;
        }

        /// <summary>
        /// Symmetrize radians to [-pi ~ +pi]
        /// </summary>
        public static float SymmetrizeRadian(float radians)
        {
            float result = radians;
            while (result > Mathf.PI)
                result = result - Mathf.PI * 2;
            while (result < -Mathf.PI)
                result = result + Mathf.PI * 2;
            return result;
        }

        /// <summary>
        /// Symmetrize angle to [-180 ~ +180]
        /// </summary>
        public static float SymmetrizeAngle(float angle)
        {
            float result = angle;
            while (result > 180)
                result = result - 360;
            while (result < -180)
                result = result + 360;
            return result;
        }

        public static bool Intersect(Rect area1, Rect area2)
        {
            if (area1.x >= area2.x + area2.width)
                return false;
            if (area1.x + area1.width <= area2.x)
                return false;
            if (area1.y >= area2.y + area2.height)
                return false;
            if (area1.y + area1.height <= area2.y)
                return false;
            return true;
        }

        /// <summary>
        /// intersetion-test between sphere and line-segment
        /// </summary>
        /// <param name="center">sphere center</param>
        /// <param name="radius">sphere radius</param>
        /// <param name="p1">line-segment p1</param>
        /// <param name="p2">line-segment p2</param>
        /// <returns></returns>
        public static bool Intersect(Vector3 center, float radius, Vector3 p1, Vector3 p2)
        {
            Vector3 point = XMath.NearestPointOnLineSeg(center, p1, p2);
            return (center - point).sqrMagnitude <= radius * radius;
        }

        /// <summary>
        /// Get the nearest point on the line(p1->p2) from the point p
        /// </summary>
        /// <param name="p">the known point</param>
        /// <param name="p1">the line segment point p1</param>
        /// <param name="p2">the line segment point p2</param>
        /// <returns></returns>
        public static Vector3 NearestPointOnLineSeg(Vector3 p, Vector3 p1, Vector3 p2)
        {
            Vector3 vl = p2 - p1;
            Vector3 vp = p - p1;

            float len = vl.magnitude;
            vl.Normalize();

            float u = Vector3.Dot(vp, vl);

            if (u <= 0) return p1;
            else if (u >= len) return p2;
            else return p1 + vl * u;
        }

        /// <summary>
        /// Compute elastic force between two points (p0 and p1).
        /// </summary>
        /// <param name="p0">the force-caster point</param>
        /// <param name="p1">the force-receiver point</param>
        /// <param name="strength">elastic strength</param>
        /// <param name="minlen">The force is pushing if the distance of two points less than this value.</param>
        /// <param name="maxlen">The force is pulling if the distance of two points greater than this value.</param>
        /// <returns></returns>
        public static Vector3 Elastic(Vector3 p0, Vector3 p1, float strength, float minlen, float maxlen)
        {
            if (Mathf.Approximately(strength, 0))
                return Vector3.zero;

            Vector3 dir = p0 - p1;
            float len = dir.magnitude;

            float delta = 0.0f;
            if (len < minlen) delta = len - minlen;
            else if (len > maxlen) delta = len - maxlen;
            if (Mathf.Approximately(delta, 0))
                return Vector3.zero;

            if (len <= Mathf.Epsilon)
                dir = XMath.RandomRange(-Vector3.one, Vector3.one);

            return dir.normalized * strength * delta;
        }

        /// <summary>
        /// compute a parabola-curve f(x) = ax^2 + bx
        /// p0 = (0, 0), p1(x, y), top = ymax + h
        /// </summary>
        /// <param name="a">a factor for f(x)</param>
        /// <param name="b">b factor for f(x)</param>
        public static void ComputeParabola(float x, float y, float h, out float a, out float b)
        {
            float top = Mathf.Max(y, 0) + h;
            float x_2 = x * x;
            float delta = x_2 - x_2 * y / top;
            float _2A = x_2 / top * 0.5f;
            float _nB = x;
            b = (_nB + Mathf.Sqrt(delta)) / _2A;
            a = -b * b / (4 * top);
        }

        public static float SampleParabola(float x, float a, float b, float c)
        {
            return a * x * x + b * x + c;
        }

        /// <summary>
        /// b(t) = (1-t)^2*p0 + 2t(1-t)*p1 + t^2 * p2
        /// </summary>
        public static Vector3 SampleBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        }

        /// <summary>
        /// b(t) = (1-t)^3*p0 + 3t(1-t)^2*p1 + 3t^2(1-t)*p2 + t^3*p3
        /// </summary>
        public static Vector3 SampleBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return
                (1 - t) * (1 - t) * (1 - t) * p0 +
                3 * t * (1 - t) * (1 - t) * p1 +
                3 * t * t * (1 - t) * p2 +
                t * t * t * p3;
        }

        public static void Smooth(float[,] data, float innerWeight = 0.5f, float outerWeight = 0.5f)
        {
            int h = data.GetLength(0);
            int w = data.GetLength(1);
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    float value = 0;
                    value += data[y, x] * innerWeight + data[y - 1, x - 1] * outerWeight;
                    value += data[y, x] * innerWeight + data[y - 1, x] * outerWeight;
                    value += data[y, x] * innerWeight + data[y - 1, x + 1] * outerWeight;
                    value += data[y, x] * innerWeight + data[y, x - 1] * outerWeight;
                    value += data[y, x] * innerWeight + data[y, x + 1] * outerWeight;
                    value += data[y, x] * innerWeight + data[y + 1, x - 1] * outerWeight;
                    value += data[y, x] * innerWeight + data[y + 1, x] * outerWeight;
                    value += data[y, x] * innerWeight + data[y + 1, x + 1] * outerWeight;
                    data[y, x] = value * 0.125f; // value / 8
                }
            }
        }

        public static void Smooth(float[] data, int w, int h, float innerWeight = 0.5f, float outerWeight = 0.5f)
        {
            if (data.Length != w * h)
                throw new System.ArgumentException("data.Length != w * h");
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    float value = 0;
                    value += data[y * w + x] * innerWeight + data[(y - 1) * w + (x - 1)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y - 1) * w + (x + 0)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y - 1) * w + (x + 1)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y + 0) * w + (x - 1)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y + 0) * w + (x + 1)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y + 1) * w + (x - 1)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y + 1) * w + (x + 0)] * outerWeight;
                    value += data[y * w + x] * innerWeight + data[(y + 1) * w + (x + 1)] * outerWeight;
                    data[y * w + x] = value * 0.125f; // value / 8
                }
            }
        }
    }
}

