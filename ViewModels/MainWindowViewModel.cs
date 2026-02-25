using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using BibliotecaAvalonia.Models;
using BibliotecaAvalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BibliotecaAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Esta es la lista que se enlazará al DataGrid
    [ObservableProperty]
    private ObservableCollection<Articulo> _articulos;

    // Aquí guardaremos el artículo que el usuario seleccione con el ratón
    [ObservableProperty]
    private Articulo? _articuloSeleccionado;

    // Texto del buscador
    [ObservableProperty]
    private string _textoBusqueda = string.Empty;
    
    // Opciones para el filtro de tipo
    [ObservableProperty]
    private List<string> _tiposFiltro = new() { "Todos", "Libros", "Audiolibros" };

    // Tipo seleccionado en el filtro
    [ObservableProperty]
    private string _tipoSeleccionado = "Todos";

    public MainWindowViewModel()
    {
    // Al arrancar, cargamos los datos de la base de datos

        var listaInicial = GestorBD.ObtenerTodos();
        
        if (!listaInicial.Any())
        {
            GestorBD.InsertarArticulo(new Libro("El Códice de Avalonia", 2024, DateTime.Now, "843760494X"));
            GestorBD.InsertarArticulo(new Libro("Cien años de soledad", 1967, DateTime.Now, "0307474720"));
            GestorBD.InsertarArticulo(new Audiolibro("Sapiens", 2014, DateTime.Now, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(25)));

            listaInicial = GestorBD.ObtenerTodos();
        }
        _articulos = new ObservableCollection<Articulo>(listaInicial);
    }
    
    // --- COMANDOS ---

    [RelayCommand]
    private async void NuevoLibro()
    {
        // Creamos el ViewModel para un nuevo libro
        var vm = new EditorItemViewModel(esLibro: true);
        var ventana = new EditorItemView { DataContext = vm };

        // Abrimos la ventana como diálogo modal
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await ventana.ShowDialog(desktop.MainWindow);
            // Al volver, refrescamos la lista por si se guardó algo nuevo
            RefrescarLista();
        }
    }

    [RelayCommand]
    private async void NuevoAudiolibro()
    {
        var vm = new EditorItemViewModel(esLibro: false);
        var ventana = new EditorItemView { DataContext = vm };

        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await ventana.ShowDialog(desktop.MainWindow);
            RefrescarLista();
        }
    }

    [RelayCommand]
    private async void Editar()
    {
        if (ArticuloSeleccionado != null)
        {
            // Pasamos el artículo seleccionado al constructor de edición
            var vm = new EditorItemViewModel(ArticuloSeleccionado);
            var ventana = new EditorItemView { DataContext = vm };

            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await ventana.ShowDialog(desktop.MainWindow);
                RefrescarLista();
            }
        }
    }
    
    [RelayCommand]
    private void CambiarEstadoPrestamo()
    {
        // Sólo actuamos si hay un artículo seleccionado y es un Libro (implementa Prestable)
        if (ArticuloSeleccionado is Libro libro)
        {
            // Cambiamos el estado usando los métodos de la interfaz
            if (libro.Disponible) 
                libro.Prestar();
            else 
                libro.Devolver();

            // Persistimos en BD (usando el ID que ya tiene el objeto)
            if (libro.Id.HasValue)
            {
                GestorBD.ActualizarDisponibilidad(libro.Id.Value, libro.Disponible);
            }

            // 3. Forzamos a la UI a refrescar la fila (esto es necesario si no usas RaisePropertyChanged en Disponible)
            // Como 'Disponible' en Libro.cs no es una [ObservableProperty], refrescamos la lista o notificamos el cambio.
            RefrescarLista(); 
        }
    }
    
    [RelayCommand]
    private void Buscar()
    {
        // Pasamos el texto y el tipo seleccionado directamente
        var resultados = GestorBD.BuscarItems(TextoBusqueda, TipoSeleccionado);
        Articulos = new ObservableCollection<Articulo>(resultados);
    }

    // Este método asegura que la lista se actualice nada más tocar el desplegable
    partial void OnTipoSeleccionadoChanged(string value)
    {
        Buscar();
    }
    
    [RelayCommand]
    private void Eliminar()
    {
        // Si hay algo seleccionado y tiene ID (es decir, viene de la BD)
        if (ArticuloSeleccionado != null && ArticuloSeleccionado.Id.HasValue)
        {
            // 1. Borramos de la base de datos
            GestorBD.EliminarArticulo(ArticuloSeleccionado.Id.Value);
            
            // 2. Borramos de la lista visual
            Articulos.Remove(ArticuloSeleccionado);
            
            // Limpiamos la selección
            ArticuloSeleccionado = null;
        }
    }
    
    // Método auxiliar para recargar los datos de la BD
    private void RefrescarLista()
    {
        var lista = GestorBD.ObtenerTodos();
        Articulos = new ObservableCollection<Articulo>(lista);
    }
    
    [RelayCommand]
    private async Task AñadirValoracion()
    {
        if (ArticuloSeleccionado is Valorable valorable && ArticuloSeleccionado.Id.HasValue)
        {
            var vm = new ValoracionViewModel();
            var vista = new ValoracionView { DataContext = vm };
        
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await vista.ShowDialog(desktop.MainWindow);

                if (vm.OperacionConfirmada) // Necesitas añadir esta propiedad bool al ValoracionViewModel
                {
                    var nuevaVal = new Valoracion
                    {
                        Puntuacion = vm.Puntuacion,
                        Comentario = vm.Comentario,
                        IdUsuario = vm.Usuario,
                        PalabrasClave = vm.PalabrasClave
                    };

                    // 1. Guardar en BD
                    GestorBD.InsertarValoracion(ArticuloSeleccionado.Id.Value, nuevaVal);

                    // 2. Actualizar objeto local y UI
                    valorable.Valoraciones.Add(nuevaVal);
                    RefrescarLista();
                }
            }
        }
    }
    
    [RelayCommand]
    private async Task ExportarCSV()
    {
        var topLevel = sugerirTopLevel(); // Método auxiliar para obtener la ventana actual
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Guardar Biblioteca",
            FileTypeChoices = new[] { new FilePickerFileType("Archivos CSV") { Patterns = new[] { "*.csv" } } }
        });

        if (file != null)
        {
            ManejadorCSV.Exportar(file.Path.LocalPath, Articulos);
        }
    }

    [RelayCommand]
    private async Task ImportarCSV()
    {
        var topLevel = sugerirTopLevel();
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions       
        {
            Title = "Importar Biblioteca",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Archivos CSV") { Patterns = new[] { "*.csv" } } }
        });

        if (files.Count > 0)
        {
            var nuevos = ManejadorCSV.Importar(files[0].Path.LocalPath);
            foreach (var art in nuevos)
            {
                GestorBD.InsertarArticulo(art); // Los guardamos en la BD
            }
            RefrescarLista(); // Actualizamos la tabla
        }
    }

    // Auxiliar para que los diálogos de archivos funcionen en Avalonia
    private TopLevel? sugerirTopLevel()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return TopLevel.GetTopLevel(desktop.MainWindow);
        return null;
    }
}