using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ManaxClient.Controls;

public class BestFitGrid:UniformGrid
{
    private double[] _rowHeights = [];
    private int _rows;
    private int _columns;

    protected override Size MeasureOverride(Size availableSize)
        {
            UpdateRowsAndColumns();

            double maxWidth = 0d;
            _rowHeights = new double[_rows];

            double availableWidthPerColumn = (availableSize.Width - (_columns - 1) * ColumnSpacing) / _columns;
            
            int x = FirstColumn;
            int y = 0;

            foreach (Control? child in Children)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                Size childAvailableSize = new(availableWidthPerColumn, double.PositiveInfinity);
                child.Measure(childAvailableSize);

                if (child.DesiredSize.Width > maxWidth)
                {
                    maxWidth = child.DesiredSize.Width;
                }

                if (child.DesiredSize.Height > _rowHeights[y])
                {
                    _rowHeights[y] = child.DesiredSize.Height;
                }

                x++;
                if (x < _columns) continue;
                x = 0;
                y++;
            }

            double totalWidth = maxWidth * _columns + ColumnSpacing * (_columns - 1);
            double totalHeight = _rowHeights.Sum() + RowSpacing * (_rows - 1);

            totalWidth = Math.Max(totalWidth, 0);
            totalHeight = Math.Max(totalHeight, 0);

            return new Size(totalWidth, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int x = FirstColumn;
            int y = 0;

            double columnSpacing = ColumnSpacing;
            double rowSpacing = RowSpacing;

            double width = Math.Max((finalSize.Width - (_columns - 1) * columnSpacing) / _columns, 0);
            
            double currentY = 0;

            foreach (Control? child in Children)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                double height = y < _rowHeights.Length ? _rowHeights[y] : 0;

                Rect rect = new(
                    x * (width + columnSpacing),
                    currentY,
                    width,
                    height);

                child.Arrange(rect);

                x++;

                if (x < _columns) continue;
                x = 0;
                currentY += height + rowSpacing;
                y++;
            }

            return finalSize;
        }

        private void UpdateRowsAndColumns()
        {
            _rows = Rows;
            _columns = Columns;
            
            if (FirstColumn >= _columns)
            {
                SetCurrentValue(FirstColumnProperty, 0);
            }

            int itemCount = FirstColumn + Children.Count(child => child.IsVisible);

            if (_rows == 0)
            {
                if (_columns == 0)
                {
                    _rows = _columns = (int)Math.Ceiling(Math.Sqrt(itemCount));
                }
                else
                {
                    _rows = Math.DivRem(itemCount, _columns, out int rem);

                    if (rem != 0)
                    {
                        _rows++;
                    }
                }
            }
            else if (_columns == 0)
            {
                _columns = Math.DivRem(itemCount, _rows, out int rem);

                if (rem != 0)
                {
                    _columns++;
                }
            }
        }
}