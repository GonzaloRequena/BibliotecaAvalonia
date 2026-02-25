using System;
using BibliotecaAvalonia.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BibliotecaAvalonia.ViewModels;

public partial class EditorItemViewModel : ViewModelBase
{
    // Propiedades comunes
    [ObservableProperty] private string _titulo = string.Empty;
    [ObservableProperty] private int _anio = DateTime.Now.Year;
    [ObservableProperty] private DateTimeOffset _fechaAdquisicion = DateTimeOffset.Now;

    // Propiedades específicas de Libro
    [ObservableProperty] private string _isbn10 = string.Empty;
    [ObservableProperty] private bool _estaDisponible = true;

    // Propiedades específicas de Audiolibro
    [ObservableProperty] private DateTimeOffset _inicioDisponibilidad = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset _finDisponibilidad = DateTimeOffset.Now.AddDays(30);

    // Control de estado
    [ObservableProperty] private bool _esLibro;
    [ObservableProperty] private string _tituloVentana;
    
    // Propiedad para que la vista sepa cómo cerrarse
    public Action? CloseAction { get; set; }
    [ObservableProperty] private string? _mensajeError;
    
    private readonly Articulo? _articuloOriginal;

    // Constructor para NUEVO artículo
    public EditorItemViewModel(bool esLibro)
    {
        EsLibro = esLibro;
        _articuloOriginal = null;
        TituloVentana = esLibro ? "Añadir nuevo libro" : "Añadir nuevo audiolibro";
    }

    // Constructor para EDITAR artículo existente
    public EditorItemViewModel(Articulo articulo)
    {
        _articuloOriginal = articulo;
        EsLibro = articulo is Libro;
        TituloVentana = $"Editando: {articulo.Titulo}";

        // Cargamos los datos existentes
        Titulo = articulo.Titulo;
        Anio = articulo.Anio;
        FechaAdquisicion = new DateTimeOffset(articulo.FechaAdquisicion);

        if (articulo is Libro libro)
        {
            Isbn10 = libro.Isbn10;
            EstaDisponible = libro.Disponible;
        }
        else if (articulo is Audiolibro audio)
        {
            InicioDisponibilidad = new DateTimeOffset(audio.InicioDisponibilidad);
            FinDisponibilidad = new DateTimeOffset(audio.FinDisponibilidad);
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        // Limpiamos errores previos
        MensajeError = null;

        // 1. Validación manual antes de intentar crear el objeto
        if (string.IsNullOrWhiteSpace(Titulo))
        {
            MensajeError = "El título es obligatorio.";
            return;
        }

        Articulo resultado;
        DateTime fechaAdq = FechaAdquisicion.DateTime;

        try 
        {
            if (EsLibro)
            {
                // El constructor de Libro lanzará ArgumentException si el ISBN es malo
                var libro = new Libro(Titulo, Anio, fechaAdq, Isbn10);
                libro.Disponible = EstaDisponible;
                resultado = libro;
            }
            else
            {
                // Validación para audiolibros
                if (FinDisponibilidad < InicioDisponibilidad)
                {
                    MensajeError = "La fecha de fin no puede ser anterior al inicio.";
                    return;
                }
                resultado = new Audiolibro(Titulo, Anio, fechaAdq, 
                    InicioDisponibilidad.DateTime, FinDisponibilidad.DateTime);
            }

            // 3. Persistir en Base de Datos
            if (_articuloOriginal == null)
            {
                GestorBD.InsertarArticulo(resultado);
            }
            else
            {
                resultado.Id = _articuloOriginal.Id;
                GestorBD.ActualizarArticulo(resultado.Id.Value, resultado);
            }

            CloseAction?.Invoke();
        }
        catch (ArgumentException ex)
        {
            // Capturamos el mensaje exacto que definimos en la clase Libro
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "Ocurrió un error inesperado al guardar.";
        }
    }
}