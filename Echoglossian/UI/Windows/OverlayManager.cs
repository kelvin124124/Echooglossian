using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Echoglossian.Utils;

namespace Echoglossian.UI.Windows
{
    internal class OverlayManager : IDisposable
    {
        // Talk Overlay
        private bool talkOverlayVisible = false;
        private string originalTalkName = string.Empty;
        private string originalTalkText = string.Empty;
        private string translatedTalkName = string.Empty;
        private string translatedTalkText = string.Empty;
        private Vector2 talkPosition = Vector2.Zero;
        private Vector2 talkDimensions = Vector2.Zero;
        private Vector2 talkImguiSize = Vector2.Zero;
        private bool talkDirty = true;
        private float talkCachedWidth = 0f;
        private string talkWindowTitle = "Talk translation";
        private ImGuiWindowFlags talkWindowFlags = ImGuiWindowFlags.NoTitleBar;

        // Battle Talk Overlay
        private bool battleTalkOverlayVisible = false;
        private string originalBattleTalkName = string.Empty;
        private string originalBattleTalkText = string.Empty;
        private string translatedBattleTalkName = string.Empty;
        private string translatedBattleTalkText = string.Empty;
        private Vector2 battleTalkPosition = Vector2.Zero;
        private Vector2 battleTalkDimensions = Vector2.Zero;
        private Vector2 battleTalkImguiSize = Vector2.Zero;
        private bool battleTalkDirty = true;
        private float battleTalkCachedWidth = 0f;
        private string battleTalkWindowTitle = "BattleTalk translation";
        private ImGuiWindowFlags battleTalkWindowFlags = ImGuiWindowFlags.NoTitleBar;

        // Talk Subtitle Overlay
        private bool talkSubtitleOverlayVisible = false;
        private string originalTalkSubtitleText = string.Empty;
        private string translatedTalkSubtitleText = string.Empty;
        private Vector2 talkSubtitlePosition = Vector2.Zero;
        private Vector2 talkSubtitleDimensions = Vector2.Zero;
        private Vector2 talkSubtitleImguiSize = Vector2.Zero;
        private bool talkSubtitleDirty = true;
        private float talkSubtitleCachedWidth = 0f;

        // Toast Overlays
        private readonly Dictionary<string, ToastOverlay> toastOverlays = [];

        private class ToastOverlay
        {
            public bool Visible { get; set; } = false;
            public string OriginalText { get; set; } = string.Empty;
            public string TranslatedText { get; set; } = string.Empty;
            public Vector2 Position { get; set; } = Vector2.Zero;
            public Vector2 Dimensions { get; set; } = Vector2.Zero;
            public Vector2 ImguiSize { get; set; } = Vector2.Zero;
            public bool Dirty { get; set; } = true;
            public float CachedWidth { get; set; } = 0f;
            public ImGuiWindowFlags WindowFlags { get; set; } = ImGuiWindowFlags.NoTitleBar;
        }

        private readonly ImGuiWindowFlags baseWindowFlags = ImGuiWindowFlags.NoNav |
                                                          ImGuiWindowFlags.NoCollapse |
                                                          ImGuiWindowFlags.AlwaysAutoResize |
                                                          ImGuiWindowFlags.NoFocusOnAppearing |
                                                          ImGuiWindowFlags.NoMouseInputs |
                                                          ImGuiWindowFlags.NoScrollbar;

        public OverlayManager()
        {
            string[] toastTypes = ["Normal", "Error", "Quest", "ClassChange", "Area", "WideText"];
            foreach (var type in toastTypes)
            {
                var overlay = new ToastOverlay();
                if (type == "Error" || type == "ClassChange")
                    overlay.WindowFlags = baseWindowFlags | ImGuiWindowFlags.NoBackground;
                else
                    overlay.WindowFlags = baseWindowFlags;

                toastOverlays[type] = overlay;
            }

            Service.framework.Update += OnFrameworkUpdate;
        }

        public void Dispose()
        {
            Service.framework.Update -= OnFrameworkUpdate;
        }

