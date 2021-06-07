using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BrowserlessBot
{
    public class AboutCommandHandler : IBotCommandHandler
    {
        public ITelegramBotClient BotClient { get; set; }
        public INotifier Notifier { get; set; }


        public string Command { get; } = "/about";

        public string CommandDescription { get; } =
            "/about" +
            Environment.NewLine +
            "Display information about this bot.";

        public async Task Handle(Chat chat, string commandArgs)
        {
            await BotClient.SendTextMessageAsync(chat,
                @$"Hello {chat.FirstName}, I am a browserless bot.
Contact for bug reports: @decryp7
Source code: https://dev.decryptology.net/decryp7/BrowserlessBot-dotnetcore");
        }
    }
}