using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ModAutoTagger.Windows;

public class ConfigWindow : Window
{
    private Config Config { get; init; }

    public ConfigWindow(Config config) : base("ModAutoTagger##configWindows")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Config = config;
    }

    public override void Draw()
    {
        var globalTag = Config.GlobalTag;
        if (ImGui.InputText("Global Tag##globalTag", ref globalTag, ushort.MaxValue))
        {
            Config.GlobalTag = globalTag;
            Config.Save();
        }
    }
}
