// Minimal UnityEngine shim so the real LHIPA.cs / SharpWave compile & run outside Unity.
// Mathf mirrors Unity exactly: float results via (float)System.Math.* casts.
using System;

namespace UnityEngine
{
    public static class Debug
    {
        public static void Log(object o)        { Console.Error.WriteLine(o); }
        public static void LogWarning(object o) { Console.Error.WriteLine("WARN: " + o); }
        public static void LogError(object o)   { Console.Error.WriteLine("ERROR: " + o); }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265358979f;
        public static float Sqrt(float f)            => (float)Math.Sqrt(f);
        public static float Pow(float f, float p)    => (float)Math.Pow(f, p);
        public static float Log(float f)             => (float)Math.Log(f);
        public static float Log(float f, float b)    => (float)Math.Log(f, b);
        public static float Log10(float f)           => (float)Math.Log10(f);
        public static float Abs(float f)             => Math.Abs(f);
        public static int   Abs(int f)               => Math.Abs(f);
        public static float Min(float a, float b)    => Math.Min(a, b);
        public static int   Min(int a, int b)         => Math.Min(a, b);
        public static float Max(float a, float b)    => Math.Max(a, b);
        public static int   Max(int a, int b)         => Math.Max(a, b);
        public static float Round(float f)           => (float)Math.Round(f);
        public static float Floor(float f)           => (float)Math.Floor(f);
        public static float Ceil(float f)            => (float)Math.Ceiling(f);
        public static float Sin(float f)             => (float)Math.Sin(f);
        public static float Cos(float f)             => (float)Math.Cos(f);
    }
}
