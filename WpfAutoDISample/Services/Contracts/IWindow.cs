using System.Windows;

namespace WpfAutoDISample.Services.Contracts;

public interface IWindow
{
    event RoutedEventHandler Loaded;

    void Show();
}