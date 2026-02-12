using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using static System.Math;

namespace ManaxClient.Controls
{
    /// <summary>
    /// A WrapPanel that stretches its children to fill the available space in each line.
    /// Inherits from WrapPanel and overrides ArrangeOverride to distribute extra space equally among children.
    /// </summary>
    public class StretchWrapPanel : WrapPanel
    {
        protected override Size ArrangeOverride(Size finalSize)
        {
            Orientation orientation = Orientation;
            bool isHorizontal = orientation == Orientation.Horizontal;
            Avalonia.Controls.Controls children = Children;
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            double itemSpacing = ItemSpacing;
            double lineSpacing = LineSpacing;
            
            bool itemWidthSet = !double.IsNaN(itemWidth);
            bool itemHeightSet = !double.IsNaN(itemHeight);
            
            double availableSize = isHorizontal ? finalSize.Width : finalSize.Height;
            double linePosition = 0.0; // Position along secondary axis
            int lineStart = 0; // Index of first child in current line
            double currentLineSize = 0.0; // Size along primary axis
            double maxSecondarySize = 0.0; // Max size along secondary axis in current line
            
            for (int i = 0; i < children.Count; i++)
            {
                Control child = children[i];
                double childPrimarySize = isHorizontal
                    ? itemWidthSet ? itemWidth : child.DesiredSize.Width
                    : itemHeightSet ? itemHeight : child.DesiredSize.Height;
                double childSecondarySize = isHorizontal
                    ? itemHeightSet ? itemHeight : child.DesiredSize.Height
                    : itemWidthSet ? itemWidth : child.DesiredSize.Width;
                
                double spacing = i > lineStart && child.IsVisible ? itemSpacing : 0;
                
                // Check if we need to wrap to a new line
                if (i > lineStart && currentLineSize + spacing + childPrimarySize > availableSize)
                {
                    // Arrange the completed line with stretching
                    ArrangeLine(lineStart, i, linePosition, maxSecondarySize, isHorizontal, finalSize);
                    
                    // Start new line
                    linePosition += maxSecondarySize + lineSpacing;
                    lineStart = i;
                    currentLineSize = childPrimarySize;
                    maxSecondarySize = childSecondarySize;
                }
                else
                {
                    // Add to current line
                    currentLineSize += spacing + childPrimarySize;
                    maxSecondarySize = Max(maxSecondarySize, childSecondarySize);
                }
            }
            
            // Arrange the last line
            if (lineStart < children.Count)
            {
                ArrangeLine(lineStart, children.Count, linePosition, maxSecondarySize, isHorizontal, finalSize);
            }
            
            return finalSize;
            
            void ArrangeLine(int start, int end, double secondaryPos, double lineSecondarySize, bool horizontal, Size size)
            {
                double availablePrimary = horizontal ? size.Width : size.Height;
                
                // Count visible children and their total desired size
                int visibleCount = 0;
                double totalDesiredSize = 0;
                
                for (int i = start; i < end; i++)
                {
                    if (!children[i].IsVisible) continue;
                    visibleCount++;
                    double childSize = horizontal
                        ? itemWidthSet ? itemWidth : children[i].DesiredSize.Width
                        : itemHeightSet ? itemHeight : children[i].DesiredSize.Height;
                    totalDesiredSize += childSize;
                }
                
                // Calculate spacing and extra space
                double totalSpacing = visibleCount > 1 ? itemSpacing * (visibleCount - 1) : 0;
                double extraSpace = Max(0, availablePrimary - totalDesiredSize - totalSpacing);
                double stretchPerChild = visibleCount > 0 ? extraSpace / visibleCount : 0;
                
                // Arrange children
                double primaryPos = 0;
                
                for (int i = start; i < end; i++)
                {
                    Control child = children[i];
                    
                    if (!child.IsVisible)
                    {
                        child.Arrange(new Rect(0, 0, 0, 0));
                        continue;
                    }
                    
                    double desiredPrimary = horizontal
                        ? itemWidthSet ? itemWidth : child.DesiredSize.Width
                        : itemHeightSet ? itemHeight : child.DesiredSize.Height;
                    
                    double actualPrimary = desiredPrimary + stretchPerChild;
                    
                    Rect rect = horizontal
                        ? new Rect(primaryPos, secondaryPos, actualPrimary, lineSecondarySize)
                        : new Rect(secondaryPos, primaryPos, lineSecondarySize, actualPrimary);
                    
                    child.Arrange(rect);
                    primaryPos += actualPrimary + itemSpacing;
                }
            }
        }
    }
}