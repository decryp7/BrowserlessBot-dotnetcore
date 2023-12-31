﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using GuardLibrary;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BrowserlessBot
{
    public class BotCommandHandlerService : IBotCommandHandlerService
    {
        private readonly IDictionary<string, IBotCommandHandler> botCommandHandlers =
            new Dictionary<string, IBotCommandHandler>(StringComparer.OrdinalIgnoreCase);

        private readonly ITelegramBotClient botClient;
        private readonly INotifier notifier;

        public BotCommandHandlerService(ITelegramBotClient botClient, 
            INotifier notifier, 
            params IBotCommandHandler[] handlers)
        {
            Guard.Ensure(botClient, nameof(botClient)).IsNotNull();
            Guard.Ensure(notifier, nameof(notifier)).IsNotNull();

            this.botClient = botClient;
            this.notifier = notifier;

            foreach (IBotCommandHandler botCommandHandler in handlers)
            {
                botCommandHandler.BotClient = botClient;
                botCommandHandler.Notifier = notifier;
                botCommandHandlers[botCommandHandler.Command] = botCommandHandler;
            }
        }

        public async Task Handle(Chat chat, string command)
        {
            Guard.Ensure(command, nameof(command)).IsNotNullOrEmpty();

            int indexOfFirstSpace = command.IndexOf(' ');

            string cmd = command;
            string cmdArgs = string.Empty;
            if (indexOfFirstSpace > -1)
            {
                cmd = command.Substring(0, indexOfFirstSpace);
                cmdArgs = command.Substring(indexOfFirstSpace);
            }

            if (!botCommandHandlers.TryGetValue(cmd, out IBotCommandHandler botCommandHandler))
            {
                await notifier.Notify(chat, $"I am unable to handle {command} from {chat.FirstName}");
                await botClient.SendTextMessageAsync(chat, $"Sorry {chat.FirstName}, I am unable to handle {command}.");
                await botClient.SendTextMessageAsync(chat, $"{this.GetAvailableCommands()}");
                return;
            }

            await botCommandHandler.Handle(chat, cmdArgs);
        }

        public string GetAvailableCommands()
        {
            StringBuilder stringBuilder = new StringBuilder("Available Commands:" + Environment.NewLine);

            int commandIndex = 0;
            foreach (KeyValuePair<string, IBotCommandHandler> botCommandHandler in botCommandHandlers)
            {
                if (string.IsNullOrEmpty(botCommandHandler.Value.CommandDescription))
                {
                    continue;
                }
                stringBuilder.AppendLine($"{++commandIndex}. {botCommandHandler.Value.CommandDescription}");
                stringBuilder.AppendLine("");
            }

            return stringBuilder.ToString();
        }
    }
}