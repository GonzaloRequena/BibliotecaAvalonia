using Avalonia.Controls;
using BibliotecaAvalonia.ViewModels;

namespace BibliotecaAvalonia.Views;

public partial class ValoracionView : Window
{
    public ValoracionView()
    {
        InitializeComponent();
        
        // Conectamos la acción de cierre del ViewModel con el método Close de la ventana
        DataContextChanged += (s, e) =>
        {
            if (DataContext is ValoracionViewModel vm)
            {
                vm.CloseAction = () => Close();
            }
        };
    }
}