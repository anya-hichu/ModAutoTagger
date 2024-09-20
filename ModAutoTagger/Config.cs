using Dalamud.Configuration;
using System;

namespace ModAutoTagger;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public string GlobalTag { get; set; } = "all";

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
