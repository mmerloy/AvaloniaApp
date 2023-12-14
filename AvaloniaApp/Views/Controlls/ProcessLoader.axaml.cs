using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaloniaFirstApp.Views.Controlls
{
    public class ProcessLoader : TemplatedControl
    {

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }


        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<ProcessLoader, bool>(
                nameof(IsActive),
                defaultValue: true);
    }
}
