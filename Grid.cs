using Raylib_cs;
using PVZBloonTest.Bloons;
using PVZBloonTest.Plants;

namespace PVZBloonTest
{
    public class Grid
    {
        private readonly Plant?[,] _cells = new Plant?[Constants.GRID_COLS, Constants.GRID_ROWS];

        public bool PlacePlant(int col, int row, Plant plant)
        {
            if (!InBounds(col, row) || _cells[col, row] != null) return false;
            _cells[col, row] = plant;
            plant.Col = col;
            plant.Row = row;
            return true;
        }

        public void RemovePlant(int col, int row)
        {
            if (InBounds(col, row)) _cells[col, row] = null;
        }

        public Plant? GetPlant(int col, int row)
            => InBounds(col, row) ? _cells[col, row] : null;

        public void Clear()
        {
            for (int c = 0; c < Constants.GRID_COLS; c++)
                for (int r = 0; r < Constants.GRID_ROWS; r++)
                    _cells[c, r] = null;
        }

        public static bool ScreenToCell(int sx, int sy, out int col, out int row)
        {
            col = (sx - Constants.GRID_X) / Constants.CELL_W;
            row = (sy - Constants.GRID_Y) / Constants.CELL_H;
            return sx >= Constants.GRID_X && sy >= Constants.GRID_Y
                && col >= 0 && col < Constants.GRID_COLS
                && row >= 0 && row < Constants.GRID_ROWS;
        }

        public static float CellCenterX(int col) => Constants.GRID_X + col * Constants.CELL_W + Constants.CELL_W / 2f;
        public static float CellCenterY(int row) => Constants.GRID_Y + row * Constants.CELL_H + Constants.CELL_H / 2f;

        public static bool InBounds(int col, int row)
            => col >= 0 && col < Constants.GRID_COLS && row >= 0 && row < Constants.GRID_ROWS;

        public System.Collections.Generic.IEnumerable<Plant> AllPlants()
        {
            for (int c = 0; c < Constants.GRID_COLS; c++)
                for (int r = 0; r < Constants.GRID_ROWS; r++)
                    if (_cells[c, r] != null) yield return _cells[c, r]!;
        }

        public void Draw(bool nightMode)
        {
            Color b1 = nightMode ? new Color(30, 60, 30, 255) : new Color(100, 180, 80, 255);
            Color b2 = nightMode ? new Color(20, 45, 20, 255) : new Color(85, 160, 65, 255);
            Color gl = new Color(0, 0, 0, 40);
            int areaH = Constants.GRID_ROWS * Constants.CELL_H;

            // ── Grid cells ────────────────────────────────────────────────────
            for (int c = 0; c < Constants.GRID_COLS; c++)
                for (int r = 0; r < Constants.GRID_ROWS; r++)
                {
                    Color tile = ((c + r) % 2 == 0) ? b1 : b2;
                    int px = Constants.GRID_X + c * Constants.CELL_W;
                    int py = Constants.GRID_Y + r * Constants.CELL_H;
                    Raylib.DrawRectangle(px, py, Constants.CELL_W, Constants.CELL_H, tile);
                    Raylib.DrawRectangleLines(px, py, Constants.CELL_W, Constants.CELL_H, gl);
                }

            // ── House sidebar (left of grid, width = GRID_X = 200) ────────────
            Sprites.DrawCentered(Sprites.HouseArea,
                Constants.GRID_X / 2f, Constants.GRID_Y + areaH / 2f,
                Constants.GRID_X, areaH);

            // ── Bloon spawn strip (right of grid) ─────────────────────────────
            int spawnX = Constants.GRID_X + Constants.GRID_COLS * Constants.CELL_W;
            int spawnW = Constants.SCREEN_W - spawnX;
            Sprites.DrawCentered(Sprites.SpawnArea,
                spawnX + spawnW / 2f, Constants.GRID_Y + areaH / 2f,
                spawnW, areaH);
        }
    }
}
