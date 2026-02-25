using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BibliotecaAvalonia.Models;

public partial class Audiolibro : Articulo, Valorable
{
    [ObservableProperty]
    private DateTime _inicioDisponibilidad;

    [ObservableProperty]
    private DateTime _finDisponibilidad;

    public ObservableCollection<Valoracion> Valoraciones { get; set; }
    
    public Audiolibro(string titulo, int anio, DateTime fechaAdquisicion, DateTime inicio, DateTime fin)
        : base(titulo, anio, fechaAdquisicion)
    {
        _inicioDisponibilidad = inicio;
        _finDisponibilidad = fin;
    }

    // Comprobamos si el audiolibro está disponible actualmente según las fechas de disponibilidad
    public bool EstaDisponibleActualmente => 
        DateTime.Now >= InicioDisponibilidad && DateTime.Now <= FinDisponibilidad;
    
    public override double NotaMedia => ((Valorable)this).CalcularPromedio();}