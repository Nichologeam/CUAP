using Archipelago.MultiClient.Net.Enums;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CUAP;
// This is entierly built on KaitoKid.ArchipelagoUtilities.AssetDownloader (https://github.com/agilbert1412/ArchipelagoUtilities/tree/main/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.AssetDownloader)
public class SpriteLogger : ILogger
{
    public void LogError(string message) => Startup.Logger.LogError(message);
    public void LogError(string message, Exception e) => Startup.Logger.LogError($"{message}, {e}");
    public void LogWarning(string message) => Startup.Logger.LogWarning(message);
    public void LogInfo(string message) => Startup.Logger.LogInfo(message);
    public void LogMessage(string message) => Startup.Logger.LogInfo(message);
    public void LogDebug(string message) => Startup.Logger.LogInfo(message);
    public void LogDebugPatchIsRunning(string patchedType, string patchedMethod, string patchType, string patchMethod, params object[] arguments)
        => Startup.Logger.LogInfo($"Debug Patch: [{patchedMethod}] -> [{patchMethod}]");
    public void LogDebug(string message, params object[] arguments) => Startup.Logger.LogInfo(message);
    public void LogErrorException(string prefixMessage, Exception ex, params object[] arguments) => Startup.Logger.LogError(ex);
    public void LogWarningException(string prefixMessage, Exception ex, params object[] arguments)
        => Startup.Logger.LogError(ex);
    public void LogErrorException(Exception ex, params object[] arguments) => Startup.Logger.LogError(ex);
    public void LogWarningException(Exception ex, params object[] arguments) => Startup.Logger.LogError(ex);
    public void LogErrorMessage(string message, params object[] arguments) => Startup.Logger.LogError(message);
    public void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments)
        => Startup.Logger.LogError(ex);
}

public class AssetItem(string game, string item, ItemFlags flags) : IAssetLocation
{
    public int GetSeed() => 0; // this is the seed used for random asset picking
    public string GameName { get; } = game; // this is the game the asset is from
    public string ItemName { get; } = item; // this is the item
    public ItemFlags ItemFlags { get; } = flags; // these are the flags the item has
}

public static class SpriteConverter
{
    public static ArchipelagoItemSprites itemSprites = new(new SpriteLogger(), stringToAliasConversion);
    private static ItemSpriteAliases stringToAliasConversion(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) // this game doesn't have any aliases, return an empty list
        {
            return new ItemSpriteAliases();
        }
        try
        {
            var obj = JsonConvert.DeserializeObject<Dictionary<string, List<ItemSpriteAlias>>>(json); // convert the json
            return obj != null && obj.TryGetValue("Aliases", out var aliases) // get the alias list from the json
                ? new ItemSpriteAliases() {Aliases = aliases} : new ItemSpriteAliases(); // if it doesn't exist, return empty. else, put it in an alias and return it
        }
        catch (Exception e)
        {
            Startup.Logger.LogWarning($"Alias parse failed: {e}");
            return new ItemSpriteAliases(); // error was hit. return empty list
        }
    }
}