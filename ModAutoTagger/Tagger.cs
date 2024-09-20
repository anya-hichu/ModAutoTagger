using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ModAutoTagger.Ipcs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModAutoTagger;

public class Tagger
{
    private string ModDataConfigPathTemplate { get; init; }
    private IPluginLog PluginLog { get; init; }
    
    public Tagger(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
    {
        var pluginConfigsDirectory = Path.Combine(pluginInterface.GetPluginConfigDirectory(), "..");
        ModDataConfigPathTemplate = Path.GetFullPath(Path.Combine(pluginConfigsDirectory, "Penumbra\\mod_data\\{0}.json"));

        PluginLog = pluginLog; 
    }

    public void OverrideAll(Dictionary<ModData, HashSet<string>> tagsByMod)
    {
        foreach (var entry in tagsByMod.Where(e => CanOverride(e.Key, e.Value)))
        {
            Override(entry.Key, entry.Value);
        }
    }

    public void Override(ModData mod, HashSet<string> tags)
    {
        PluginLog.Debug($"Override tags ({string.Join(", ", tags)}) on mod '{mod.name}' ({mod.path})");
        var modDataConfigPath = ModDataConfigPathTemplate.Format(mod.dir);
        var modDataConfig = DeserializeJsonFile(modDataConfigPath);
        modDataConfig["LocalTags"] = JArray.FromObject(tags);
        PluginLog.Verbose($"Rewrite '{mod.dir}' mod data file with overriden tags: {modDataConfig}");
        SerializeJsonFile(modDataConfigPath, modDataConfig);
    }

    public void AppendAll(Dictionary<ModData, HashSet<string>> tagsByMod)
    {
        foreach (var entry in tagsByMod.Where(e => CanAppend(e.Key, e.Value)))
        {
            Append(entry.Key, entry.Value);
        }
    }

    public void Append(ModData mod, HashSet<string> tags)
    {
        PluginLog.Debug($"Append tags ({string.Join(", ", tags)}) on mod '{mod.name}' ({mod.path})");
        var modDataConfigPath = ModDataConfigPathTemplate.Format(mod.dir);
        var modDataConfig = DeserializeJsonFile(modDataConfigPath);
        JArray localTagsJArray = JArray.FromObject(modDataConfig["LocalTags"]);
        var localTags = localTagsJArray.ToObject<HashSet<string>>();
        if (localTags != null)
        {
            modDataConfig["LocalTags"] = JArray.FromObject(localTags.Union(tags));
        }
        PluginLog.Verbose($"Rewrite '{mod.dir}' mod data file with appended tags: {modDataConfig}");
        SerializeJsonFile(modDataConfigPath, modDataConfig);
    }

    private static dynamic DeserializeJsonFile(string path)
    {
        return JsonConvert.DeserializeObject(File.ReadAllText(path))!;
    }

    private static void SerializeJsonFile(string path, dynamic data)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public static bool CanOverride(ModData mod, HashSet<string> tags)
    {
        return !tags.SetEquals(mod.localTags);
    }

    public static bool CanAppend(ModData mod, HashSet<string> tags)
    {
        return tags.Except(mod.localTags).Any();
    }
}
