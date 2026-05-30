using Raylib_cs;
using System.Numerics;

namespace PVZBloonTest
{
    /// <summary>
    /// Global font registry. Call Load() once after Raylib.InitWindow()
    /// and Unload() just before Raylib.CloseWindow().
    ///
    /// Usage:
    ///   Draw    — replaces Raylib.DrawText(text, x, y, size, color)
    ///   Measure — replaces Raylib.MeasureText(text, size)
    /// </summary>
    public static class Fonts
    {
        private static Font _main;
        private static bool _loaded = false;

        public static void Load(string path)
        {
            _main   = Raylib.LoadFont(path);
            _loaded = true;
        }

        public static void Unload()
        {
            if (_loaded) Raylib.UnloadFont(_main);
            _loaded = false;
        }

        /// <summary>Draw text using the loaded custom font.</summary>
        public static void Draw(string text, int x, int y, int size, Color color)
        {
            if (_loaded)
                Raylib.DrawTextEx(_main, text, new Vector2(x, y), size, 1f, color);
            else
                Raylib.DrawText(text, x, y, size, color);
        }

        /// <summary>Measure text width using the loaded custom font.</summary>
        public static int Measure(string text, int size)
        {
            if (_loaded)
                return (int)Raylib.MeasureTextEx(_main, text, size, 1f).X;
            return Raylib.MeasureText(text, size);
        }
    }
}
