using System.Collections.Generic;
using Avalonia.Media;

namespace ManaxClient.Models.Theme;

public static class ThemePresets
{
    public static List<ManaxTheme> GetPresets()
    {
        return
        [
            new ManaxTheme("Bleu", Color.Parse("#007ACC"), Color.Parse("#6C757D")),
            new ManaxTheme("Violet", Color.Parse("#6F42C1"), Color.Parse("#6C757D")),
            new ManaxTheme("Rouge", Color.Parse("#DC3545"), Color.Parse("#6C757D")),
            new ManaxTheme("Vert", Color.Parse("#28A745"), Color.Parse("#6C757D"))
        ];
    }
}