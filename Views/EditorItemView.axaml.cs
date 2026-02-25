using Avalonia.Controls;
using Avalonia.Interactivity;
using BibliotecaAvalonia.ViewModels;

namespace BibliotecaAvalonia.Views;

public partial class EditorItemView : Window
{
    public EditorItemView()
    {
        InitializeComponent();
        // Cuando el DataContext cambie, asignamos la acción de cerrar
        DataContextChanged += (s, e) =>
        {
            if (DataContext is EditorItemViewModel vm)
            {
                vm.CloseAction = () => Close();
            }
        };
    }

    // Cerramos la ventana sin hacer nada
    private void Cancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}