using System.Collections.Generic;
using ChefMachine.Ingredients;
using UnityEngine;

namespace ChefMachine.UI
{
    /// <summary>
    /// Generates clean, colorful procedural Sprites for ingredient UI icons
    /// (Customer Windows and Refrigerator selector).
    /// Cached so textures are generated only once.
    /// </summary>
    public static class IngredientIconFactory
    {
        private static readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Gets a procedural icon sprite for the specified ingredient type and state.
        /// </summary>
        public static Sprite GetIcon(IngredientType type, IngredientState state)
        {
            string key = type + "_" + state;
            if (iconCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Sprite generated = CreateIconSprite(type, state);
            iconCache[key] = generated;
            return generated;
        }

        private static Sprite CreateIconSprite(IngredientType type, IngredientState state)
        {
            int width = 64;
            int height = 64;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Vector2 center = new Vector2(32f, 32f);

            switch (type)
            {
                case IngredientType.Cheese:
                    DrawCheeseIcon(pixels, width, height, center);
                    break;

                case IngredientType.Meat:
                    if (state == IngredientState.Cooked)
                        DrawCookedMeatIcon(pixels, width, height, center);
                    else
                        DrawRawMeatIcon(pixels, width, height, center);
                    break;

                case IngredientType.Vegetable:
                    if (state == IngredientState.Chopped)
                        DrawChoppedVegIcon(pixels, width, height, center);
                    else
                        DrawRawVegIcon(pixels, width, height, center);
                    break;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        // --- DRAWING HELPERS ---

        private static void DrawCheeseIcon(Color[] pixels, int w, int h, Vector2 center)
        {
            Color cheeseYellow = new Color(1.0f, 0.82f, 0.15f, 1f);
            Color holeColor = new Color(0.9f, 0.65f, 0.1f, 1f);
            Color outline = new Color(0.75f, 0.5f, 0.05f, 1f);

            // Triangular wedge
            for (int y = 12; y <= 52; y++)
            {
                float t = (y - 12f) / 40f;
                int minX = Mathf.RoundToInt(Mathf.Lerp(14, 28, t));
                int maxX = Mathf.RoundToInt(Mathf.Lerp(52, 52, t));

                for (int x = minX; x <= maxX; x++)
                {
                    bool isBorder = (x <= minX + 2 || x >= maxX - 2 || y <= 14 || y >= 50);
                    Color c = isBorder ? outline : cheeseYellow;

                    // Hole 1
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(40, 32)) < 5f) c = holeColor;
                    // Hole 2
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(28, 22)) < 3.5f) c = holeColor;

                    pixels[y * w + x] = c;
                }
            }
        }

        private static void DrawRawMeatIcon(Color[] pixels, int w, int h, Vector2 center)
        {
            Color rawRed = new Color(0.88f, 0.2f, 0.2f, 1f);
            Color darkRedBorder = new Color(0.55f, 0.1f, 0.1f, 1f);
            Color fatWhite = new Color(0.98f, 0.92f, 0.92f, 1f);

            // Rounded steak cutlet
            for (int y = 12; y <= 52; y++)
            {
                for (int x = 12; x <= 52; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);

                    // Ellipse adjustment
                    float normX = (x - center.x) / 20f;
                    float normY = (y - center.y) / 16f;
                    float ellipseDist = normX * normX + normY * normY;

                    if (ellipseDist <= 1.0f)
                    {
                        bool isBorder = ellipseDist > 0.8f;
                        Color c = isBorder ? darkRedBorder : rawRed;

                        // Marbling fat streak
                        if (Mathf.Abs((x - 12) - (y - 12)) < 3f && ellipseDist < 0.6f)
                        {
                            c = fatWhite;
                        }

                        pixels[y * w + x] = c;
                    }
                }
            }
        }

        private static void DrawCookedMeatIcon(Color[] pixels, int w, int h, Vector2 center)
        {
            Color cookedBrown = new Color(0.42f, 0.22f, 0.12f, 1f);
            Color darkBrownBorder = new Color(0.24f, 0.10f, 0.04f, 1f);
            Color grillMark = new Color(0.18f, 0.08f, 0.02f, 1f);

            // Cooked steak cutlet with grill lines
            for (int y = 12; y <= 52; y++)
            {
                for (int x = 12; x <= 52; x++)
                {
                    float normX = (x - center.x) / 20f;
                    float normY = (y - center.y) / 16f;
                    float ellipseDist = normX * normX + normY * normY;

                    if (ellipseDist <= 1.0f)
                    {
                        bool isBorder = ellipseDist > 0.8f;
                        Color c = isBorder ? darkBrownBorder : cookedBrown;

                        // Diagonal grill marks
                        if (ellipseDist < 0.75f && ((x + y) % 12 < 3))
                        {
                            c = grillMark;
                        }

                        pixels[y * w + x] = c;
                    }
                }
            }
        }

        private static void DrawRawVegIcon(Color[] pixels, int w, int h, Vector2 center)
        {
            Color vegGreen = new Color(0.18f, 0.78f, 0.25f, 1f);
            Color stemGreen = new Color(0.1f, 0.45f, 0.15f, 1f);
            Color highlightGreen = new Color(0.45f, 0.92f, 0.5f, 1f);

            // Broccoli / Lettuce dome
            for (int y = 8; y <= 56; y++)
            {
                for (int x = 8; x <= 56; x++)
                {
                    Vector2 pos = new Vector2(x, y);

                    // Stem at bottom
                    if (x >= 28 && x <= 36 && y >= 8 && y <= 24)
                    {
                        pixels[y * w + x] = stemGreen;
                        continue;
                    }

                    // Floret cluster top
                    float distCenter = Vector2.Distance(pos, new Vector2(32, 38));
                    float distLeft = Vector2.Distance(pos, new Vector2(22, 34));
                    float distRight = Vector2.Distance(pos, new Vector2(42, 34));

                    if (distCenter <= 16f || distLeft <= 12f || distRight <= 12f)
                    {
                        bool isHighlight = (y > 38 && distCenter < 12f);
                        pixels[y * w + x] = isHighlight ? highlightGreen : vegGreen;
                    }
                }
            }
        }

        private static void DrawChoppedVegIcon(Color[] pixels, int w, int h, Vector2 center)
        {
            Color choppedGreen = new Color(0.35f, 0.88f, 0.4f, 1f);
            Color darkOutline = new Color(0.12f, 0.52f, 0.18f, 1f);

            // Diced green cubes cluster
            Vector2[] diceCenters = new Vector2[]
            {
                new Vector2(22, 22), new Vector2(42, 22),
                new Vector2(32, 34),
                new Vector2(20, 44), new Vector2(44, 44)
            };

            foreach (var dc in diceCenters)
            {
                int size = 6;
                for (int dy = -size; dy <= size; dy++)
                {
                    for (int dx = -size; dx <= size; dx++)
                    {
                        int px = Mathf.RoundToInt(dc.x + dx);
                        int py = Mathf.RoundToInt(dc.y + dy);

                        if (px >= 0 && px < w && py >= 0 && py < h)
                        {
                            bool isBorder = (Mathf.Abs(dx) == size || Mathf.Abs(dy) == size);
                            pixels[py * w + px] = isBorder ? darkOutline : choppedGreen;
                        }
                    }
                }
            }
        }
    }
}
