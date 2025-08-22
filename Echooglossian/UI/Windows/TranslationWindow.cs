using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Echooglossian.Localization;
using Echooglossian.Utils;
using System.Globalization;
using System.Numerics;

namespace Echooglossian.UI.Windows
{
    internal class TranslationWindow : Window
    {
        public TranslationWindow(Plugin plugin) : base(
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
