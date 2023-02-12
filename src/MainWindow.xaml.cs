using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        private static readonly IDictionary<Difficulty, GameConfig> _configurations = new Dictionary<Difficulty, GameConfig>
        {
            { Difficulty.Easy, new GameConfig(10, 10, 10) },
            { Difficulty.Normal, new GameConfig(20, 20, 40) },
            { Difficulty.Hard, new GameConfig(20, 20, 100) },
        };

        public MainWindow()
        {
            InitializeComponent();
            CreateNewGame(_configurations[Difficulty.Normal]);
        }

        private void NewGameOnClick(object sender, RoutedEventArgs e)
        {
            if (rbEasy.IsChecked.Value)
                CreateNewGame(_configurations[Difficulty.Easy]);

            else if (rbMedium.IsChecked.Value)
                CreateNewGame(_configurations[Difficulty.Normal]);

            else if(rbHard.IsChecked.Value)
                CreateNewGame(_configurations[Difficulty.Hard]);

            else if (rbCustom.IsChecked.Value && TryParseCustomConfig(out var config))
                CreateNewGame(config);
        }

        private void CreateNewGame(GameConfig config)
        {
            var existingGame = MainGrid.Children.OfType<GameView>().SingleOrDefault();
            if (existingGame != null)
                MainGrid.Children.Remove(existingGame);

            var game = new GameView(config);

            Grid.SetRow(game, 1);

            DataContext = game;
            MainGrid.Children.Add(game);
        }

        private bool TryParseCustomConfig(out GameConfig config)
        {
            config = default;

            if (!int.TryParse(txtRows.Text, out int rows) || rows <= 0 || rows > 99)
            {
                MessageBox.Show("Rows count must be a valid integer between [1-99]");
                return false;
            }

            if (!int.TryParse(txtColumns.Text, out int columns) || columns <= 0 || columns > 99)
            {
                MessageBox.Show("Columns count must be a valid integer between [1-99]");
                return false;
            }

            if (!int.TryParse(txtBombs.Text, out int bombs) || bombs <= 0 || bombs > rows * columns)
            {
                MessageBox.Show($"Bombs count must be a valid integer between [1-{rows * columns}]");
                return false;
            }

            config = new GameConfig(rows, columns, bombs);
            return true;
        }
    }
}
