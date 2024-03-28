using System;

#pragma warning disable CS0660
namespace Dirt.Game.Math
{
    [Serializable]
    public struct float2 : IEquatable<float2>
    {
        public float x, y;
        public float2(float x, float y) 
        {
            this.x = x;
            this.y = y;
        }

        public float2(float[] arr)
        {
            x = arr[0];
            y = arr[1];
        }

        public float2(float3 f3)
        {
            x = f3.x;
            y = f3.y;
        }

        public readonly static float2 one = new float2() { x = 1f, y = 1f };
        public readonly static float2 zero = new float2() { x = 0f, y = 0f };


        public float2 normalized()
        {
            if ( x != 0f || y != 0f)
            {
                float mag = magnitude;
                return new float2(x / mag, y / mag);
            }

            return float2.zero;
        }

        public float normalize()
        {
            if (x != 0f || y != 0f)
            {
                float mag = magnitude;
                x /= mag;
                y /= mag;
                return mag;
            }
            return 0f;
        }

        public float magnitude {  get { return (float) System.Math.Sqrt(x * x + y * y); } }
        public float sqrMagnitude {  get { return x * x + y * y; } }

        public static float2 operator *(float2 l, float r)
        {
            return new float2() { x = l.x * r, y = l.y * r};
        }

        public static float2 operator *(float l, float2 r)
        {
            return new float2() { x = l * r.x, y = l * r.y };
        }

        public static float2 operator *(float2 l, float2 r)
        {
            return new float2() { x = l.x * r.x, y = l.y * r.y};
        }

        public static float2 operator+(float2 l, float2 r)
        {
            return new float2() { x = l.x + r.x, y = l.y + r.y };
        }

        public static float2 min(float2 l, float2 r)
        {
            return new float2() { x = l.x < r.x ? l.x : r.x, y = l.y < r.y ? l.y : r.y };
        }

        public static float dot(float2 l, float2 r)
        {
            float2 prod = l * r;
            return prod.x + prod.y;
        }
        public static float2 max(float2 l, float2 r)
        {
            return new float2() { x = l.x > r.x ? l.x : r.x, y = l.y > r.y ? l.y : r.y };
        }

        public static float2 operator-(float2 l, float2 r)
        {
            return new float2() { x = l.x - r.x, y = l.y - r.y };
        }

        public static bool operator ==(float2 l, float2 r)
        {
            return l.x == r.x && l.y == r.y;
        }

        public static bool operator !=(float2 l, float2 r)
        {
            return l.x != r.x || l.y != r.y;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }

        public bool Equals(float2 other)
        {
            return this == other;
        }

        public override string ToString()
        {
            return $"{{{x}, {y}}}";
        }
    }
}
#pragma warning restore CS0660