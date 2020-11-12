using System;

#pragma warning disable CS0660
namespace Dirt.Game.Math
{
    [System.Serializable]
    public struct float3 : IEquatable<float3>
    {
        public float x, y, z;
        public float3(float x, float y) 
        {
            this.x = x;
            this.y = y;
            this.z = 0f;
        }

        public float3(float[] arr)
        {
            x = arr[0];
            y = arr[1];
            z = arr[2];
        }

        public float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public readonly static float3 one = new float3() { x = 1f, y = 1f, z = 1f };
        public readonly static float3 zero = new float3() { x = 0f, y = 0f, z = 0f };


        public float3 normalized()
        {
            if ( x != 0f || y != 0f || z != 0f )
            {
                float mag = magnitude;
                return new float3(x / mag, y / mag, z / mag);
            }

            return float3.zero;
        }

        public float magnitude {  get { return (float) System.Math.Sqrt(x * x + y * y + z * z); } }
        public float sqrMagnitude {  get { return x * x + y * y + z * z; } }

        public static float3 operator *(float3 l, float r)
        {
            return new float3() { x = l.x * r, y = l.y * r, z = l.z * r };
        }

        public static float3 operator *(float l, float3 r)
        {
            return new float3() { x = l * r.x, y = l * r.y, z = l * r.z };
        }

        public static float3 operator *(float3 l, float3 r)
        {
            return new float3() { x = l.x * r.x, y = l.y * r.y, z = l.z * r.z };
        }

        public static float3 operator+(float3 l, float3 r)
        {
            return new float3() { x = l.x + r.x, y = l.y + r.y, z = l.z + r.z };
        }

        public static float3 min(float3 l, float3 r)
        {
            return new float3() { x = l.x < r.x ? l.x : r.x, y = l.y < r.y ? l.y : r.y, z = l.z < r.z ? l.z : r.z };
        }

        public static float dot(float3 l, float3 r)
        {
            float3 prod = l * r;
            return prod.x + prod.y + prod.z;
        }
        public static float3 max(float3 l, float3 r)
        {
            return new float3() { x = l.x > r.x ? l.x : r.x, y = l.y > r.y ? l.y : r.y, z = l.z > r.z ? l.z : r.z };
        }

        public static float3 operator-(float3 l, float3 r)
        {
            return new float3() { x = l.x - r.x, y = l.y - r.y, z = l.z - r.z };
        }

        public static bool operator ==(float3 l, float3 r)
        {
            return l.x == r.x && l.y == r.y && l.z == r.z;
        }

        public static bool operator !=(float3 l, float3 r)
        {
            return l.x != r.x || l.y != r.y || l.z != r.z;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        public bool Equals(float3 other)
        {
            return this == other;
        }

        public override string ToString()
        {
            return $"{{{x}, {y}, {z}}}";
        }
    }
}
#pragma warning restore CS0660