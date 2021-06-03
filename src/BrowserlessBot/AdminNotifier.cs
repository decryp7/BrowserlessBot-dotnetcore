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

        public async Task Notify(string message)
        {
            Guard.Ensure(message, nameof(message)).IsNotNullOrEmpty();

            await botClient.SendTextMessageAsync(new ChatId(Settings.AdminChatId), message);
        }
    }
}