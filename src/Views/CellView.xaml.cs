using Minesweeper.Enums;
using System.Windows;
using System.Windows.Controls;

namespace Minesweeper.Views
{
    public partial class CellView : UserControl
    {
        public readonly int Row;
        public readonly int Column;

        public CellView(int row, int column)
        {
            Row = row;
            Column = column;

            InitializeComponent();
        }

        public static readonly DependencyProperty CellTypeProperty =
           DependencyProperty.Register("CellType", typeof(CellType), typeof(CellView), new PropertyMetadata());

        public static readonly DependencyProperty IsUncoveredProperty =
            DependencyProperty.Register("IsUncovered", typeof(bool), typeof(CellView), new PropertyMetadata());
       
        public static readonly DependencyProperty IsFlaggedProperty =
            DependencyProperty.Register("IsFlagged", typeof(bool), typeof(CellView), new PropertyMetadata());

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int), typeof(CellView), new PropertyMetadata());

        public CellType CellType
        {
            get => (CellType)GetValue(CellTypeProperty);
            set => SetValue(CellTypeProperty, value);
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
            CellType = CellType.Bomb;
            Number = 0;
        }

        public void SetAsNumber()
        {
            CellType = CellType.Number;
            Number = 0;
        }

        public void Uncover()
        {
            if (IsUncovered)
                return;

            IsUncovered = true;
        }

        public void ToggleFlag()
        {
            IsFlagged = !IsFlagged;
        }

        public void Increment()
        {
            Number++;
        }
    }
}
