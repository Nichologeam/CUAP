using CUAP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public static class APLocale
{
    private static APLanguage lang; // the language JSON file as a dictionary
    private static bool successful = false;
    public class APLanguage
    {
        public string langName; // name in plaintext (ex: "English"). language code will be the filename (ex: EN.JSON)
        public Dictionary<string, string> UI = new Dictionary<string, string>(); // strings used in all forms of UI, like item receive messages or hints
        public Dictionary<string, string> Commands = new Dictionary<string, string>(); // strings used in custom commands, like `apchat`
        public Dictionary<string, string> Messages = new Dictionary<string, string>(); // strings used in onscreen messages, like server countdowns or deathlinks
        public Dictionary<string, string> Errors = new Dictionary<string, string>(); // strings used when things go wrong, like error messages or debug outputs
    }

    public enum APLanguageType
    {
        UI,
        Commands,
        Messages,
        Errors
    }

    public static void LoadLang(string filename) // when given a filename, return the contents of the JSON file as a dict
    {
        successful = false;
        string path = Path.Combine(BepInEx.Paths.PluginPath, "CUAP", "lang", filename); // assume the file is in the lang folder (it realistically should never be anywhere else)
        try
        {
            string file = File.ReadAllText(path); // get the contents of the file as a string
            APLanguage loaded = JsonConvert.DeserializeObject<APLanguage>(file); // save it to the lang variable
            successful = true;
            loaded.UI ??= new Dictionary<string, string>(); // make sure the dicts actually exist and aren't missing
            loaded.Commands ??= new Dictionary<string, string>();
            loaded.Messages ??= new Dictionary<string, string>();
            loaded.Errors ??= new Dictionary<string, string>();
            lang = loaded;
        }
        catch (Exception ex)
        {
            if (successful)
            {
                Startup.Logger.LogError($"Locale {filename} was loaded, but is missing sections!"); // error in english
                APCanvas.EnqueueArchipelagoNotification($"Locale {filename} was loaded, but is missing sections!", 3);
                return;
            }
            Startup.Logger.LogError($"Failed to load locale from {filename} at {path}! Make sure the mod is installed in the correct folder!\n{ex}"); // error in english
            APCanvas.EnqueueArchipelagoNotification($"Failed to load locale from {filename} at {path}! Make sure the mod is installed in the correct folder!<br>{ex}",3);
            return; // the code below this will always error if this catch is triggered. return early to avoid that
        }
    }

    public static string Get(string key, APLanguageType type)
    {
        if (lang == null) // no language loaded?
        {
            Startup.Logger.LogError("Language file not loaded!"); // error in english
            return $"[LANG NOT LOADED]";
        }
        var dict = GetDictionary(type); // get the right dict for the type specified
        if (dict != null && dict.TryGetValue(key, out var value))
        {
            return value; // if it exists, return the string
        }
        Startup.Logger.LogError($"{key} is not in the language dictionary! Type: {type}");
        return $"[LANG MISSING {type.ToString().ToUpper()}:{key}]"; // if it doesn't exist, return error
    }

    private static Dictionary<string, string> GetDictionary(APLanguageType type)
    {
        return type switch
        {
            APLanguageType.UI => lang.UI,
            APLanguageType.Commands => lang.Commands,
            APLanguageType.Messages => lang.Messages,
            APLanguageType.Errors => lang.Errors,
            _ => null
        };
    }
}