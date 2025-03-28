﻿// <copyright file="SimpleWindow.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Numerics;
using System.Threading;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Echoglossian.Properties;
using ImGuiNET;

namespace Echoglossian
{
  internal class SimpleWindow : Window, IDisposable
  {
    // TODO: add window position calculations based on the current addon
    // TODO: add window sizing calculations based on the current translation
    // This is currently not in use yet!
    private bool disposedValue;
    private bool displayTranslation;
    private readonly SemaphoreSlim? translationSemaphore;
    private string? translation = string.Empty;
    private volatile int currentTranslationId;

    private Vector2 textDimensions = Vector2.Zero;
    private Vector2 textImguiSize = Vector2.Zero;
    private Vector2 textPosition = Vector2.Zero;

    private Config? configuration;
    private ImFontPtr uiFont;
    private bool fontLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleWindow"/> class.
    /// </summary>
    /// <param name="name">Window Name.</param>
    /// <param name="flags">Window Flags.</param>
    /// <param name="forceMainWindow">Indicates whether to force the window to be the main window.</param>
    /// <param name="translation">Translation text to be displayed in the window.</param>
    /// <param name="translationSemaphore">Semaphore to control access to the translation resource.</param>
    /// <param name="displayTranslation">Indicates whether to display the translation text.</param>
    /// <param name="curentTranslationId">Current translation ID.</param>
    /// <param name="textDimensions">Dimensions of the text.</param>
    /// <param name="textImguiSize">Size of the text in ImGui.</param>
    /// <param name="textPosition">Position of the text.</param>
    /// <param name="uiFont">Font used for the UI.</param>
    /// <param name="fontLoaded">Indicates whether the font is loaded.</param>
    public SimpleWindow(
  string name,
  ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
    ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration,
  bool forceMainWindow = false,
  string? translation = "",
  bool displayTranslation = false,
  int curentTranslationId = 0,
  Vector2 textDimensions = default,
  Vector2 textImguiSize = default,
  Vector2 textPosition = default,
  ImFontPtr uiFont = default,
  bool fontLoaded = default,
  SemaphoreSlim? translationSemaphore = default)
  : base(name, flags, forceMainWindow)
    {
      this.WindowName = name;
      this.translation = translation;
      this.displayTranslation = displayTranslation;
      this.currentTranslationId = curentTranslationId;
      this.textDimensions = textDimensions;
      this.textImguiSize = textImguiSize;
      this.textPosition = textPosition;
      this.configuration = Echoglossian.PluginInterface.GetPluginConfig() as Config;
      this.translationSemaphore = translationSemaphore;
      this.uiFont = uiFont;
      this.fontLoaded = fontLoaded;
    }

    public override void Draw()
    {
#if DEBUG
      // PluginLog.Debug("Inside DrawTranslatedDialogueWindow method!");
#endif
      ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(
          this.textPosition.X + (this.textDimensions.X / 2) - (this.textImguiSize.X / 2),
          this.textPosition.Y - this.textImguiSize.Y - 20) + this.configuration.ImGuiWindowPosCorrection);
      if (this.fontLoaded)
      {
#if DEBUG
        // PluginLog.Debug("Pushing font");
#endif

        Echoglossian.UINewFontHandler.GeneralFontHandle.Push();
      }

      float size = Math.Min(
          (this.textDimensions.X * this.configuration.ImGuiTalkWindowWidthMult) + (ImGui.GetStyle().WindowPadding.X * 2),
          (ImGui.CalcTextSize(this.translation).X * 1.25f) + (ImGui.GetStyle().WindowPadding.X * 2));
      ImGui.SetNextWindowSizeConstraints(new Vector2(size, 0), new Vector2(size, this.textDimensions.Y * this.configuration.ImGuiTalkWindowHeightMult));
      ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(this.configuration.OverlayTalkTextColor, 255));
      if (this.configuration.TranslateNpcNames)
      {
        string name = string.Empty; // GetTranslatedNpcNameForWindow();
        if (!name.IsNullOrEmpty())
        {
          ImGui.Begin(
            name,
            ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoMouseInputs
            | ImGuiWindowFlags.NoScrollbar);
        }
        else
        {
          ImGui.Begin(
            "Talk translation",
            ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoMouseInputs
            | ImGuiWindowFlags.NoScrollbar);
        }
      }
      else
      {
        ImGui.Begin(
          "Talk translation",
          ImGuiWindowFlags.NoTitleBar
          | ImGuiWindowFlags.NoNav
          | ImGuiWindowFlags.AlwaysAutoResize
          | ImGuiWindowFlags.NoFocusOnAppearing
          | ImGuiWindowFlags.NoMouseInputs
          | ImGuiWindowFlags.NoScrollbar);
      }

      ImGui.SetWindowFontScale(this.configuration.FontScale);
      if (this.translationSemaphore.Wait(0))
      {
        ImGui.TextWrapped(this.translation);

        this.translationSemaphore.Release();
      }
      else
      {
        ImGui.Text(Resources.WaitingForTranslation);
      }

      this.textImguiSize = ImGui.GetWindowSize();

      ImGui.PopStyleColor(1);

      ImGui.End();
      if (this.fontLoaded)
      {
#if DEBUG
        // PluginLog.Debug("Popping font!");
#endif
        Echoglossian.UINewFontHandler.GeneralFontHandle.Pop();
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposedValue)
      {
        if (disposing)
        {
          this.translationSemaphore.Dispose();
        }

        this.disposedValue = true;
      }
    }

    public void Dispose()
    {
      this.Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
