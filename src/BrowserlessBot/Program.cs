using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace BrowserlessBot
{
    class Program
    {
        private static ITelegramBotClient botClient;
        private static IBotCommandHandlerService botCommandHandlerService;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Missing parameters! Mandatory parameters are TELEGRAM_BOT_TOKEN BROWSERLESS_ENDPOINT BROWSERLESS_TOKEN in this order.");
                Environment.Exit(0);
            }

            Settings.BrowserlessEndpoint = $"wss://{args[1]}?token={args[2]}";
            botClient = new TelegramBotClient(args[0].Trim());
            botCommandHandlerService = new BotCommandHandlerService(botClient,
                new StartCommandHandler(),
                new PDFCommandHandler(),
                new ScreenshotCommandHandler(),
                new ContentCommandHandler());

            try
            {
                var me = botClient.GetMeAsync().Result;
                Console.WriteLine(
                    $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There is an exception starting telegram bot client. {ex}");
                Environment.Exit(0);
            }

            botClient.OnMessage += BotClientOnOnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            botClient.StopReceiving();
        }

        private static async void BotClientOnOnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Entities != null &&
                    e.Message.Entities.Length > 0 &&
                    e.Message.Entities[0].Type == MessageEntityType.BotCommand)
                {
                    string command = e.Message.Text.Substring(e.Message.Entities[0].Offset);
                    await botCommandHandlerService.Handle(e.Message.Chat,
                            e.Message.Text.Substring(e.Message.Entities[0].Offset));

                    return;
                }

                if (e.Message.Text != null)
                {
                    Console.WriteLine(
                        $"Received a text message from {e.Message.From.FirstName}. Message: {e.Message.Text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal Error has occurred. {ex}");
                await botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: $"Internal Error has occurred."
                );
            }
        }
    }
}
