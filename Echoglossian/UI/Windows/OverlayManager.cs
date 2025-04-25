using Dalamud.Interface.Utility;
using Echoglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

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
        private bool talkLastFontState = false;

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
        private bool battleTalkLastFontState = false;

        // Talk Subtitle Overlay
        private bool talkSubtitleOverlayVisible = false;
        private string originalTalkSubtitleText = string.Empty;
        private string translatedTalkSubtitleText = string.Empty;
        private Vector2 talkSubtitlePosition = Vector2.Zero;
        private Vector2 talkSubtitleDimensions = Vector2.Zero;
        private Vector2 talkSubtitleImguiSize = Vector2.Zero;
        private bool talkSubtitleDirty = true;
        private float talkSubtitleCachedWidth = 0f;
        private bool subtitleLastFontState = false;

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
            public string WindowTitle { get; set; } = string.Empty;
            public bool LastFontState { get; set; } = false;
        }

        private readonly ImGuiWindowFlags baseWindowFlags = ImGuiWindowFlags.NoNav |
                                                          ImGuiWindowFlags.NoCollapse |
                                                          ImGuiWindowFlags.AlwaysAutoResize |
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
                    WindowTitle = $"{type} Toast Translation"
                };

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

            foreach (var type in ToastTypeStrings)
            {
                if (toastOverlays.TryGetValue(type, out var overlay) && overlay.Visible)
                {
                    overlay.Visible = false;
                }
            }
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
                if (showTitle)
                {
                    talkWindowTitle = translatedTalkName;
                    talkWindowFlags = baseWindowFlags;
                }
                else
                {
                    talkWindowTitle = "Talk translation";
                    talkWindowFlags = baseWindowFlags | ImGuiWindowFlags.NoTitleBar;
                }
            }
        }

        public unsafe void UpdateTalkOverlayPosition(AtkUnitBase* talkAddon)
        {
            if (talkAddon == null) return;

            var textNode = talkAddon->GetTextNodeById(3);
            if (textNode == null) return;

            float x = textNode->AtkResNode.X;
            float y = textNode->AtkResNode.Y;
            float width = textNode->AtkResNode.Width;
            float height = textNode->AtkResNode.Height;

            if (talkPosition.X != x || talkPosition.Y != y ||
                talkDimensions.X != width || talkDimensions.Y != height)
            {
                talkPosition = new Vector2(x, y);
                talkDimensions = new Vector2(width, height);
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

            bool fontStateChanged = talkLastFontState != Service.config.TALK_EnableImGuiTextSwap;

            if (talkDirty || fontStateChanged)
            {
                talkCachedWidth = CalculateWindowWidth(
                    talkDimensions.X,
                    Service.config.ImGuiTalkWindowWidthMult,
                    translatedTalkText);
                talkDirty = false;
                talkLastFontState = Service.config.TALK_EnableImGuiTextSwap;
            }

            var windowPos = CalculateWindowPosition(talkPosition, talkDimensions, talkImguiSize);
            windowPos += Service.config.ImGuiWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(talkCachedWidth, 0),
                new Vector2(talkCachedWidth, talkDimensions.Y * Service.config.ImGuiTalkWindowHeightMult));

            PushFontHandle(Service.config.TALK_EnableImGuiTextSwap);
            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayTalkTextColor);

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
                if (showTitle)
                {
                    battleTalkWindowTitle = translatedBattleTalkName;
                    battleTalkWindowFlags = baseWindowFlags;
                }
                else
                {
                    battleTalkWindowTitle = "BattleTalk translation";
                    battleTalkWindowFlags = baseWindowFlags | ImGuiWindowFlags.NoTitleBar;
                }
            }
        }

        public unsafe void UpdateBattleTalkOverlayPosition(AtkUnitBase* battleTalkAddon)
        {
            if (battleTalkAddon == null) return;

            var textNode = battleTalkAddon->GetTextNodeById(6);
            if (textNode == null) return;

            float x = textNode->AtkResNode.X;
            float y = textNode->AtkResNode.Y;
            float width = textNode->AtkResNode.Width;
            float height = textNode->AtkResNode.Height;

            if (battleTalkPosition.X != x || battleTalkPosition.Y != y ||
                battleTalkDimensions.X != width || battleTalkDimensions.Y != height)
            {
                battleTalkPosition = new Vector2(x, y);
                battleTalkDimensions = new Vector2(width, height);
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

            bool fontStateChanged = battleTalkLastFontState != Service.config.BATTLETALK_EnableImGuiTextSwap;

            if (battleTalkDirty || fontStateChanged)
            {
                battleTalkCachedWidth = Math.Min(
                    battleTalkDimensions.X * Service.config.ImGuiBattleTalkWindowWidthMult * 1.5f,
                    ImGui.CalcTextSize(translatedBattleTalkText).X + (ImGui.GetStyle().WindowPadding.X * 3));
                battleTalkDirty = false;
                battleTalkLastFontState = Service.config.BATTLETALK_EnableImGuiTextSwap;
            }

            var windowPos = CalculateWindowPosition(battleTalkPosition, battleTalkDimensions, battleTalkImguiSize);
            windowPos += Service.config.ImGuiWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(battleTalkCachedWidth, 0),
                new Vector2(battleTalkCachedWidth, battleTalkDimensions.Y * 2.5f * Service.config.ImGuiBattleTalkWindowHeightMult));

            PushFontHandle(Service.config.BATTLETALK_EnableImGuiTextSwap);
            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayBattleTalkTextColor);

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

            float x = textNode->AtkResNode.X;
            float y = textNode->AtkResNode.Y;
            float width = textNode->AtkResNode.Width;
            float height = textNode->AtkResNode.Height;

            if (talkSubtitlePosition.X != x || talkSubtitlePosition.Y != y ||
                talkSubtitleDimensions.X != width || talkSubtitleDimensions.Y != height)
            {
                talkSubtitlePosition = new Vector2(x, y);
                talkSubtitleDimensions = new Vector2(width, height);
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

            bool fontStateChanged = subtitleLastFontState != Service.config.SUBTITLE_EnableImGuiTextSwap;

            if (talkSubtitleDirty || fontStateChanged)
            {
                talkSubtitleCachedWidth = CalculateWindowWidth(
                    talkSubtitleDimensions.X,
                    Service.config.ImGuiTalkSubtitleWindowWidthMult,
                    translatedTalkSubtitleText);
                talkSubtitleDirty = false;
                subtitleLastFontState = Service.config.SUBTITLE_EnableImGuiTextSwap;
            }

            var windowPos = CalculateWindowPosition(talkSubtitlePosition, talkSubtitleDimensions, talkSubtitleImguiSize);
            windowPos += Service.config.ImGuiWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(talkSubtitleCachedWidth, 0),
                new Vector2(talkSubtitleCachedWidth, talkSubtitleDimensions.Y * Service.config.ImGuiTalkSubtitleWindowHeightMult));

            PushFontHandle(Service.config.SUBTITLE_EnableImGuiTextSwap);
            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayTalkTextColor);

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

            if (overlay.Position.X != x || overlay.Position.Y != y ||
                overlay.Dimensions.X != width || overlay.Dimensions.Y != height)
            {
                overlay.Position = new Vector2(x, y);
                overlay.Dimensions = new Vector2(width, height);
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

        private static void DrawToastOverlay(string toastType, ToastOverlay overlay)
        {
            if (string.IsNullOrEmpty(overlay.TranslatedText)) return;

            bool fontStateChanged = overlay.LastFontState != Service.config.SwapTextsUsingImGui;

            if (overlay.Dirty || fontStateChanged)
            {
                overlay.CachedWidth = Math.Min(
                    overlay.Dimensions.X * Service.config.ImGuiToastWindowWidthMult,
                    ImGui.CalcTextSize(overlay.TranslatedText).X + (ImGui.GetStyle().WindowPadding.X * 2));
                overlay.Dirty = false;
                overlay.LastFontState = Service.config.SwapTextsUsingImGui;
            }

            var windowPos = CalculateWindowPosition(overlay.Position, overlay.Dimensions, overlay.ImguiSize);
            windowPos += Service.config.ImGuiToastWindowPosCorrection;

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(windowPos);

            ImGui.SetNextWindowSizeConstraints(
                new Vector2(overlay.CachedWidth, 0),
                new Vector2(overlay.CachedWidth * 4f, overlay.Dimensions.Y * 2));

            PushFontHandle(Service.config.SwapTextsUsingImGui);
            ImGui.PushStyleColor(ImGuiCol.Text, Service.config.OverlayTalkTextColor);

            ImGui.Begin(overlay.WindowTitle, overlay.WindowFlags);
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
            var textSize = ImGui.CalcTextSize(text);
            var windowPadding = ImGui.GetStyle().WindowPadding.X * 2;

            return Math.Min(
                (baseDimension * multiplier) + windowPadding,
                (textSize.X * 1.25f) + windowPadding);
        }

        private static Vector2 CalculateWindowPosition(Vector2 position, Vector2 dimensions, Vector2 imguiSize)
        {
            return new Vector2(
                position.X + (dimensions.X / 2) - (imguiSize.X / 2),
                position.Y - imguiSize.Y - 20);
        }

        private static void PushFontHandle(bool useGeneralFont)
        {
            if (useGeneralFont)
                Service.fontManager.GeneralFontHandle.Push();
            else
                Service.fontManager.LanguageFontHandle.Push();
        }

        private static void PopFontHandle(bool useGeneralFont)
        {
            if (useGeneralFont)
                Service.fontManager.GeneralFontHandle.Pop();
            else
                Service.fontManager.LanguageFontHandle.Pop();
        }
        #endregion
    }
}
