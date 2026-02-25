using Avalonia.Controls;
using BibliotecaAvalonia.ViewModels;

namespace BibliotecaAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Asignamos el ViewModel como el contexto de datos de esta ventana
        DataContext = new MainWindowViewModel();
    }
}