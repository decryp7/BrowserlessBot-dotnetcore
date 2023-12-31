﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BrowserlessBot
{
    public class ContentCommandHandler : IBotCommandHandler
    {
        public ITelegramBotClient BotClient { get; set; }
        public INotifier Notifier { get; set; }

        public string Command { get; } = "/content";

        public string CommandDescription { get; } =
            "/content {url}" +
            Environment.NewLine +
            "Get the content of {url}";

        public async Task Handle(Chat chat, string commandArgs)
        {
            if (string.IsNullOrEmpty(commandArgs.Trim()))
            {
                await BotClient.SendTextMessageAsync(chat, $"Missing url parameter. Usage: {CommandDescription}");
                return;
            }

            Message message = await BotClient.SendTextMessageAsync(chat, $"{chat.FirstName}, please hold while I get the content for {commandArgs}.");

            try
            {
                using (IBotBrowser botBrowser = new BotBrowser())
                {
                    Response response = await botBrowser.Goto(commandArgs, async page =>
                    {
                        string html = await page.GetContentAsync();

                        // convert string to stream
                        byte[] byteArray = Encoding.UTF8.GetBytes(html);
                        //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
                        MemoryStream stream = new MemoryStream(byteArray);

                        await BotClient.SendDocumentAsync(chat,
                            new InputOnlineFile(stream, $"{commandArgs}.txt"));
                        await BotClient.DeleteMessageAsync(chat, message.MessageId);
                        await Notifier.Notify(chat, $"I have gotten content from {commandArgs} for {chat.FirstName}.");
                    });

                    if (!response.Ok)
                    {
                        await BotClient.EditMessageTextAsync(chat, message.MessageId,
                            $"Sorry {chat.FirstName}, I am unable to navigate to {commandArgs}. {response.ToString()}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await BotClient.EditMessageTextAsync(chat, message.MessageId,
                    $"Sorry {chat.FirstName}, I am unable to get content for {commandArgs}. Error: {ex.Message}");
                await Notifier.Notify(chat, $"I am unable to get content for {chat.FirstName}. CommandArgs: {commandArgs} Error: {ex.Message}");
            }
        }
    }
}