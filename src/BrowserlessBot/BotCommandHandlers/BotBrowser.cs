using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace BrowserlessBot
{
    public class BotBrowser : IBotBrowser
    {
        private readonly ConnectOptions connectOptions;
        private readonly NavigationOptions navigationOptions;
        private Browser browser;
        private Page page;

        public BotBrowser()
        {
            connectOptions = new ConnectOptions()
            {
                BrowserWSEndpoint = Settings.BrowserlessEndpoint
            };

            navigationOptions = new NavigationOptions()
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
            };
        }

        public void Dispose()
        {
            page?.Dispose();
            browser?.Dispose();
        }

        public async Task<Response> Goto(string url, Action<Page> pageAction)
        {
            browser = await Puppeteer.ConnectAsync(connectOptions);
            page = await browser.NewPageAsync();

            Response response = await page.GoToAsync(url, navigationOptions);

            if (!response.Ok)
            {
                return response;
            }

            await Task.Run(() => pageAction);

            return response;
        }
    }
}