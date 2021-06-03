using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BrowserlessBot
{
    public class HtmlCommandHandler : IBotCommandHandler
    {
        public ITelegramBotClient BotClient { get; set; }

        public string Command { get; } = "/html";

        public string CommandDescription { get; } =
            "/html {url}" +
            Environment.NewLine +
            "Get the html of {url}";

        public async Task Handle(Chat chat, string commandArgs)
        {
            if (string.IsNullOrEmpty(commandArgs.Trim()))
            {
                await BotClient.SendTextMessageAsync(chat, $"Missing url parameter. Usage: {CommandDescription}");
            }

            ConnectOptions connectOptions = new ConnectOptions()
            {
                BrowserWSEndpoint = Settings.BrowserlessEndpoint
            };

            NavigationOptions navigationOptions = new NavigationOptions()
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
            };

            Message message = await BotClient.SendTextMessageAsync(chat, $"Please hold while I get the html for {commandArgs}.");

            try
            {
                await using (Browser browser = await Puppeteer.ConnectAsync(connectOptions))
                await using (Page page = await browser.NewPageAsync())
                {
                    Response response = page.GoToAsync(commandArgs, navigationOptions).Result;

                    if (!response.Ok)
                    {
                        await BotClient.EditMessageTextAsync(chat, message.MessageId,
                            "Unable to navigate to {commandArgs}. {response.ToString()}");
                        return;
                    }

                    string html = await page.GetContentAsync();

                    // convert string to stream
                    byte[] byteArray = Encoding.UTF8.GetBytes(html);
                    //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
                    MemoryStream stream = new MemoryStream(byteArray);

                    await BotClient.SendDocumentAsync(chat,
                            new InputOnlineFile(stream, $"{commandArgs}.txt"));
                }
            }
            catch (Exception ex)
            {
                await BotClient.EditMessageTextAsync(chat, message.MessageId,
                    $"Unable to get html for {commandArgs}. Error: {ex.Message}");
            }
            finally
            {
                await BotClient.DeleteMessageAsync(chat, message.MessageId);
            }
        }
    }
}