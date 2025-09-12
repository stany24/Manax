using System.Drawing;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ManaxServer.Models.Converters;

internal class ColorToInt32Converter() : ValueConverter<Color, int>(c => c.ToArgb(), v => Color.FromArgb(v));