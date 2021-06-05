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
    public class PDFCommandHandler : IBotCommandHandler
    {
        public ITelegramBotClient BotClient { get; set; }
        public INotifier Notifier { get; set; }

        public string Command { get; } = "/pdf";

        public string CommandDescription { get; } =
            "/pdf {url}" +
            Environment.NewLine +
            "Get the pdf of target {url}.";
        
        public async Task Handle(Chat chat, string commandArgs)
        {
            if (string.IsNullOrEmpty(commandArgs.Trim()))
            {
                await BotClient.SendTextMessageAsync(chat, $"Missing url parameter. Usage: {CommandDescription}");
                return;
            }

            PdfOptions pdfOptions = new PdfOptions()
            {
                Format = PaperFormat.A4,
                Landscape = true,
                PrintBackground = true
            };

            Message message = await BotClient.SendTextMessageAsync(chat, $"{chat.FirstName}, please hold while I generate the PDF for {commandArgs}.");

            try
            {
                using (IBotBrowser botBrowser = new BotBrowser())
                {
                    Response response = await botBrowser.Goto(commandArgs, async page =>
                    {
                        await page.EmulateMediaTypeAsync(MediaType.Screen);

                        await using (Stream pdfStream = await page.PdfStreamAsync(pdfOptions))
                        {
                            await BotClient.SendDocumentAsync(chat, new InputOnlineFile(pdfStream, $"{commandArgs}.pdf"));
                            await BotClient.DeleteMessageAsync(chat, message.MessageId);
                            await Notifier.Notify(chat, $"I have generated PDF from {commandArgs} for {chat.FirstName}.");
                        }
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
                    $"Sorry {chat.FirstName}, I am unable to generate PDF for {commandArgs}. Error: {ex.Message}");
                await Notifier.Notify(chat, $"I am unable to generate PDF for {chat.FirstName}. CommandArgs: {commandArgs} Error: {ex.Message}");
            }
        }
    }
}