using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    public partial class BlockView : UserControl
    {
        public event EventHandler OnBombClicked;
        public event EventHandler OnExposed;

        public readonly int Row;
        public readonly int Column;
        public readonly ICollection<BlockView> Neighbours;

        public BlockView(int row, int column)
        {
            Row = row;
            Column = column;
            Neighbours = new List<BlockView>();

            InitializeComponent();
        }

        public static readonly DependencyProperty IsExposedProperty =
            DependencyProperty.Register("IsExposed", typeof(bool), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty IsBombProperty =
            DependencyProperty.Register("IsBomb", typeof(bool), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty IsNumberProperty =
            DependencyProperty.Register("IsNumber", typeof(bool), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty IsFlaggedProperty =
            DependencyProperty.Register("IsFlagged", typeof(bool), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BlockView), new PropertyMetadata());

        public bool IsExposed
        {
            get => (bool)GetValue(IsExposedProperty);
            set => SetValue(IsExposedProperty, value);
        }

        public bool IsBomb
        {
            get => (bool)GetValue(IsBombProperty);
            set => SetValue(IsBombProperty, value);
        }

        public bool IsNumber
        {
            get => (bool)GetValue(IsNumberProperty);
            set => SetValue(IsNumberProperty, value);
        }

        public bool IsFlagged
        {
            get => (bool)GetValue(IsFlaggedProperty);
            set => SetValue(IsFlaggedProperty, value);
        }

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public void SetAsBomb()
        {
            IsBomb = true;
            IsNumber = false;
            Number = 0;
        }

        public void SetAsNumber()
        {
            IsBomb = false;
            IsNumber = true;
            Number = 0;
        }

        public void Expose()
        {
            if (IsExposed)
                return;

            IsExposed = true;
            OnExposed?.Invoke(this, EventArgs.Empty);
        }

        public void Increment()
        {
            Number++;
            Text = Number.ToString();
        }

        public void AddNeighbour(BlockView neighbour)
        {
            if (neighbour == null)
                return;

            Neighbours.Add(neighbour);
        }

        public void UpdateNeighbours()
        {
            foreach (var neighbour in Neighbours)
            {
                if (neighbour.IsBomb)
                    continue;

                if (neighbour.IsNumber)
                    neighbour.Increment();

                else
                {
                    neighbour.SetAsNumber();
                    neighbour.Increment();
                }
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (IsFlagged)
                        return;

                    if (IsBomb)
                    {
                        Expose();
                        OnBombClicked?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    if (!IsNumber)
                    {
                        SpreadExpose(this);
                        return;
                    }

                    Expose();
                    break;

                case MouseButton.Right:
                    if (IsExposed)
                        return;

                    IsFlagged = !IsFlagged;

                    break;
            }
        }

        public void SpreadExpose(BlockView block)
        {
            if (block.IsFlagged || block.IsExposed || block.IsBomb)
                return;

            block.Expose();

            if (block.IsNumber)
                return;

            foreach (var neighbour in block.Neighbours)
            {
                SpreadExpose(neighbour);
            }
        }
    }
}
