using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace BibliotecaAvalonia.Models;

public abstract partial class Articulo : ObservableObject
{
    // ID es nullable para permitir que se asigne automáticamente al agregar a la base de datos
    // Necesario para operaciones CRUD
    [ObservableProperty]
    private int? _id;
    
    [ObservableProperty]
    private string _titulo;

    [ObservableProperty]
    private int _anio;

    [ObservableProperty]
    private DateTime _fechaAdquisicion;

    // Constructor cooperativo: se llama desde las clases hijas
    protected Articulo(string titulo, int anio, DateTime fechaAdquisicion)
    {
        _titulo = FormatearTitulo(titulo);
        
        int añoActual = DateTime.Now.Year;
        if (anio < 1500 || anio > añoActual)
        {
            throw new ArgumentException($"El año debe estar entre 1500 y {añoActual}.");
        }
        _anio = anio > DateTime.Now.Year ? DateTime.Now.Year : anio;
        
        _fechaAdquisicion = fechaAdquisicion;
    }
    
    public static string FormatearTitulo(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
        texto = texto.Trim();
        return char.ToUpper(texto[0]) + texto.Substring(1).ToLower();
    }
    
    public string Tipo => this is Libro ? "Libro" : "Audiolibro";
    // Propiedad virtual para que las clases hijas puedan proporcionar su propia implementación de la nota media, si es que tienen valoraciones
    public virtual double NotaMedia => 0;
}