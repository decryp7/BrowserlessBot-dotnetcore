using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BrowserlessBot
{
    public class ScreenshotCommandHandler : IBotCommandHandler
    {
        public ITelegramBotClient BotClient { get; set; }
        public INotifier Notifier { get; set; }

        public string Command { get; } = "/screenshot";

        public string CommandDescription { get; } =
            "/screenshot {url}" +
            Environment.NewLine +
            "Get the screenshot of target {url}";
            
        public async Task Handle(Chat chat, string commandArgs)
        {
            if (string.IsNullOrEmpty(commandArgs.Trim()))
            {
                await BotClient.SendTextMessageAsync(chat, $"Missing url parameter. Usage: {CommandDescription}");
                return;
            }

            ScreenshotOptions screenshotOptions = new ScreenshotOptions()
            {
                FullPage = true,
                Type = ScreenshotType.Png
            };

            Message message = await BotClient.SendTextMessageAsync(chat, $"{chat.FirstName}, please hold while I generate the screenshot for {commandArgs}.");

            try
            {
                using (IBotBrowser botBrowser = new BotBrowser())
                {
                    Response response = await botBrowser.Goto(commandArgs, async page =>
                    {
                        await using (Stream screenshotStream = await page.ScreenshotStreamAsync(screenshotOptions))
                        {
                            await BotClient.SendDocumentAsync(chat,
                                new InputOnlineFile(screenshotStream, $"{commandArgs}.png"));
                            await BotClient.DeleteMessageAsync(chat, message.MessageId);
                            await Notifier.Notify(chat, $"I have generated screenshot from {commandArgs} for {chat.FirstName}.");
                        }
                    });

                    if (!response.Ok)
                    {
                        await BotClient.EditMessageTextAsync(chat, message.MessageId,
                            $"Sorry { chat.FirstName}, I am unable to navigate to {commandArgs}. {response.ToString()}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await BotClient.EditMessageTextAsync(chat, message.MessageId,
                    $"Sorry {chat.FirstName}, I am unable to generate screenshot for {commandArgs}. Error: {ex.Message}");
                await Notifier.Notify(chat, $"I am unable to generate screenshot for {chat.FirstName}. CommandArgs: {commandArgs} Error: {ex.Message}");
            }
        }
    }
}