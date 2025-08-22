using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Echoglossian.Localization;
using Echoglossian.Utils;
using System.Globalization;
using System.Numerics;

namespace Echoglossian.UI.Windows
{
    internal class TranslationWindow : Window
    {
        public TranslationWindow(Echoglossian plugin) : base(
            $"Translation window",
            ImGuiWindowFlags.AlwaysAutoResize)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(900, 700),
                MaximumSize = new Vector2(1920, 1080)
            };

            Resources.Culture = (CultureInfo)Service.config.SelectedPluginLanguage;
        }

        public override void Draw()
        {
            // Language selection

            // Input message

            // Output message

            // Reverse translation
        }
    }
}
