﻿using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BrowserlessBot
{
    public class StartCommandHandler : IBotCommandHandler
    {
        public ITelegramBotClient BotClient { get; set; }
        public INotifier Notifier { get; set; }

        public string Command { get; } = "/start";

        public string CommandDescription { get; }

        public async Task Handle(Chat chat, string commandArgs)
        {
            User bot = await BotClient.GetMeAsync();
            await BotClient.SendTextMessageAsync(chat, $"Hello {chat.FirstName}, {bot.FirstName} at your service.");
            await Notifier.Notify(chat, $"I have received chat request from {chat.FirstName}.");
        }
    }
}