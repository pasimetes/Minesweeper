namespace Minesweeper
{
    public class GameConfig
    {
        public GameConfig(int rows, int columns, int bombs)
        {
            Rows = rows;
            Columns = columns;
            Bombs = bombs;
        }

        public int Rows { get; }

        public int Columns { get; }

        public int Bombs { get; }
    }
}
