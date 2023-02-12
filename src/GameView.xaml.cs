using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Minesweeper
{
    public partial class GameView : UserControl
    {
        private readonly GameConfig _config;
        private readonly BlockView[,] _blocks;
        private int _unexposedBlocksRemaining;
        public GameView(GameConfig config)
        {
            _config = config;
            _blocks = new BlockView[config.Rows, config.Columns];
            _unexposedBlocksRemaining = config.Rows * config.Columns;

            Initialize();
        }

        public static readonly DependencyProperty GameResultProperty =
            DependencyProperty.Register("GameResult", typeof(int), typeof(GameView), new PropertyMetadata());

        /// <summary>
        /// 0 - game in progress
        /// 1 - lost
        /// 2 - won
        /// </summary>
        public int GameStatus
        {
            get => (int)GetValue(GameResultProperty);
            set => SetValue(GameResultProperty, value);
        }

        private void Initialize()
        {
            InitializeComponent();
            AddBlocks();
            SetNeighbours();
            GenerateBombs();
        }

        public IEnumerable<BlockView> GetAllBlocks()
        {
            for (int i = 0; i < _config.Rows; i++)
                for (int j = 0; j < _config.Columns; j++)
                    yield return _blocks[i, j];
        }

        private void AddBlocks()
        {
            for (int i = 0; i < _config.Rows; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());

                for (int j = 0; j < _config.Columns; j++)
                {
                    if (i == 0)
                        GameGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    var block = new BlockView(i, j);
                    block.OnBombClicked += OnBombClicked;
                    block.OnExposed += OnExposed;

                    _blocks[i, j] = block;

                    Grid.SetRow(block, i);
                    Grid.SetColumn(block, j);
                    GameGrid.Children.Add(block);
                }
            }
        }

        private void SetNeighbours()
        {
            foreach (var block in _blocks)
            {
                block.AddNeighbour(TryGetBlock(block.Row - 1, block.Column));
                block.AddNeighbour(TryGetBlock(block.Row - 1, block.Column - 1));
                block.AddNeighbour(TryGetBlock(block.Row - 1, block.Column + 1));

                block.AddNeighbour(TryGetBlock(block.Row, block.Column - 1));
                block.AddNeighbour(TryGetBlock(block.Row, block.Column + 1));

                block.AddNeighbour(TryGetBlock(block.Row + 1, block.Column));
                block.AddNeighbour(TryGetBlock(block.Row + 1, block.Column - 1));
                block.AddNeighbour(TryGetBlock(block.Row + 1, block.Column + 1));
            }
        }

        private void GenerateBombs()
        {
            var generated = 0;
            var random = new Random(Guid.NewGuid().GetHashCode());
            while (generated != _config.Bombs)
            {
                var row = random.Next(0, _config.Rows);
                var column = random.Next(0, _config.Columns);

                if (!_blocks[row, column].IsBomb)
                {
                    _blocks[row, column].SetAsBomb();
                    _blocks[row, column].UpdateNeighbours();
                    generated++;
                }
            }
        }

        private BlockView TryGetBlock(int row, int column)
        {
            if (row < 0 || column < 0 || row >= _config.Rows || column >= _config.Columns)
                return null;

            return _blocks[row, column];
        }

        private void OnBombClicked(object sender, EventArgs e)
        {
            GameStatus = 1;
            IsHitTestVisible = false;

            foreach (var block in GetAllBlocks().Where(b => b.IsBomb))
                block.Expose();

            MessageBox.Show("Loser");
        }

        private void OnExposed(object sender, EventArgs e)
        {
            Interlocked.Decrement(ref _unexposedBlocksRemaining);

            if (_unexposedBlocksRemaining - _config.Bombs == 0 && GameStatus == 0)
            {
                GameStatus = 2;
                MessageBox.Show("Winner");
            }
        }
    }
}
