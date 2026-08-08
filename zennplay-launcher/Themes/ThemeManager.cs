using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using System;
using System.Linq;

namespace zennplay_launcher.Themes
{
    public static class ThemeManager
    {
        public static void SetColorScheme(string colorSchemeName)
        {
            var appResources = Application.Current?.Resources;
            if (appResources == null) return;

            var colorSchemeUri = new Uri($"avares://zennplay-launcher/Themes/ColorSchemes/{colorSchemeName}.axaml");
            var newColorSchemeDict = new ResourceInclude(colorSchemeUri) { Source = colorSchemeUri };

            if (appResources.MergedDictionaries.Count > 0)
            {
                appResources.MergedDictionaries[0] = newColorSchemeDict;
            }
            else
            {
                appResources.MergedDictionaries.Add(newColorSchemeDict);
            }
        }
    }
}