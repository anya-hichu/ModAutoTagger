using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ModAutoTagger.Windows;
using ModAutoTagger.Ipcs;

namespace ModAutoTagger;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog  { get; private set; } = null!;

    private const string CommandName = "/modautotagger";
    private const string CommandHelpMessage = "Available subcommands for /modautotagger are main and config";

    public Config Config { get; init; }

    public readonly WindowSystem WindowSystem = new("ModAutoTagger");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private QuackEmotesIpc QuackEmotesIpc { get; init; }
    private QuackPenumbraIpc QuackPenumbraIpc { get; init; }

    private Resolver Resolver { get; init; }
    private Tagger Tagger { get; init; }

    public Plugin()
    {
        Config = PluginInterface.GetPluginConfig() as Config ?? new Config();

        QuackEmotesIpc = new(PluginInterface);
        QuackPenumbraIpc = new(PluginInterface);

        Resolver = new(Config, QuackEmotesIpc, QuackPenumbraIpc, PluginLog);
        Tagger = new(PluginInterface, PluginLog);

        ConfigWindow = new ConfigWindow(Config);
        MainWindow = new MainWindow(Resolver, Tagger);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = CommandHelpMessage
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        var subcommand = args.Split(" ", 2)[0];
        if (subcommand == "main")
        {
            ToggleMainUI();
        }
        else if (subcommand == "config")
        {
            ToggleConfigUI();
        }
        else
        {
            ChatGui.Print(CommandHelpMessage);
        }
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
