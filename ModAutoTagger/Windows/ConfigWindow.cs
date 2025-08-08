using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ModAutoTagger.Windows;

public class ConfigWindow : Window
{
    private Config Config { get; init; }

    public ConfigWindow(Config config) : base("ModAutoTagger##configWindows")
    {
        SizeConstraints = new()
        {
            MinimumSize = new(300, 100),
            MaximumSize = new(float.MaxValue, float.MaxValue)
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
