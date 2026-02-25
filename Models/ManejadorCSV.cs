using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BibliotecaAvalonia.Models;

public static class ManejadorCSV
{
    // Exporta la lista actual a un archivo
    public static void Exportar(string ruta, IEnumerable<Articulo> articulos)
    {
        var sb = new StringBuilder();
        // Encabezado
        sb.AppendLine("Tipo;Titulo;Anio;FechaAdquisicion;InfoExtra");

        foreach (var art in articulos)
        {
            // La InfoExtra depende de si es Libro (ISBN) o Audiolibro (Fechas)
            string infoExtra = art is Libro l ? l.Isbn10 : (art is Audiolibro a ? $"{a.InicioDisponibilidad:o}|{a.FinDisponibilidad:o}" : "");
            
            sb.AppendLine($"{art.GetType().Name};{art.Titulo};{art.Anio};{art.FechaAdquisicion:o};{infoExtra}");
        }
        File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);
    }

    // Lee un archivo y devuelve una lista de objetos
    public static List<Articulo> Importar(string ruta)
    {
        var lista = new List<Articulo>();
        var lineas = File.ReadAllLines(ruta);

        // Saltamos la cabecera (i=1)
        for (int i = 1; i < lineas.Length; i++)
        {
            var datos = lineas[i].Split(';');
            if (datos.Length < 5) continue;

            string tipo = datos[0];
            string titulo = datos[1];
            int anio = int.Parse(datos[2]);
            DateTime fechaAdq = DateTime.Parse(datos[3]);
            string infoExtra = datos[4];

            if (tipo == "Libro")
            {
                lista.Add(new Libro(titulo, anio, fechaAdq, infoExtra));
            }
            else if (tipo == "Audiolibro")
            {
                var fechas = infoExtra.Split('|');
                lista.Add(new Audiolibro(titulo, anio, fechaAdq, DateTime.Parse(fechas[0]), DateTime.Parse(fechas[1])));
            }
        }
        return lista;
    }
}