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
                
                // Count visible children
                int visibleCount = 0;
                
                for (int i = start; i < end; i++)
                {
                    Control child = children[i];
                    if (!child.IsVisible) continue;
                    
                    visibleCount++;
                }
                
                if (visibleCount == 0) return;
                
                // Calculate spacing and available space for stretching
                double totalSpacing = visibleCount > 1 ? itemSpacing * (visibleCount - 1) : 0;
                double remainingSpace = availablePrimary - totalSpacing;
                
                // Calculate target uniform size for each child (equal distribution)
                double uniformSize = remainingSpace / visibleCount;
                
                // Check if uniform size respects minimum and maximum sizes
                // If total minimum size exceeds available space, we need to adjust
                double[] childSizes = new double[end - start];
                bool[] isFlexible = new bool[end - start];
                int flexibleChildCount = 0;
                
                // First pass: assign sizes, respecting minimums
                for (int i = start; i < end; i++)
                {
                    Control child = children[i];
                    if (!child.IsVisible) continue;
                    
                    double minSize = horizontal
                        ? itemWidthSet ? itemWidth : child.DesiredSize.Width
                        : itemHeightSet ? itemHeight : child.DesiredSize.Height;
                    
                    if (uniformSize >= minSize)
                    {
                        // Child can use uniform size
                        childSizes[i - start] = uniformSize;
                        isFlexible[i - start] = true;
                        flexibleChildCount++;
                    }
                    else
                    {
                        // Child needs its minimum size
                        childSizes[i - start] = minSize;
                        isFlexible[i - start] = false;
                    }
                }
                
                // Iteratively redistribute space, respecting maximum sizes
                bool hasChanges = true;
                while (flexibleChildCount > 0 && hasChanges)
                {
                    hasChanges = false;
                    
                    // Calculate how much space should go to flexible children
                    double spaceForFlexible = remainingSpace;
                    for (int i = start; i < end; i++)
                    {
                        if (!children[i].IsVisible) continue;
                        if (!isFlexible[i - start])
                        {
                            spaceForFlexible -= childSizes[i - start];
                        }
                    }
                    
                    double sizePerFlexible = spaceForFlexible / flexibleChildCount;
                    
                    // Check if any flexible child would exceed its max size
                    for (int i = start; i < end; i++)
                    {
                        Control child = children[i];
                        if (!child.IsVisible || !isFlexible[i - start]) continue;
                        
                        double maxSize = horizontal
                            ? double.IsNaN(child.MaxWidth) ? double.PositiveInfinity : child.MaxWidth
                            : double.IsNaN(child.MaxHeight) ? double.PositiveInfinity : child.MaxHeight;
                        
                        if (sizePerFlexible > maxSize)
                        {
                            // This child reached its maximum size
                            childSizes[i - start] = maxSize;
                            isFlexible[i - start] = false;
                            flexibleChildCount--;
                            hasChanges = true;
                        }
                        else
                        {
                            // Child can take the calculated size
                            childSizes[i - start] = sizePerFlexible;
                        }
                    }
                }
                
                // Arrange children with calculated sizes
                double primaryPos = 0;
                
                for (int i = start; i < end; i++)
                {
                    Control child = children[i];
                    
                    if (!child.IsVisible)
                    {
                        child.Arrange(new Rect(0, 0, 0, 0));
                        continue;
                    }
                    
                    double childSize = childSizes[i - start];
                    
                    Rect rect = horizontal
                        ? new Rect(primaryPos, secondaryPos, childSize, lineSecondarySize)
                        : new Rect(secondaryPos, primaryPos, lineSecondarySize, childSize);
                    
                    child.Arrange(rect);
                    primaryPos += childSize + itemSpacing;
                }
            }
        }
    }
}