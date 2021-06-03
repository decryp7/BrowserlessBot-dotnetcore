using System.Threading.Tasks;

namespace BrowserlessBot
{
    public interface INotifier
    {
        Task Notify(string message);
    }
}