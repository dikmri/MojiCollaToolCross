using Avalonia;
using Avalonia.Styling;
using System;
using System.IO;
using System.Text.Json;

namespace MojiCollaTool;

public enum AppTheme { System, Light, Dark }

public static class ThemeManager
{
    private static readonly string SettingsPath = Path.Combine(DataIO.GetExeDirPath(), "theme.json");

    public static AppTheme CurrentSetting { get; private set; } = AppTheme.System;

    public static void Initialize()
    {
        Load();
        Apply(CurrentSetting);
    }

    public static void SetTheme(AppTheme theme)
    {
        CurrentSetting = theme;
        Apply(theme);
        Save();
    }

    private static void Apply(AppTheme theme)
    {
        if (Application.Current == null) return;
        Application.Current.RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark  => ThemeVariant.Dark,
            _              => ThemeVariant.Default,
        };
    }

    private static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var setting = JsonSerializer.Deserialize<ThemeSetting>(json);
                if (setting != null) CurrentSetting = setting.Theme;
            }
        }
        catch { }
    }

    private static void Save()
    {
        try
        {
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new ThemeSetting { Theme = CurrentSetting }));
        }
        catch { }
    }

    private class ThemeSetting { public AppTheme Theme { get; set; } = AppTheme.System; }
}
