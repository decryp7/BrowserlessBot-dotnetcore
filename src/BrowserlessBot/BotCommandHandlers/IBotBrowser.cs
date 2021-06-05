using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace BrowserlessBot
{
    public interface IBotBrowser : IDisposable
    {
        Task<Response> Goto(string url, Action<Page> pageAction);
    }
}