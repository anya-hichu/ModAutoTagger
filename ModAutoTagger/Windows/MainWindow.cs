using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ModAutoTagger.Utils;

namespace ModAutoTagger.Windows;

public class MainWindow : Window
{
    private string Filter { get; set; } = string.Empty;

    private Resolver Resolver { get; init; }
    private Tagger Tagger { get; init; }

    public MainWindow(Resolver resolver, Tagger tagger) : base("ModAutoTagger##mainWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Resolver = resolver;
        Tagger = tagger;
    }

    public override void Draw()
    {
        if (ImGui.Button("Refresh##refresh"))
        {
            Task.Run(Resolver.RefreshTagsByMod);
        }

        var tagsByMod = Resolver.TagsByMod.ToDictionary();
        if (tagsByMod.Count > 0)
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudRed);
            if (ImGui.Button("Override All##overrideAll"))
            {
                Task.Run(() => Tagger.OverrideAll(tagsByMod));
            }
            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.HealerGreen);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
            if (ImGui.Button("Append All##appendAll"))
            {
                Task.Run(() => Tagger.AppendAll(tagsByMod));
            }
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            var filter = Filter;
            if (ImGui.InputText("Filter##filter", ref filter, ushort.MaxValue))
            {
                Filter = filter;
            }

            if (ImGui.BeginTable("tagsByMod", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable, new(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 20)))
            {
                ImGui.TableSetupColumn($"Name##name", ImGuiTableColumnFlags.None, 0.4f);
                ImGui.TableSetupColumn($"Tags##currentTags", ImGuiTableColumnFlags.None, 0.2f);
                ImGui.TableSetupColumn($"Resolved Tags##newTags", ImGuiTableColumnFlags.None, 0.2f);
                ImGui.TableSetupColumn($"Actions##actions", ImGuiTableColumnFlags.None, 0.3f);
                ImGui.TableHeadersRow();

                var clipper = ImGuiHelper.NewListClipper();

                var filteredTagsByMod = tagsByMod.Where(e => e.Key.name.Contains(Filter)).OrderBy(e => e.Key.name).ToList();
                clipper.Begin(filteredTagsByMod.Count, 21);
                while (clipper.Step())
                {
                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var entry = filteredTagsByMod.ElementAt(i);
                        var mod = entry.Key;
                        var resolvedTags = entry.Value;

                        if (ImGui.TableNextColumn())
                        {
                            ImGui.Text(mod.name);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(mod.path);
                            }
                        }

                        if (ImGui.TableNextColumn())
                        {
                            var joinedLocalTags = string.Join(",", mod.localTags);
                            ImGui.Text(joinedLocalTags);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(joinedLocalTags);
                            }
                        }

                        if (ImGui.TableNextColumn())
                        {
                            var joinedResolvedTags = string.Join(",", resolvedTags);
                            ImGui.Text(joinedResolvedTags);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(joinedResolvedTags);
                            }
                        }

                        if (ImGui.TableNextColumn())
                        {
                            if (Tagger.CanOverride(mod, resolvedTags))
                            {
                                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudRed);
                                if (ImGui.Button("Override##override", new(60, ImGui.GetTextLineHeight())))
                                {
                                    Tagger.Override(mod, resolvedTags);
                                }
                                ImGui.PopStyleColor();
                            }

                            ImGui.SameLine();
                            if (Tagger.CanAppend(mod, resolvedTags))
                            {
                                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.HealerGreen);
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
                                if (ImGui.Button("Append##append", new(60, ImGui.GetTextLineHeight())))
                                {
                                    Tagger.Append(mod, resolvedTags);
                                }
                                ImGui.PopStyleColor();
                                ImGui.PopStyleColor();
                            }
                        }
                    } 
                }
                clipper.Destroy();

                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.Text("Make sure Quack plugin is installed for IPC data if refresh is not working");
            ImGui.PopStyleColor();
        }
    }
}
