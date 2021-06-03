using System;
using System.Threading.Tasks;
using GuardLibrary;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BrowserlessBot
{
    public class AdminNotifier : INotifier
    {
        private readonly ITelegramBotClient botClient;

        public AdminNotifier(ITelegramBotClient botClient)
        {
            Guard.Ensure(botClient, nameof(botClient)).IsNotNull();

            this.botClient = botClient;
        }

        public async Task Notify(Chat chat, string message)
        {
            Guard.Ensure(message, nameof(message)).IsNotNullOrEmpty();

            if (chat != null && 
                chat.Id == Settings.AdminChatId)
            {
                return;
            }

            try
            {
                await botClient.SendTextMessageAsync(new ChatId(Settings.AdminChatId), message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred when notifying admin. AdminChatId: {Settings.AdminChatId}. Message: {message}. {ex}");
            }
        }
    }
}