using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace ModAutoTagger.Ipcs;

public class QuackPenumbraIpc(IDalamudPluginInterface pluginInterface)
{
    public static readonly string LIST = "Quack.Penumbra.GetModListWithSettings";

    private ICallGateSubscriber<ModData[]> GetListSubscriber { get; init; } = pluginInterface.GetIpcSubscriber<ModData[]>(LIST);

    public ModData[] GetList()
    {
        return GetListSubscriber.InvokeFunc();
    }
}