        private void OnFrameworkUpdate(Dalamud.Plugin.Services.IFramework framework)
        {
            if (!Service.clientState.IsLoggedIn)
            {
                HideAllOverlays();
                return;
            }

            if (talkOverlayVisible && Service.config.TalkModuleEnabled && Service.config.TALK_UseImGui)
                DrawTalkOverlay();

            if (battleTalkOverlayVisible && Service.config.BattleTalkModuleEnabled && Service.config.BATTLETALK_UseImGui)
                DrawBattleTalkOverlay();

            if (talkSubtitleOverlayVisible && Service.config.TalkSubtitleModuleEnabled && Service.config.SUBTITLE_UseImGui)
                DrawTalkSubtitleOverlay();

            if (Service.config.ToastModuleEnabled && Service.config.TOAST_UseImGui)
            {
                foreach (var entry in toastOverlays)
                {
                    if (entry.Value.Visible)
                        DrawToastOverlay(entry.Key, entry.Value);
                }
            }
        }

        private void HideAllOverlays()
        {
            SetTalkOverlayVisible(false);
            SetBattleTalkOverlayVisible(false);
            SetTalkSubtitleOverlayVisible(false);
            foreach (var key in toastOverlays.Keys)
                SetToastOverlayVisible(key, false);
        }

        #region Talk Overlay
        public void UpdateTalkOverlay(string originalName, string originalText, string translatedName, string translatedText)
        {
            if (originalTalkName != originalName || originalTalkText != originalText ||
                translatedTalkName != translatedName || translatedTalkText != translatedText)
            {
                originalTalkName = originalName;
                originalTalkText = originalText;
                translatedTalkName = translatedName;
                translatedTalkText = translatedText;
                talkDirty = true;

                bool showTitle = Service.config.TALK_TranslateNpcNames && !string.IsNullOrEmpty(translatedTalkName);
                talkWindowTitle = showTitle ? translatedTalkName : "Talk translation";
                talkWindowFlags = showTitle ? baseWindowFlags : baseWindowFlags | ImGuiWindowFlags.NoTitleBar;
            }
        }

        public unsafe void UpdateTalkOverlayPosition(AtkUnitBase* talkAddon)
        {
            if (talkAddon == null) return;

            var textNode = talkAddon->GetTextNodeById(3);
            if (textNode == null) return;

            var newPosition = new Vector2(textNode->AtkResNode.X, textNode->AtkResNode.Y);
            var newDimensions = new Vector2(textNode->AtkResNode.Width, textNode->AtkResNode.Height);

            if (talkPosition != newPosition || talkDimensions != newDimensions)
            {
                talkPosition = newPosition;
                talkDimensions = newDimensions;
                talkDirty = true;
            }
        }

        public void SetTalkOverlayVisible(bool visible)
        {
            if (talkOverlayVisible != visible)
            {
                talkOverlayVisible = visible;
                talkDirty = true;
            }
        }

        private void DrawTalkOverlay()
        {
            if (string.IsNullOrEmpty(translatedTalkText)) return;

            if (talkDirty)
            {
                talkCachedWidth = CalculateWindowWidth(
                    talkDimensions.X,
                    Service.config.ImGuiTalkWindowWidthMult,
                    translatedTalkText);
                talkDirty = false;
            }

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(
                CalculateWindowPosition(talkPosition, talkDimensions, talkImguiSize)
                + Service.config.ImGuiWindowPosCorrection);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(talkCachedWidth, 0),
                new Vector2(talkCachedWidth, talkDimensions.Y * Service.config.ImGuiTalkWindowHeightMult));

            PushFontHandle(Service.config.TALK_EnableImGuiTextSwap);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(Service.config.OverlayTalkTextColor, 255));

