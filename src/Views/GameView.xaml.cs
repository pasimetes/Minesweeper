using Minesweeper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper.Views
{
    public partial class GameView : UserControl
    {
        private readonly GameConfig _config;
        private readonly IDictionary<CellView, ICollection<CellView>> _cellsAdjacencies;
        private readonly Timer _timer;

        public GameView(GameConfig config)
        {
            _config = config;
            _cellsAdjacencies = new Dictionary<CellView, ICollection<CellView>>();
            _timer = new Timer();

            FlagsRemaining = config.Bombs;
            UncoveredCellsRemaining = (config.Rows * config.Columns) - config.Bombs;

            Initialize();
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(GameState), typeof(GameView), new PropertyMetadata());

        public static readonly DependencyProperty FlagsRemainingProperty =
            DependencyProperty.Register("FlagsRemaining", typeof(int), typeof(GameView), new PropertyMetadata());

        public static readonly DependencyProperty UncoveredCellsRemainingProperty =
            DependencyProperty.Register("UncoveredCellsRemaining", typeof(int), typeof(GameView), new PropertyMetadata());

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

        public int UncoveredCellsRemaining
        {
            get => (int)GetValue(UncoveredCellsRemainingProperty);
            set => SetValue(UncoveredCellsRemainingProperty, value);
        }

        public int TimeElapsed
        {
            get => (int)GetValue(TimeElapsedProperty);
            set => SetValue(TimeElapsedProperty, value);
        }

        private void Initialize()
        {
            InitializeComponent();
            AddCells();
            SetNeighbours();
            GenerateBombs();
        }

        private void AddCells()
        {
            for (int i = 0; i < _config.Rows; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());

                for (int j = 0; j < _config.Columns; j++)
                {
                    if (i == 0)
                        GameGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    var cell = new CellView(i, j);
                    cell.MouseDown += Cell_MouseDown;

                    _cellsAdjacencies.Add(cell, new List<CellView>());

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    GameGrid.Children.Add(cell);
                }
            }
        }

        private void SetNeighbours()
        {
            foreach (var kvp in _cellsAdjacencies)
            {
                var cell = kvp.Key;
                var neighbours = kvp.Value;

                neighbours.Add(GetCellByPlacementOrDefault(cell.Row - 1, cell.Column));
                neighbours.Add(GetCellByPlacementOrDefault(cell.Row - 1, cell.Column - 1));
                neighbours.Add(GetCellByPlacementOrDefault(cell.Row - 1, cell.Column + 1));

                neighbours.Add(GetCellByPlacementOrDefault(cell.Row, cell.Column - 1));
                neighbours.Add(GetCellByPlacementOrDefault(cell.Row, cell.Column + 1));

                neighbours.Add(GetCellByPlacementOrDefault(cell.Row + 1, cell.Column));
                neighbours.Add(GetCellByPlacementOrDefault(cell.Row + 1, cell.Column - 1));
                neighbours.Add(GetCellByPlacementOrDefault(cell.Row + 1, cell.Column + 1));
            }
        }

        public void UpdateNeighbours(CellView cell)
        {
            foreach (var neighbour in _cellsAdjacencies[cell].Where(n => n != null))
                switch (neighbour.CellType)
                {
                    case CellType.Bomb:
                        continue;

                    case CellType.Number:
                        neighbour.Increment();
                        break;

                    case CellType.None:
                        neighbour.SetAsNumber();
                        neighbour.Increment();
                        break;
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
                var prefferedCell = GetCellByPlacementOrDefault(row, column);

                if (prefferedCell != null && prefferedCell.CellType != CellType.Bomb)
                {
                    prefferedCell.SetAsBomb();
                    UpdateNeighbours(prefferedCell);
                    generated++;
                }
            }
        }

        private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cell = (CellView)sender;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (cell.IsFlagged)
                        return;

                    switch (cell.CellType)
                    {
                        case CellType.Bomb:
                            State = GameState.Lost;
                            UncoverAllCells();
                            EndGame();
                            return;

                        case CellType.Number:
                            UncoverCell(cell);
                            return;
                    }

                    SpreadUncover(cell);
                    break;

                case MouseButton.Right:
                    if (cell.IsUncovered)
                        return;

                    cell.ToggleFlag();
                    FlagsRemaining += cell.IsFlagged ? -1 : 1;
                    break;
            }
        }

        private void UncoverCell(CellView cell)
        {
            cell.Uncover();

            if (State == GameState.Ready)
            {
                State = GameState.InProgress;
                BeginTimer();
            }

            UncoveredCellsRemaining--;

            CheckForWin();
        }

        private void UncoverAllCells()
        {
            foreach (var cell in GetAllCells())
            {
                UncoverCell(cell);
            }
        }

        public void SpreadUncover(CellView cell)
        {
            if (cell == null || cell.IsFlagged || cell.IsUncovered || cell.CellType == CellType.Bomb)
                return;

            UncoverCell(cell);

            if (cell.CellType == CellType.Number)
                return;

            foreach (var neighbour in _cellsAdjacencies[cell])
                SpreadUncover(neighbour);
        }

        private IEnumerable<CellView> GetAllCells()
        {
            return _cellsAdjacencies.Keys;
        }

        private CellView GetCellByPlacementOrDefault(int row, int column)
        {
            if (row < 0 || column < 0 || row >= _config.Rows || column >= _config.Columns)
                return null;

            return _cellsAdjacencies.Keys.SingleOrDefault(b => b.Row == row && b.Column == column);
        }

        private void CheckForWin()
        {
            if (UncoveredCellsRemaining == 0 && State == GameState.InProgress)
            {
                State = GameState.Won;
                UncoverAllCells();
                EndGame();
            }
        }

        private void EndGame()
        {
            StopTimer();

            IsHitTestVisible = false;

            switch (State)
            {
                case GameState.Won:
                    MessageBox.Show("Winner");
                    break;

                case GameState.Lost:
                    MessageBox.Show("Loser");
                    break;
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
