using System;
using System.Collections.Generic;
using System.IO;

using Android.Content;
using Android.Graphics;
using Newtonsoft.Json;
using Path = Android.Graphics.Path;

namespace CitraRooms
{
    class Utility
    {
        private static Dictionary<String, String> games = new Dictionary<String, String>();

        public static void ImportGames(Stream stream)
        {
            var serializer = new JsonSerializer();
            var nGames = new Dictionary<String, String>();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                nGames = serializer.Deserialize<Dictionary<String, String>>(jsonTextReader);
            }

            foreach (KeyValuePair<string, string> g in nGames) { games.TryAdd(g.Key, g.Value); }
        }

        public static String MatchAvailableGame(long gameNum, Context c)
        {
            String name = null;
            games.TryGetValue(gameNum.ToString(), out name);
            return name;
        }

        
        public static Bitmap RoundImage(Bitmap source, Bitmap overlay)
        {
            Bitmap final = Bitmap.CreateBitmap(48, 48, Bitmap.Config.Argb8888);

            source = Bitmap.CreateScaledBitmap(source, 48, 48, false);

            Canvas canvas = new Canvas(final);
            // Disabled because antialiasing doesn't work properly
            /*Path path = new Path();
            path.AddCircle(24, 24, 24, Path.Direction.Cw);
            canvas.ClipPath(path);
            canvas.Save();*/

            Paint customPaint = new Paint();
            customPaint.AntiAlias = true;
            customPaint.FilterBitmap = true;
            canvas.DrawBitmap(source, 0f, 0f, customPaint);
            if (overlay != null)
            {
                overlay = Bitmap.CreateScaledBitmap(overlay, 48, 48, false);
                canvas.DrawBitmap(overlay, 0f, 0f, customPaint);
            }

            canvas.Dispose();

            return final;
        }

        public static String[] LightColors = {"#E57373", "#F06292", "#CE93D8", "#FF8A80", "#FF80AB", "#EA80FC", "#B39DDB", "#9FA8DA",
                                        "#64B5F6", "#B388FF", "#8C9EFF", "#82B1FF", "#4FC3F7", "#4DD0E1", "#4DB6AC",
                                        "#84FFFF", "#A7FFEB", "#81C784", "#AED581", "#DCE775", "#69F0AE", "#B2FF59",
                                        "#EEFF41", "#FFF176", "#FFD54F", "#FFB74D", "#FFFF00", "#FFD740", "#FFAB40",
                                        "#FF8A65", "#A1887F", "#E0E0E0", "#FF6E40", "#90A4AE", "#EF9A9A", "#F48FB1",
                                        "#80D8FF", "#E1BEE7", "#D1C4E9", "#C5CAE9", "#90CAF9", "#81D4FA", "#80DEEA",
                                        "#80CBC4", "#A5D6A7", "#A5D6A7", "#E6EE9C", "#FFF59D", "#FFF59D", "#FFCC80"};
    }
}