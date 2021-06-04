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

            ConnectOptions connectOptions = new ConnectOptions()
            {
                BrowserWSEndpoint = Settings.BrowserlessEndpoint
            };

            ScreenshotOptions screenshotOptions = new ScreenshotOptions()
            {
                FullPage = true,
                Type = ScreenshotType.Png
            };

            NavigationOptions navigationOptions = new NavigationOptions()
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
            };

            Message message = await BotClient.SendTextMessageAsync(chat, $"{chat.FirstName}, please hold while I generate the screenshot for {commandArgs}.");

            try
            {
                await using (Browser browser = await Puppeteer.ConnectAsync(connectOptions))
                await using (Page page = await browser.NewPageAsync())
                {
                    Response response = page.GoToAsync(commandArgs, navigationOptions).Result;

                    if (!response.Ok)
                    {
                        await BotClient.EditMessageTextAsync(chat, message.MessageId,
                            $"Sorry { chat.FirstName}, I am unable to navigate to {commandArgs}. {response.ToString()}");
                        return;
                    }

                    await using (Stream screenshotStream = await page.ScreenshotStreamAsync(screenshotOptions))
                    {
                        await BotClient.SendDocumentAsync(chat,
                            new InputOnlineFile(screenshotStream, $"{commandArgs}.png"));
                        await BotClient.DeleteMessageAsync(chat, message.MessageId);
                        await Notifier.Notify(chat, $"I have generated screenshot from {commandArgs} for {chat.FirstName}.");
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