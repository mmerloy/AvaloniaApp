using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.Infrastructure.Services.Notifications;

public interface INotifier
{
    Task NotifyErrorAsync(string errorMsg, CancellationToken cToken = default);

    Task NotifyWarningAsync(string warningMsg, CancellationToken cToken = default);

    Task NotifyInfoAsync(string info, CancellationToken cToken = default);
}
