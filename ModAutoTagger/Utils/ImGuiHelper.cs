using ImGuiNET;

namespace ModAutoTagger.Utils;

public class ImGuiHelper
{
    public unsafe static ImGuiListClipperPtr NewListClipper()
    {
        return new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
    }
}
