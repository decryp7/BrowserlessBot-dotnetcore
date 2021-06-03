﻿using System;
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
            }

            ConnectOptions connectOptions = new ConnectOptions()
            {
                BrowserWSEndpoint = Settings.BrowserlessEndpoint
            };

            PdfOptions pdfOptions = new PdfOptions()
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            };

            NavigationOptions navigationOptions = new NavigationOptions()
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
            };

            Message message = await BotClient.SendTextMessageAsync(chat, $"Please hold while I generate the PDF for {commandArgs}.");

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

                    await using (Stream pdfStream = await page.PdfStreamAsync(pdfOptions))
                    {
                        await BotClient.SendDocumentAsync(chat, new InputOnlineFile(pdfStream, $"{commandArgs}.pdf"));
                    }
                }
            }
            catch (Exception ex)
            {
                await BotClient.EditMessageTextAsync(chat, message.MessageId,
                    $"Unable to generate PDF for {commandArgs}. Error: {ex.Message}");
            }
            finally
            {
                await BotClient.DeleteMessageAsync(chat, message.MessageId);
            }
        }
    }
}