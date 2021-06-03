using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BrowserlessBot
{
    public interface INotifier
    {
        Task Notify(Chat chat, string message);
    }
}