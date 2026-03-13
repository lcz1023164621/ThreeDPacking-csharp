using System;
using System.Drawing;

namespace ThreeDPacking.App.Rendering
{
    /// <summary>
    /// 颜色辅助工具，为不同物品分配区分颜色
    /// </summary>
    public static class ColorHelper
    {
        private static readonly float[] Saturations = { 0.7f, 0.8f, 0.6f, 0.9f };
        private static readonly float[] Lightnesses = { 0.5f, 0.6f, 0.45f, 0.55f };

        public static Color GetColor(string id, float alpha = 0.6f)
        {
            int hash = (id ?? "").GetHashCode();
            float hue = ((hash & 0x7FFFFFFF) % 360) / 360f;
            float sat = Saturations[Math.Abs(hash >> 8) % Saturations.Length];
            float lit = Lightnesses[Math.Abs(hash >> 16) % Lightnesses.Length];

            return HslToColor(hue, sat, lit, alpha);
        }

        private static Color HslToColor(float h, float s, float l, float alpha)
        {
            float r, g, b;
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                r = HueToRgb(p, q, h + 1f / 3f);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1f / 3f);
            }

            return Color.FromArgb(
                (int)(alpha * 255),
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255));
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }
    }
}
