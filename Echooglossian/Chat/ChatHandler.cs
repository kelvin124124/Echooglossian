using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Echooglossian.Localization;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echooglossian.Chat
{
    internal partial class ChatHandler : IDisposable
    {
        private static readonly StringBuilder sb = new();
        private readonly Dictionary<string, int> lastMessageTime = [];

        public ChatHandler() => Service.chatGui.ChatMessage += OnChatMessage;

        private void OnChatMessage(XivChatType type, int _, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!isHandled)
                HandleChatMessage(type, sender, message);
        }

        private async void HandleChatMessage(XivChatType type, SeString sender, SeString message)
        {
            if (!Service.configuration.ChatModuleEnabled || sender.TextValue.Contains("[E]") || !Service.configuration.CHAT_SelectedChatTypes.Contains(type))
                return;

            // Get sender name
            var playerPayload = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
            string playerName = Service.sanitizer.Sanitize(playerPayload?.PlayerName ?? sender.ToString());
            string localPlayerName = Service.sanitizer.Sanitize(Service.clientState.LocalPlayer?.Name.ToString() ?? string.Empty);

            if (type == XivChatType.TellOutgoing)
                playerName = localPlayerName;
            if (playerName.Contains(localPlayerName))
                return;

            // Create and validate message
            var chatMessage = new Message(
                sender: playerName,
                type: type,
                content: Service.sanitizer.Sanitize(message.TextValue)
            );
            chatMessage.Context = GetChatMessageContext();

            if (IsFilteredMessage(chatMessage) || IsJPFilteredMessage(chatMessage))
                return;

            // Translate if needed
            if (await IsCustomSourceLanguage(chatMessage))
            {
                string translation = await Service.translationHandler.TranslateChat(chatMessage);
                OutputMessage(chatMessage, translation);
            }
            else
            {
                Service.pluginLog.Info("Message language is not selected source language. Skipped.");
            }
        }

        public unsafe string GetChatMessageContext()
        {
            var chatPanelIndex = GetActiveChatLogPanel();
            try
            {
                var chatLogPanelPtr = Service.gameGui.GetAddonByName($"ChatLogPanel_{chatPanelIndex}");
                if (chatLogPanelPtr != 0)
                {
                    var chatLogPanel = (AddonChatLogPanel*)chatLogPanelPtr.Address;
                    var lines = SeString.Parse((byte*)chatLogPanel->ChatText->GetText()).TextValue
                        .Split('\r')
                        .TakeLast(15)
                        .Select(line => line.Trim())
                        .ToList();

                    if (Service.condition[ConditionFlag.BoundByDuty])
                        lines.Add("In instanced area: true");
                    if (Service.condition[ConditionFlag.InCombat])
                        lines.Add("In combat: true");

                    return string.Join('\n', lines);
                }
            }
            catch (Exception ex)
            {
                Service.pluginLog.Error(ex, "Failed to read chat panel.");
            }
            return string.Empty;
        }

        public unsafe nint GetActiveChatLogPanel()
        {
            var addon = (AddonChatLog*)Service.gameGui.GetAddonByName("ChatLog").Address;
            return addon == null ? 0 : addon->TabIndex;
        }

        private static async Task<bool> IsCustomSourceLanguage(Message chatMessage) =>
            Service.configuration.CHAT_SelectedSourceLanguages.Contains(
                await Service.translationHandler.DetermineLanguage(chatMessage.Content)
            );

        internal static void OutputMessage(Message chatMessage, string translation)
        {
            if (translation == chatMessage.Content)  // no need to output if translation is the same
            {
                Service.pluginLog.Info("Translation is the same as original. Skipped.");
                return;
            }

            sb.Clear().Append(chatMessage.Content).Append(" || ").Append(translation);

            Service.chatGui.Print(new XivChatEntry
            {
                Type = chatMessage.Type,
                Name = new SeString(new PlayerPayload("[E] " + chatMessage.Sender, 0)),
                Message = sb.ToString(),
            });
        }

        private bool IsFilteredMessage(Message chatMessage) =>
            chatMessage.Content.Trim().Length < 2 || IsMacroMessage(chatMessage.Sender);

        private bool IsMacroMessage(string playerName)
        {
            int now = Environment.TickCount;

            if (lastMessageTime.Count > 20)
            {
                foreach (var key in lastMessageTime.Where(kv => now - kv.Value > 10000).Select(kv => kv.Key).ToList())
                    lastMessageTime.Remove(key);
            }

            if (lastMessageTime.TryGetValue(playerName, out int lastMsgTime) && now - lastMsgTime < 650)
            {
                lastMessageTime[playerName] = now;
                return true;
            }

            lastMessageTime[playerName] = now;
            return false;
        }

        private static bool IsJPFilteredMessage(Message chatMessage)
        {
            if (ChatRegex.JPWelcomeRegex().IsMatch(chatMessage.Content))
            {
                OutputMessage(chatMessage, Resources.Chat_WelcomeStr);
                return true;
            }
            if (ChatRegex.JPByeRegex().IsMatch(chatMessage.Content))
            {
                OutputMessage(chatMessage, Resources.Chat_GGstr);
                return true;
            }
            if (ChatRegex.JPDomaRegex().IsMatch(chatMessage.Content))
            {
                OutputMessage(chatMessage, Resources.Chat_DomaStr);
                return true;
            }
            return false;
        }

        public void Dispose() => Service.chatGui.ChatMessage -= OnChatMessage;
    }
}
