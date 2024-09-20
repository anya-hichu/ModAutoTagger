
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace ModAutoTagger.Ipcs;

public class QuackEmotesIpc(IDalamudPluginInterface pluginInterface)
{
    public static readonly string LIST = "Quack.Emotes.GetList";

    private ICallGateSubscriber<EmoteData[]> GetListSubscriber { get; init; } = pluginInterface.GetIpcSubscriber<EmoteData[]>(LIST);

    public EmoteData[] GetList()
    {
        return GetListSubscriber.InvokeFunc();
    }
}


