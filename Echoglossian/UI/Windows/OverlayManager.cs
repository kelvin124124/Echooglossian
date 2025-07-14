using Dalamud.Interface.Utility;
using Echoglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Echoglossian.UI.Windows
{
    internal class OverlayManager : IDisposable
    {
        private bool talkOverlayVisible = false;
        private string originalTalkName = string.Empty;
        private string originalTalkText = string.Empty;
        private string translatedTalkName = string.Empty;
        private string translatedTalkText = string.Empty;
        private Vector2 talkPosition = Vector2.Zero;
        private Vector2 talkDimensions = Vector2.Zero;
        private string talkWindowTitle = "Talk translation";
        private ImGuiWindowFlags talkWindowFlags = ImGuiWindowFlags.NoTitleBar;

        private bool battleTalkOverlayVisible = false;
        private string originalBattleTalkName = string.Empty;
        private string originalBattleTalkText = string.Empty;
        private string translatedBattleTalkName = string.Empty;
        private string translatedBattleTalkText = string.Empty;
        private Vector2 battleTalkPosition = Vector2.Zero;
        private Vector2 battleTalkDimensions = Vector2.Zero;
        private string battleTalkWindowTitle = "BattleTalk translation";
        private ImGuiWindowFlags battleTalkWindowFlags = ImGuiWindowFlags.NoTitleBar;

        private bool talkSubtitleOverlayVisible = false;
        private string originalTalkSubtitleText = string.Empty;
        private string translatedTalkSubtitleText = string.Empty;
        private Vector2 talkSubtitlePosition = Vector2.Zero;
        private Vector2 talkSubtitleDimensions = Vector2.Zero;

        private readonly Dictionary<string, ToastOverlay> toastOverlays = [];

        private class ToastOverlay
        {
            public bool Visible { get; set; } = false;
            public string OriginalText { get; set; } = string.Empty;
            public string TranslatedText { get; set; } = string.Empty;
            public Vector2 Position { get; set; } = Vector2.Zero;
            public Vector2 Dimensions { get; set; } = Vector2.Zero;
            public ImGuiWindowFlags WindowFlags { get; set; } = ImGuiWindowFlags.NoTitleBar;
            public string WindowTitle { get; set; } = string.Empty;
        }

        private readonly ImGuiWindowFlags baseWindowFlags = ImGuiWindowFlags.NoNav |
                                                          ImGuiWindowFlags.NoCollapse |
                                                          ImGuiWindowFlags.NoFocusOnAppearing |
                                                          ImGuiWindowFlags.NoMouseInputs |
                                                          ImGuiWindowFlags.NoScrollbar;

        private static readonly string[] ToastTypeStrings = ["Normal", "Error", "Quest", "ClassChange", "Area", "WideText"];

        public OverlayManager()
        {
            foreach (var type in ToastTypeStrings)
            {
                var overlay = new ToastOverlay
                {
                    WindowTitle = $"{type} Toast Translation",
                    WindowFlags = (type == "Error" || type == "ClassChange")
                        ? baseWindowFlags | ImGuiWindowFlags.NoBackground
                        : baseWindowFlags
                };

                toastOverlays[type] = overlay;
            }
        }

        public void Dispose() { }

        public void Draw()
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
                foreach (var overlay in toastOverlays.Values)
                {
                    if (overlay.Visible)
                        DrawToastOverlay(overlay);
                }
            }
        }

        private void HideAllOverlays()
        {
            talkOverlayVisible = false;
            battleTalkOverlayVisible = false;
            talkSubtitleOverlayVisible = false;

            foreach (var overlay in toastOverlays.Values)
            {
                overlay.Visible = false;
            }
        }

        #region Talk Overlay
        public void UpdateTalkOverlay(string originalName, string originalText, string translatedName, string translatedText)
        {
            if (originalTalkName == originalName && originalTalkText == originalText &&
                translatedTalkName == translatedName && translatedTalkText == translatedText)
                return;

            originalTalkName = originalName;
            originalTalkText = originalText;
            translatedTalkName = translatedName;
            translatedTalkText = translatedText;

            bool showTitle = Service.config.TALK_TranslateNpcNames && !string.IsNullOrEmpty(translatedTalkName);
            talkWindowTitle = showTitle ? translatedTalkName : "Talk translation";
            talkWindowFlags = showTitle ? baseWindowFlags : baseWindowFlags | ImGuiWindowFlags.NoTitleBar;
        }

        public unsafe void UpdateTalkOverlayPosition(AtkUnitBase* talkAddon)
        {
            if (talkAddon == null) return;

            var textNode = talkAddon->GetTextNodeById(3);
            if (textNode == null) return;

            var scale = talkAddon->Scale;
            var newPosition = new Vector2(
                talkAddon->X + textNode->AtkResNode.X,
                talkAddon->Y + textNode->AtkResNode.Y
            );
            var newDimensions = new Vector2(
                textNode->AtkResNode.Width * scale,
                textNode->AtkResNode.Height * scale
            );

            if (talkPosition != newPosition || talkDimensions != newDimensions)
            {
                talkPosition = newPosition;
                talkDimensions = newDimensions;
            }
        }

        public void SetTalkOverlayVisible(bool visible)
        {
            talkOverlayVisible = visible;
        }

        private void DrawTalkOverlay()
        {
            if (string.IsNullOrEmpty(translatedTalkText)) return;

            PushFontHandle();

            var windowPos = CalculateWindowPosition(talkPosition, talkDimensions);
            windowPos += Service.config.ImGuiWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);
            ImGui.SetNextWindowSize(talkDimensions);

            //ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayTalkTextColor);

            ImGui.Begin(talkWindowTitle, talkWindowFlags);
            ImGui.SetWindowFontScale(Service.config.FontScale * Service.config.ImGuiTalkFontMult);
            ImGui.TextWrapped(translatedTalkText);
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle();
        }
        #endregion

        #region Battle Talk Overlay
        public void UpdateBattleTalkOverlay(string originalName, string originalText, string translatedName, string translatedText)
        {
            if (originalBattleTalkName == originalName && originalBattleTalkText == originalText &&
                translatedBattleTalkName == translatedName && translatedBattleTalkText == translatedText)
                return;

            originalBattleTalkName = originalName;
            originalBattleTalkText = originalText;
            translatedBattleTalkName = translatedName;
            translatedBattleTalkText = translatedText;

            bool showTitle = Service.config.BATTLETALK_TranslateNpcNames && !string.IsNullOrEmpty(translatedBattleTalkName);
            battleTalkWindowTitle = showTitle ? translatedBattleTalkName : "BattleTalk translation";
            battleTalkWindowFlags = showTitle ? baseWindowFlags : baseWindowFlags | ImGuiWindowFlags.NoTitleBar;
        }

        public unsafe void UpdateBattleTalkOverlayPosition(AtkUnitBase* battleTalkAddon)
        {
            if (battleTalkAddon == null) return;

            var textNode = battleTalkAddon->GetTextNodeById(6);
            if (textNode == null) return;

            var scale = battleTalkAddon->Scale;
            var newPosition = new Vector2(
                battleTalkAddon->X + textNode->AtkResNode.X,
                battleTalkAddon->Y + textNode->AtkResNode.Y
            );
            var newDimensions = new Vector2(
                textNode->AtkResNode.Width * scale,
                textNode->AtkResNode.Height * scale
            );

            if (battleTalkPosition != newPosition || battleTalkDimensions != newDimensions)
            {
                battleTalkPosition = newPosition;
                battleTalkDimensions = newDimensions;
            }
        }

        public void SetBattleTalkOverlayVisible(bool visible)
        {
            battleTalkOverlayVisible = visible;
        }

        private void DrawBattleTalkOverlay()
        {
            if (string.IsNullOrEmpty(translatedBattleTalkText)) return;

            PushFontHandle();

            var windowPos = CalculateWindowPosition(battleTalkPosition, battleTalkDimensions);
            windowPos += Service.config.ImGuiWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);
            ImGui.SetNextWindowSize(battleTalkDimensions);

            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayBattleTalkTextColor);

            ImGui.Begin(battleTalkWindowTitle, battleTalkWindowFlags);
            ImGui.SetWindowFontScale(Service.config.FontScale * Service.config.ImGuiBattleTalkFontMult);
            ImGui.TextWrapped(translatedBattleTalkText);
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle();
        }
        #endregion

        #region Talk Subtitle Overlay
        public void UpdateTalkSubtitleOverlay(string originalText, string translatedText)
        {
            if (originalTalkSubtitleText == originalText && translatedTalkSubtitleText == translatedText)
                return;

            originalTalkSubtitleText = originalText;
            translatedTalkSubtitleText = translatedText;
        }

        public unsafe void UpdateTalkSubtitleOverlayPosition(AtkUnitBase* talkSubtitleAddon)
        {
            if (talkSubtitleAddon == null) return;

            var textNode = talkSubtitleAddon->GetTextNodeById(2);
            if (textNode == null) return;

            var scale = talkSubtitleAddon->Scale;
            var newPosition = new Vector2(
                talkSubtitleAddon->X + textNode->AtkResNode.X,
                talkSubtitleAddon->Y + textNode->AtkResNode.Y
            );
            var newDimensions = new Vector2(
                textNode->AtkResNode.Width * scale,
                textNode->AtkResNode.Height * scale
            );

            if (talkSubtitlePosition != newPosition || talkSubtitleDimensions != newDimensions)
            {
                talkSubtitlePosition = newPosition;
                talkSubtitleDimensions = newDimensions;
            }
        }

        public void SetTalkSubtitleOverlayVisible(bool visible)
        {
            talkSubtitleOverlayVisible = visible;
        }

        private void DrawTalkSubtitleOverlay()
        {
            if (string.IsNullOrEmpty(translatedTalkSubtitleText)) return;

            PushFontHandle();

            var windowPos = CalculateWindowPosition(talkSubtitlePosition, talkSubtitleDimensions);
            windowPos += Service.config.ImGuiWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);
            ImGui.SetNextWindowSize(talkSubtitleDimensions);

            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayTalkTextColor);

            ImGui.Begin("TalkSubtitle translation", baseWindowFlags | ImGuiWindowFlags.NoTitleBar);
            ImGui.SetWindowFontScale(Service.config.FontScale * Service.config.ImGuiTalkSubtitleFontMult);
            ImGui.TextWrapped(translatedTalkSubtitleText);
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle();
        }
        #endregion

        #region Toast Overlays
        public void UpdateToastOverlay(string toastType, string originalText, string translatedText)
        {
            if (!toastOverlays.TryGetValue(toastType, out var overlay)) return;

            if (overlay.OriginalText == originalText && overlay.TranslatedText == translatedText)
                return;

            overlay.OriginalText = originalText;
            overlay.TranslatedText = translatedText;
        }

        public void UpdateToastOverlayPosition(string toastType, float x, float y, float width, float height)
        {
            if (!toastOverlays.TryGetValue(toastType, out var overlay)) return;

            // no addon position calculation needed
            var newPosition = new Vector2(x, y);
            var newDimensions = new Vector2(width, height);

            if (overlay.Position != newPosition || overlay.Dimensions != newDimensions)
            {
                overlay.Position = newPosition;
                overlay.Dimensions = newDimensions;
            }
        }

        public void SetToastOverlayVisible(string toastType, bool visible)
        {
            if (toastOverlays.TryGetValue(toastType, out var overlay))
            {
                overlay.Visible = visible;
            }
        }

        private static void DrawToastOverlay(ToastOverlay overlay)
        {
            if (string.IsNullOrEmpty(overlay.TranslatedText)) return;

            PushFontHandle();

            var windowPos = CalculateWindowPosition(overlay.Position, overlay.Dimensions);
            windowPos += Service.config.ImGuiToastWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);
            ImGui.SetNextWindowSize(overlay.Dimensions);

            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayTalkTextColor);

            ImGui.Begin(overlay.WindowTitle, overlay.WindowFlags);
            ImGui.SetWindowFontScale(Service.config.FontScale * Service.config.ImGuiToastFontMult);
            ImGui.TextWrapped(overlay.TranslatedText);
            ImGui.End();

            ImGui.PopStyleColor();
            PopFontHandle();
        }
        #endregion

        #region Helper Methods
        private static Vector2 CalculateWindowPosition(Vector2 position, Vector2 dimensions)
        {
            return new Vector2(position.X, position.Y - dimensions.Y - 40);
        }

        private static void PushFontHandle()
        {
            Service.fontManager.LanguageFontHandle.Push();
        }

        private static void PopFontHandle()
        {
            Service.fontManager.LanguageFontHandle.Pop();
        }
        #endregion
    }
}
