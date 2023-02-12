using Minesweeper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Minesweeper.Views
{
    public partial class GameView : UserControl
    {
        private readonly GameConfig _config;
        private readonly BlockView[,] _blocks;
        private readonly Timer _timer;

        public GameView(GameConfig config)
        {
            _config = config;
            _blocks = new BlockView[config.Rows, config.Columns];
            _timer = new Timer();

            FlagsRemaining = config.Bombs;
            UncoveredBlocksRemaining = (config.Rows * config.Columns) - config.Bombs;

            Initialize();
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(GameState), typeof(GameView), new PropertyMetadata());

        public static readonly DependencyProperty FlagsRemainingProperty =
           DependencyProperty.Register("FlagsRemaining", typeof(int), typeof(GameView), new PropertyMetadata());

        public static readonly DependencyProperty UncoveredBlocksRemainingProperty =
          DependencyProperty.Register("UncoveredBlocksRemaining", typeof(int), typeof(GameView), new PropertyMetadata());

        public static readonly DependencyProperty TimeElapsedProperty =
           DependencyProperty.Register("TimeElapsed", typeof(int), typeof(GameView), new PropertyMetadata());

        public GameState State
        {
            get => (GameState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public int FlagsRemaining
        {
            get => (int)GetValue(FlagsRemainingProperty);
            set => SetValue(FlagsRemainingProperty, value);
        }

        public int UncoveredBlocksRemaining
        {
            get => (int)GetValue(UncoveredBlocksRemainingProperty);
            set => SetValue(UncoveredBlocksRemainingProperty, value);
        }

        public int TimeElapsed
        {
            get => (int)GetValue(TimeElapsedProperty);
            set => SetValue(TimeElapsedProperty, value);
        }

        private void Initialize()
        {
            InitializeComponent();
            AddBlocks();
            SetNeighbours();
            GenerateBombs();
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
                    block.OnExplode += OnExplode;
                    block.OnUncover += OnUncover;
                    block.OnFlag += OnFlag;

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
                block.AddNeighbour(TryGetBlockByPlacement(block.Row - 1, block.Column));
                block.AddNeighbour(TryGetBlockByPlacement(block.Row - 1, block.Column - 1));
                block.AddNeighbour(TryGetBlockByPlacement(block.Row - 1, block.Column + 1));

                block.AddNeighbour(TryGetBlockByPlacement(block.Row, block.Column - 1));
                block.AddNeighbour(TryGetBlockByPlacement(block.Row, block.Column + 1));

                block.AddNeighbour(TryGetBlockByPlacement(block.Row + 1, block.Column));
                block.AddNeighbour(TryGetBlockByPlacement(block.Row + 1, block.Column - 1));
                block.AddNeighbour(TryGetBlockByPlacement(block.Row + 1, block.Column + 1));
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

                if (_blocks[row, column].BlockType != BlockType.Bomb)
                {
                    _blocks[row, column].SetAsBomb();
                    _blocks[row, column].UpdateNeighbours();
                    generated++;
                }
            }
        }

        private BlockView TryGetBlockByPlacement(int row, int column)
        {
            if (row < 0 || column < 0 || row >= _config.Rows || column >= _config.Columns)
                return null;

            return _blocks[row, column];
        }

        public IEnumerable<BlockView> GetAllBlocks()
        {
            for (int i = 0; i < _config.Rows; i++)
                for (int j = 0; j < _config.Columns; j++)
                    yield return _blocks[i, j];
        }

        private void OnExplode(object sender, EventArgs e)
        {
            UncoverAllBombs();
            EndGame(GameState.Lost);
        }

        private void OnUncover(object sender, EventArgs e)
        {
            if (State == GameState.Ready)
            {
                State = GameState.InProgress;
                BeginTimer();
            }

            UncoveredBlocksRemaining--;

            if (UncoveredBlocksRemaining == 0 && State == GameState.InProgress)
                EndGame(GameState.Won);
        }

        private void OnFlag(object sender, bool e)
        {
            FlagsRemaining += e ? -1 : 1;
        }

        private void EndGame(GameState state)
        {
            StopTimer();

            State = state;
            IsHitTestVisible = false;

            switch (state)
            {
                case GameState.Won:
                    MessageBox.Show("Winner");
                    break;
                case GameState.Lost:
                    MessageBox.Show("Loser");
                    break;
            }
        }

        private void UncoverAllBombs()
        {
            foreach (var block in GetAllBlocks().Where(b => b.BlockType == BlockType.Bomb))
            {
                block.OnUncover -= OnUncover;
                block.OnExplode -= OnExplode;
                block.OnFlag -= OnFlag;
                block.Uncover();
            }
        }

        private void BeginTimer()
        {
            _timer.Interval = 1000;
            _timer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TimeElapsed++;
                });
            };
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
        }
    }
}
