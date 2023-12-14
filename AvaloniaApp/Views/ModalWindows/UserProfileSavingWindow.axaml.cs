using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaFirstApp;

public partial class UserProfileSavingWindow : Window
{
    public UserProfileSavingWindow()
    {
        InitializeComponent();
        SaveBtn.Click += SaveBtn_Click;
        CancelBtn.Click += CancelBtn_Click;
    }

    private void CancelBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void SaveBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(NameBox.Text);
    }
}