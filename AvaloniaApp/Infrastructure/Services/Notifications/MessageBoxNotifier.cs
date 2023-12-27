using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaFirstApp.Infrastructure.Services.Notifications;

public class MessageBoxNotifier : INotifier
{
    public Task NotifyErrorAsync(string errorMsg, CancellationToken cToken = default)
        => Show(Icon.Error, errorMsg, "Ошибка");

    public Task NotifyInfoAsync(string info, CancellationToken cToken = default)
        => Show(Icon.Info, info, "Уведомление");

    public Task NotifyWarningAsync(string warningMsg, CancellationToken cToken = default)
        => Show(Icon.Warning, warningMsg, "Предупреждение");


    private Task Show(Icon icon, string msg, string title)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title,
            msg, icon: icon, windowStartupLocation: WindowStartupLocation.CenterOwner
        );

        return box.ShowWindowDialogAsync(App.MainWindow);
    }
}
