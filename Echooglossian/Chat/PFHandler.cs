using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Memory;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Echooglossian.Chat
{
    internal class PFHandler : IDisposable
    {
        private readonly MenuItem contextMenuItem;

        public PFHandler()
        {
            contextMenuItem = new MenuItem
            {
                UseDefaultPrefix = true,
                Name = "Translate",
                OnClicked = TranslatePF
            };

            Service.contextMenu.OnMenuOpened += OnContextMenuOpened;
        }

        private void OnContextMenuOpened(IMenuOpenedArgs args)
        {
            if (!Service.config.PFModuleEnabled)
                return;

            if (args.AddonName == "LookingForGroupDetail")
                args.AddMenuItem(contextMenuItem);
        }

        private unsafe void TranslatePF(IMenuItemClickedArgs args)
        {
            AddonLookingForGroupDetail* PfAddonPtr = (AddonLookingForGroupDetail*)args.AddonPtr;
            string description = PfAddonPtr->DescriptionString.ToString();

            // fix weird characters in pf description
            description = description
                .Replace("\u0002\u0012\u0002\u0037\u0003", " \uE040 ")
                .Replace("\u0002\u0012\u0002\u0038\u0003", " \uE041 ");

            Message PFmessage = new Message("PF", XivChatType.Say, description);

            string category = PfAddonPtr->CategoriesString.ToString();
            category = string.Join(" ", Regex.Matches(category, @"\[[^\]]+\]")
                                             .Cast<Match>()
                                             .Select(m => m.Value));
            if (string.IsNullOrWhiteSpace(category))
            {
                category = "null";
            }

            byte* dutyText = PfAddonPtr->DutyNameTextNode->GetText();
            string duty = MemoryHelper.ReadSeStringNullTerminated((nint)dutyText).TextValue;
            if (string.IsNullOrWhiteSpace(duty))
            {
                duty = "null";
            }

            string context = $"Tags: {category}, Duty: {duty}";
            PFmessage.Context = context;

            Task.Run(() => TranslatePFAsync(PFmessage));
        }

        private static async Task TranslatePFAsync(Message message)
        {
            var translation = await Service.translationHandler.TranslateChat(message);
            ChatHandler.OutputMessage(message, translation);
        }

        public void Dispose()
        {
            Service.contextMenu.OnMenuOpened -= OnContextMenuOpened;
        }
    }
}
