using Minesweeper.Enums;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper.Views
{
    public partial class BlockView : UserControl
    {
        public event EventHandler OnExplode;
        public event EventHandler OnUncover;
        public event EventHandler<bool> OnFlag;

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

        public static readonly DependencyProperty BlockTypeProperty =
           DependencyProperty.Register("BlockType", typeof(BlockType), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty IsUncoveredProperty =
            DependencyProperty.Register("IsUncovered", typeof(bool), typeof(BlockView), new PropertyMetadata());
       
        public static readonly DependencyProperty IsFlaggedProperty =
            DependencyProperty.Register("IsFlagged", typeof(bool), typeof(BlockView), new PropertyMetadata());

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int), typeof(BlockView), new PropertyMetadata());

        public BlockType BlockType
        {
            get => (BlockType)GetValue(BlockTypeProperty);
            set => SetValue(BlockTypeProperty, value);
        }

        public bool IsUncovered
        {
            get => (bool)GetValue(IsUncoveredProperty);
            set => SetValue(IsUncoveredProperty, value);
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

        public void SetAsBomb()
        {
            BlockType = BlockType.Bomb;
            Number = 0;
        }

        public void SetAsNumber()
        {
            BlockType = BlockType.Number;
            Number = 0;
        }

        public void Uncover()
        {
            if (IsUncovered)
                return;

            IsUncovered = true;
            OnUncover?.Invoke(this, EventArgs.Empty);
        }

        public void Increment()
        {
            Number++;
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
                switch (neighbour.BlockType)
                {
                    case BlockType.Bomb:
                        continue;

                    case BlockType.Number:
                        neighbour.Increment();
                        break;

                    case BlockType.None:
                        neighbour.SetAsNumber();
                        neighbour.Increment();
                        break;
                }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (IsFlagged)
                        return;

                    switch (BlockType)
                    {
                        case BlockType.Bomb:
                            OnExplode?.Invoke(this, EventArgs.Empty);
                            return;
                        case BlockType.Number:
                            Uncover();
                            return;
                    }

                    SpreadUncover(this);
                    break;

                case MouseButton.Right:
                    if (IsUncovered)
                        return;

                    IsFlagged = !IsFlagged;
                    OnFlag?.Invoke(this, IsFlagged);
                    break;
            }
        }

        public void SpreadUncover(BlockView block)
        {
            if (block.IsFlagged || block.IsUncovered || block.BlockType == BlockType.Bomb)
                return;

            block.Uncover();

            if (block.BlockType == BlockType.Number)
                return;

            foreach (var neighbour in block.Neighbours)
                SpreadUncover(neighbour);
        }
    }
}
