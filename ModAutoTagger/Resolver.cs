using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.Services;
using ModAutoTagger.Ipcs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ModAutoTagger;

public class Resolver(Config config, QuackEmotesIpc quackEmotesIpc, QuackPenumbraIpc quackPenumbraIpc, IPluginLog pluginLog)
{
    private static readonly EmoteData IDLE_PSEUDO_EMOTE = new("/idle", [
        "emote/pose00_loop", 
        "emote/pose01_loop", 
        "emote/pose02_loop", 
        "emote/pose03_loop", 
        "emote/pose04_loop", 
        "emote/pose05_loop", 
        "emote/pose06_loop"
    ]);

    private Config Config { get; init; } = config;
    private QuackEmotesIpc QuackEmotesIpc { get; init; } = quackEmotesIpc;
    private QuackPenumbraIpc QuackPenumbraIpc { get; init; } = quackPenumbraIpc;
    private IPluginLog PluginLog { get; init; } = pluginLog;

    private EmoteData[] Emotes { get; set; } = [];
    private ModData[] Mods { get; set; } = [];
    public Dictionary<ModData, HashSet<string>> TagsByMod { get; init; } = [];

    public void RefreshTagsByMod()
    {
        if (TryGetIpcData())
        {
            ResolveModTags();
        }
    }

    private bool TryGetIpcData()
    {
        try
        {
            Emotes = QuackEmotesIpc.GetList();
            Mods = QuackPenumbraIpc.GetList();
            return true;
        }
        catch (IpcNotReadyError)
        {
            PluginLog.Error("Failed to retrieve data from Quack plugin IPCs");
            return false;
        }
    }

    private void ResolveModTags()
    {
        TagsByMod.Clear();
        foreach (var mod in Mods)
        {
            HashSet<string> tags = [Config.GlobalTag];
            if (mod.settings.TryGetValue("files", out var modFilesObject))
            {
                if (modFilesObject is JObject modFilesJObject)
                {
                    var modFiles = modFilesJObject.ToObject<Dictionary<string, string>>();
                    if (modFiles != null)
                    {
                        AddEmoteCommandTags(tags, modFiles.Keys);
                    }
                }
            }
            if (mod.settings.TryGetValue("groupSettings", out var groupSettingsObject))
            {
                if (groupSettingsObject is JArray groupSettingsJArray)
                {
                    var groupSettings = groupSettingsJArray.ToObject<Dictionary<string, object>[]>();

                    if (groupSettings != null)
                    {
                        foreach (var groupSetting in groupSettings)
                        {
                            if (groupSetting.TryGetValue("options", out var optionsObject))
                            {
                                if (optionsObject is JArray optionsJObject)
                                {
                                    var options = optionsJObject.ToObject<Dictionary<string, object>[]>();
                                    if (options != null)
                                    {
                                        foreach (var option in options)
                                        {
                                            if (option.TryGetValue("files", out var optionFilesObject))
                                            {
                                                if (optionFilesObject is JObject optionFilesJObject)
                                                {
                                                    var optionFiles = optionFilesJObject.ToObject<Dictionary<string, string>>();
                                                    if (optionFiles != null)
                                                    {
                                                        AddEmoteCommandTags(tags, optionFiles.Keys);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            TagsByMod[mod] = tags;
        }
    }

    private void AddEmoteCommandTags(HashSet<string> tags, IEnumerable<string> gamePaths)
    {
        foreach (var gamePath in gamePaths)
        {
            foreach (var emote in Emotes.Concat([IDLE_PSEUDO_EMOTE]))
            {
                foreach (var emoteKey in emote.actionTimelineKeys.Concat(emote.poseKeys))
                {
                    if (gamePath.EndsWith($"{emoteKey}.pap"))
                    {
                        tags.Add(emote.command);
                    }
                }
            }
        }
    }
}
