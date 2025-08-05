using ManaxLibrary.Logging;

namespace ManaxServer.Localization;

using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Globalization;

public static class Localizer
{
    private static string _language = "en";
    private static readonly List<string> Languages;
    private static readonly Dictionary<string, string> Fallback;
    private static Dictionary<string, string> _current;

    public static string GetString(string key)
    {
        _current.TryGetValue(key, out string? value);
        if (value != null) return value;
        Logger.LogWarning("The key '{Key}' was not found in the current language '{Language}'. Using fallback.",Environment.StackTrace);
        Fallback.TryGetValue(key, out value);
        if (value == null)
        {
            Logger.LogError("The key '{Key}' was not found in the fallback language. Returning the key itself.",new NotImplementedException(),Environment.StackTrace);
        }
        return value ?? key;
    }
    
    public static string Format(string key, params object[] args)
    {
        string template = GetString(key);
        return string.Format(template, args);
    }

    static Localizer()
    {
        Fallback = LoadFallback();
        Languages = LoadAvailableLanguages();
        _current = Fallback;
    }

    public static void SetLanguage(string language)
    {
        _language = language;
        _current = LoadCurrentLanguage();
    }

    public static string getCurrentLanguage()
    {
        return _language;
    }

    public static List<string> getAvailableLanguages()
    {
        return Languages;
    }

    private static Dictionary<string, string> LoadFallback()
    {
        return LoadResourceFile("manax");
    }

    private static Dictionary<string, string> LoadCurrentLanguage()
    {
        return LoadResourceFile($"manax.{_language}");
    }

    private static List<string> LoadAvailableLanguages()
    {
        List<string> languages = [];
        const string resourcePrefix = "manax.";

        string? origin = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (origin == null) return languages;
        
        string basePath = Path.Combine(origin, "Localization", "Languages");
        if (!Directory.Exists(basePath)) return languages;
        foreach (string file in Directory.GetFiles(basePath, "manax.*.resx"))
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            if (!filename.StartsWith(resourcePrefix)) continue;
            string lang = filename[resourcePrefix.Length..];
            if (!string.IsNullOrEmpty(lang))
            {
                languages.Add(lang);
            }
        }

        return languages;
    }

    private static Dictionary<string, string> LoadResourceFile(string baseName)
    {
        Dictionary<string, string> resources = new();
        
        try
        {
            ResourceManager resourceManager = new($"ManaxServer.Localization.Languages.{baseName}", 
                Assembly.GetExecutingAssembly());
            
            ResourceSet? resourceSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
            
            if (resourceSet != null)
            {
                foreach (System.Collections.DictionaryEntry entry in resourceSet)
                {
                    if (entry is { Key: string key, Value: string value })
                    {
                        resources[key] = value;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load resource File",e,Environment.StackTrace);
            return resources;
        }
        
        return resources;
    }
}