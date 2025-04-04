using Echoglossian.Properties;
using ImGuiNET;
using System.Numerics;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial void DrawAboutTab()
    {
        if (ImGui.BeginTable("AboutTable", 2, ImGuiTableFlags.None))
        {
            ImGui.TableSetupColumn("TextColumn", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("LogoColumn", ImGuiTableColumnFlags.WidthFixed, 300f);
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.TextColored(new Vector4(0.97f, 0.97f, 0.27f, 1.0f), Resources.DisclaimerTitle);
            ImGui.TextWrapped(Resources.DisclaimerText1);
            ImGui.TextWrapped(Resources.DisclaimerText2);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextWrapped(Resources.ContribText);

            ImGui.TableSetColumnIndex(1);
            float availableHeight = ImGui.GetContentRegionAvail().Y;
            float logoHeight = 300f;
            float cursorY = ImGui.GetCursorPosY();
            float logoY = Math.Max(cursorY, cursorY + (availableHeight - logoHeight) * 0.5f);
            ImGui.SetCursorPosY(logoY);

            if (this.logo?.ImGuiHandle != System.IntPtr.Zero)
            {
                float logoWidth = 300f;
                float columnWidth = ImGui.GetColumnWidth();
                float logoX = ImGui.GetCursorPosX() + (columnWidth - logoWidth) * 0.5f;
                ImGui.SetCursorPosX(logoX);
                ImGui.Image(this.logo.ImGuiHandle, new Vector2(logoWidth, logoHeight));
            }
            else { ImGui.Text("Logo not loaded"); }
            ImGui.EndTable();
        }
    }
}
