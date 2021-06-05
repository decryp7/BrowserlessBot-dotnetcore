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

        public async Task<Response> Goto(string url, Func<Page, Task> pageAction)
        {
            browser = await Puppeteer.ConnectAsync(connectOptions);
            page = await browser.NewPageAsync();

            Response response = await page.GoToAsync(url, navigationOptions);

            if (!response.Ok)
            {
                return response;
            }

            //from https://github.com/chenxiaochun/blog/issues/38
            await page.EvaluateExpressionAsync(
                @"new Promise((resolve, reject) => {
                            var totalHeight = 0;
                            var distance = 100;
                            var timer = setInterval(() => {
                                var scrollHeight = document.body.scrollHeight;
                                window.scrollBy(0, distance);
                                totalHeight += distance;

                                if(totalHeight >= scrollHeight){
                                    clearInterval(timer);
                                    resolve();
                                }
                            }, 100);
                        });");
            await pageAction(page);

            return response;
        }
    }
}