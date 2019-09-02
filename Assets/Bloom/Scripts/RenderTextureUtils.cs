using UnityEngine;

namespace PostEffects
{
    public class RenderTextureUtils
    {
        public static bool SupportsRenderToFloatTexture ()
        {
            return
                SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) ||
                SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        }

        public static RenderTextureFormat GetSupportedFormat (RenderTextureFormat targetFormat)
        {
            if (IsHalfFormat(targetFormat))
            {
                bool supportsHalf = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
                if (!supportsHalf)
                    targetFormat = ToFloatFormat(targetFormat);
            }

            if (!SystemInfo.SupportsRenderTextureFormat(targetFormat))
            {
                switch (targetFormat)
                {
                    case RenderTextureFormat.RHalf:
                        return GetSupportedFormat(RenderTextureFormat.RGHalf);

                    case RenderTextureFormat.RGHalf:
                        return GetSupportedFormat(RenderTextureFormat.ARGBHalf);

                    case RenderTextureFormat.RFloat:
                        return GetSupportedFormat(RenderTextureFormat.RGFloat);

                    case RenderTextureFormat.RGFloat:
                        return GetSupportedFormat(RenderTextureFormat.ARGBFloat);
                }
            }

            return targetFormat;
        }

        public static bool IsHalfFormat (RenderTextureFormat format)
        {
            switch (format)
            {
                case RenderTextureFormat.RHalf:
                case RenderTextureFormat.RGHalf:
                case RenderTextureFormat.ARGBHalf:
                    return true;
            }

            return false;
        }

        public static RenderTextureFormat ToFloatFormat (RenderTextureFormat format)
        {
            switch (format)
            {
                case RenderTextureFormat.RHalf:
                    return RenderTextureFormat.RFloat;

                case RenderTextureFormat.RGHalf:
                    return RenderTextureFormat.RGFloat;

                case RenderTextureFormat.ARGBHalf:
                    return RenderTextureFormat.ARGBFloat;
            }

            return format;
        }

        public static Vector2Int GetScreenResolution (int resolution)
        {
            float aspectRatio = (float)Screen.width / (float)Screen.height;
            if (aspectRatio < 1)
                aspectRatio = 1f / aspectRatio;

            int min = resolution;
            int max = (int)((float)resolution * aspectRatio);

            if (Screen.width > Screen.height)
                return new Vector2Int(max, min);
            else
                return new Vector2Int(min, max);
        }

        public static Vector2 GetTextureScreenScale (Texture2D texture)
        {
            if (texture == null) return Vector2.one;

            Vector2 scale;
            scale.x = (float)Screen.width / (float)texture.width;
            scale.y = (float)Screen.height / (float)texture.height;
            return scale;
        }
    }
}