            ImGui.Begin(talkWindowTitle, talkWindowFlags);
            ImGui.SetWindowFontScale(Service.config.FontScale);
            ImGui.TextWrapped(translatedTalkText);
            talkImguiSize = ImGui.GetWindowSize();
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle(Service.config.TALK_EnableImGuiTextSwap);
        }
        #endregion

        #region Battle Talk Overlay
        public void UpdateBattleTalkOverlay(string originalName, string originalText, string translatedName, string translatedText)
        {
            if (originalBattleTalkName != originalName || originalBattleTalkText != originalText ||
                translatedBattleTalkName != translatedName || translatedBattleTalkText != translatedText)
            {
                originalBattleTalkName = originalName;
                originalBattleTalkText = originalText;
                translatedBattleTalkName = translatedName;
                translatedBattleTalkText = translatedText;
                battleTalkDirty = true;

                bool showTitle = Service.config.BATTLETALK_TranslateNpcNames && !string.IsNullOrEmpty(translatedBattleTalkName);
                battleTalkWindowTitle = showTitle ? translatedBattleTalkName : "BattleTalk translation";
                battleTalkWindowFlags = showTitle ? baseWindowFlags : baseWindowFlags | ImGuiWindowFlags.NoTitleBar;
            }
        }

        public unsafe void UpdateBattleTalkOverlayPosition(AtkUnitBase* battleTalkAddon)
        {
            if (battleTalkAddon == null) return;

            var textNode = battleTalkAddon->GetTextNodeById(6);
            if (textNode == null) return;

            var newPosition = new Vector2(textNode->AtkResNode.X, textNode->AtkResNode.Y);
            var newDimensions = new Vector2(textNode->AtkResNode.Width, textNode->AtkResNode.Height);

            if (battleTalkPosition != newPosition || battleTalkDimensions != newDimensions)
            {
                battleTalkPosition = newPosition;
                battleTalkDimensions = newDimensions;
                battleTalkDirty = true;
            }
        }

        public void SetBattleTalkOverlayVisible(bool visible)
        {
            if (battleTalkOverlayVisible != visible)
            {
                battleTalkOverlayVisible = visible;
                battleTalkDirty = true;
            }
        }

        private void DrawBattleTalkOverlay()
        {
            if (string.IsNullOrEmpty(translatedBattleTalkText)) return;

            if (battleTalkDirty)
            {
                battleTalkCachedWidth = Math.Min(
                    battleTalkDimensions.X * Service.config.ImGuiBattleTalkWindowWidthMult * 1.5f,
                    ImGui.CalcTextSize(translatedBattleTalkText).X + (ImGui.GetStyle().WindowPadding.X * 3));
                battleTalkDirty = false;
            }

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(
                CalculateWindowPosition(battleTalkPosition, battleTalkDimensions, battleTalkImguiSize)
                + Service.config.ImGuiWindowPosCorrection);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(battleTalkCachedWidth, 0),
                new Vector2(battleTalkCachedWidth, battleTalkDimensions.Y * 2.5f * Service.config.ImGuiBattleTalkWindowHeightMult));

            PushFontHandle(Service.config.BATTLETALK_EnableImGuiTextSwap);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(Service.config.OverlayBattleTalkTextColor, 255));

            ImGui.Begin(battleTalkWindowTitle, battleTalkWindowFlags);
            ImGui.SetWindowFontScale(Service.config.FontScale);
            ImGui.TextWrapped(translatedBattleTalkText);
            battleTalkImguiSize = ImGui.GetWindowSize();
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle(Service.config.BATTLETALK_EnableImGuiTextSwap);
        }
        #endregion

        #region Talk Subtitle Overlay
        public void UpdateTalkSubtitleOverlay(string originalText, string translatedText)
        {
            if (originalTalkSubtitleText != originalText || translatedTalkSubtitleText != translatedText)
            {
                originalTalkSubtitleText = originalText;
                translatedTalkSubtitleText = translatedText;
                talkSubtitleDirty = true;
            }
        }

        public unsafe void UpdateTalkSubtitleOverlayPosition(AtkUnitBase* talkSubtitleAddon)
        {
            if (talkSubtitleAddon == null) return;

            var textNode = talkSubtitleAddon->GetTextNodeById(2);
            if (textNode == null) return;

            var newPosition = new Vector2(textNode->AtkResNode.X, textNode->AtkResNode.Y);
            var newDimensions = new Vector2(textNode->AtkResNode.Width, textNode->AtkResNode.Height);

            if (talkSubtitlePosition != newPosition || talkSubtitleDimensions != newDimensions)
            {
                talkSubtitlePosition = newPosition;
                talkSubtitleDimensions = newDimensions;
                talkSubtitleDirty = true;
            }
        }

        public void SetTalkSubtitleOverlayVisible(bool visible)
        {
            if (talkSubtitleOverlayVisible != visible)
            {
                talkSubtitleOverlayVisible = visible;
                talkSubtitleDirty = true;
            }
        }

        private void DrawTalkSubtitleOverlay()
        {
            if (string.IsNullOrEmpty(translatedTalkSubtitleText)) return;

            if (talkSubtitleDirty)
            {
                talkSubtitleCachedWidth = CalculateWindowWidth(
                    talkSubtitleDimensions.X,
                    Service.config.ImGuiTalkSubtitleWindowWidthMult,
                    translatedTalkSubtitleText);
                talkSubtitleDirty = false;
            }

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(
                CalculateWindowPosition(talkSubtitlePosition, talkSubtitleDimensions, talkSubtitleImguiSize)
                + Service.config.ImGuiWindowPosCorrection);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(talkSubtitleCachedWidth, 0),
                new Vector2(talkSubtitleCachedWidth, talkSubtitleDimensions.Y * Service.config.ImGuiTalkSubtitleWindowHeightMult));

            PushFontHandle(Service.config.SUBTITLE_EnableImGuiTextSwap);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(Service.config.OverlayTalkTextColor, 255));

            ImGui.Begin("TalkSubtitle translation", baseWindowFlags | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowFontScale(Service.config.FontScale);
            ImGui.TextWrapped(translatedTalkSubtitleText);
            talkSubtitleImguiSize = ImGui.GetWindowSize();
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle(Service.config.SUBTITLE_EnableImGuiTextSwap);
        }
        #endregion

        #region Toast Overlays
        public void UpdateToastOverlay(string toastType, string originalText, string translatedText)
        {
            if (!toastOverlays.TryGetValue(toastType, out var overlay)) return;

            if (overlay.OriginalText != originalText || overlay.TranslatedText != translatedText)
            {
                overlay.OriginalText = originalText;
                overlay.TranslatedText = translatedText;
                overlay.Dirty = true;
            }
        }

        public void UpdateToastOverlayPosition(string toastType, float x, float y, float width, float height)
        {
            if (!toastOverlays.TryGetValue(toastType, out var overlay)) return;

            var newPosition = new Vector2(x, y);
            var newDimensions = new Vector2(width, height);

            if (overlay.Position != newPosition || overlay.Dimensions != newDimensions)
            {
                overlay.Position = newPosition;
                overlay.Dimensions = newDimensions;
                overlay.Dirty = true;
            }
        }

        public void SetToastOverlayVisible(string toastType, bool visible)
        {
            if (!toastOverlays.TryGetValue(toastType, out var overlay)) return;

            if (overlay.Visible != visible)
            {
                overlay.Visible = visible;
                overlay.Dirty = true;
            }
        }

        private void DrawToastOverlay(string toastType, ToastOverlay overlay)
        {
            if (string.IsNullOrEmpty(overlay.TranslatedText)) return;

            if (overlay.Dirty)
            {
                overlay.CachedWidth = Math.Min(
                    overlay.Dimensions.X * Service.config.ImGuiToastWindowWidthMult,
                    ImGui.CalcTextSize(overlay.TranslatedText).X + (ImGui.GetStyle().WindowPadding.X * 2));
                overlay.Dirty = false;
            }

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(
                CalculateWindowPosition(overlay.Position, overlay.Dimensions, overlay.ImguiSize)
                + Service.config.ImGuiToastWindowPosCorrection);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(overlay.CachedWidth, 0),
                new Vector2(overlay.CachedWidth * 4f, overlay.Dimensions.Y * 2));

            PushFontHandle(Service.config.SwapTextsUsingImGui);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(Service.config.OverlayTalkTextColor, 255));

            ImGui.Begin($"{toastType} Toast Translation", overlay.WindowFlags);
            ImGui.SetWindowFontScale(Service.config.FontScale);
            ImGui.Text(overlay.TranslatedText);
            overlay.ImguiSize = ImGui.GetWindowSize();
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle(Service.config.SwapTextsUsingImGui);
        }
        #endregion

        #region Helper Methods
        private static float CalculateWindowWidth(float baseDimension, float multiplier, string text)
        {
            return Math.Min(
                (baseDimension * multiplier) + (ImGui.GetStyle().WindowPadding.X * 2),
                (ImGui.CalcTextSize(text).X * 1.25f) + (ImGui.GetStyle().WindowPadding.X * 2));
        }

        private static Vector2 CalculateWindowPosition(Vector2 position, Vector2 dimensions, Vector2 imguiSize)
        {
            return new Vector2(
                position.X + (dimensions.X / 2) - (imguiSize.X / 2),
                position.Y - imguiSize.Y - 20);
        }

        private void PushFontHandle(bool useGeneralFont)
        {
            if (useGeneralFont)
                Service.fontHandler.GeneralFontHandle.Push();
            else
                Service.fontHandler.LanguageFontHandle.Push();
        }

        private void PopFontHandle(bool useGeneralFont)
        {
            if (useGeneralFont)
                Service.fontHandler.GeneralFontHandle.Pop();
            else
                Service.fontHandler.LanguageFontHandle.Pop();
        }
        #endregion
    }
}
