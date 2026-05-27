using UnityEngine;

namespace ExtraBattleUpgrades.Hud;

internal static class BattleUpgradeIconPainter
{
    private const int TextureSize = 64;

    private static Sprite _lightningSprite;
    private static Sprite _heartSprite;

    internal static Sprite LightningSprite()
    {
        if (_lightningSprite != null)
        {
            return _lightningSprite;
        }

        Texture2D texture = DrawLightning();
        texture.name = "ExtraBattleUpgrades_PanicResponse_Lightning";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        _lightningSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f),
            100f);

        return _lightningSprite;
    }

    internal static Sprite HeartSprite()
    {
        if (_heartSprite != null)
        {
            return _heartSprite;
        }

        Texture2D texture = DrawHeart();
        texture.name = "ExtraBattleUpgrades_SecondChance_Heart";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        _heartSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f),
            100f);

        return _heartSprite;
    }

    private static Texture2D DrawLightning()
    {
        Texture2D texture = NewTransparentTexture();

        Vector2[] bolt =
        {
            new Vector2(37f, 4f),
            new Vector2(16f, 34f),
            new Vector2(30f, 34f),
            new Vector2(23f, 60f),
            new Vector2(48f, 25f),
            new Vector2(34f, 25f)
        };

        DrawGlowPolygon(texture, bolt, Color.white, 7f, 0.28f);
        FillPolygon(texture, bolt, Color.white);

        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D DrawHeart()
    {
        Texture2D texture = NewTransparentTexture();

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float nx = (x - 32f) / 24f;
                float ny = (y - 28f) / 24f;

                float value = Mathf.Pow(nx * nx + ny * ny - 0.32f, 3f) - nx * nx * ny * ny * ny;

                if (value <= 0f)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (value <= 0.08f)
                {
                    float alpha = Mathf.Clamp01(1f - value / 0.08f) * 0.28f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }

        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D NewTransparentTexture()
    {
        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[TextureSize * TextureSize];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        texture.SetPixels(pixels);
        return texture;
    }

    private static void FillPolygon(Texture2D texture, Vector2[] polygon, Color color)
    {
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), polygon))
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void DrawGlowPolygon(Texture2D texture, Vector2[] polygon, Color color, float radius, float strength)
    {
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                float distance = DistanceToPolygon(point, polygon);

                if (distance <= radius)
                {
                    float alpha = Mathf.Pow(1f - distance / radius, 2f) * strength;
                    Color current = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, Blend(current, color, alpha));
                }
            }
        }
    }

    private static float DistanceToPolygon(Vector2 point, Vector2[] polygon)
    {
        float minDistance = float.MaxValue;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Length];
            minDistance = Mathf.Min(minDistance, DistanceToSegment(point, a, b));
        }

        if (PointInPolygon(point, polygon))
        {
            return 0f;
        }

        return minDistance;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return Vector2.Distance(point, a + ab * t);
    }

    private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;

        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y)
                && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static Color Blend(Color under, Color over, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        float outAlpha = alpha + under.a * (1f - alpha);
        if (outAlpha <= 0f)
        {
            return Color.clear;
        }

        return new Color(
            (over.r * alpha + under.r * under.a * (1f - alpha)) / outAlpha,
            (over.g * alpha + under.g * under.a * (1f - alpha)) / outAlpha,
            (over.b * alpha + under.b * under.a * (1f - alpha)) / outAlpha,
            outAlpha);
    }
}