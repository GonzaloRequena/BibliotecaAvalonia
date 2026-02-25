using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BibliotecaAvalonia.Models;

public interface Valorable
{
    // Una lista de valoraciones asociadas
    ObservableCollection<Valoracion> Valoraciones { get; set; }
    
    void AñadirValoracion(int puntuacion, string? comentario, string? palabrasClave, string idUsuario)
    {
        if (puntuacion < 0 || puntuacion > 10)
        {
            throw new ArgumentException("La puntuación debe ser un número entero entre 0 y 10.");
        }
        
        Valoraciones.Add(new Valoracion {
            Puntuacion = puntuacion,
            Comentario = comentario,
            PalabrasClave = palabrasClave,
            IdUsuario = idUsuario
        });
    }
    
    double CalcularPromedio()
    {
        if (Valoraciones == null || !Valoraciones.Any()) return 0;
        return Valoraciones.Average(v => v.Puntuacion);
    }
}