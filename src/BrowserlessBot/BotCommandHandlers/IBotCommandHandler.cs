using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BrowserlessBot
{
    public interface IBotCommandHandler
    {
        ITelegramBotClient BotClient { set; }

        INotifier Notifier { set; }

        string Command { get; }

        string CommandDescription { get; }

        Task Handle(Chat chat, string commandArgs);
    }
}