using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;

namespace BibliotecaAvalonia.Models;

public static class GestorBD
{
    private const string NombreBD = "biblioteca.db";

    public static SqliteConnection CrearConexion()
    {
        var conexion = new SqliteConnection($"Data Source={NombreBD}");
        conexion.Open();
        return conexion;
    }

    public static void InicializarBD()
    {
        using var conexion = CrearConexion();
        var comando = conexion.CreateCommand();
        comando.CommandText = @"
            CREATE TABLE IF NOT EXISTS Articulos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Titulo TEXT NOT NULL,
                Anio INTEGER NOT NULL,
                FechaAdquisicion TEXT NOT NULL,
                Tipo TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Libros (
                ArticuloId INTEGER PRIMARY KEY,
                Isbn10 TEXT NOT NULL,
                Disponible INTEGER DEFAULT 1,
                FOREIGN KEY(ArticuloId) REFERENCES Articulos(Id) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS Audiolibros (
                ArticuloId INTEGER PRIMARY KEY,
                InicioDisponibilidad TEXT NOT NULL,
                FinDisponibilidad TEXT NOT NULL,
                FOREIGN KEY(ArticuloId) REFERENCES Articulos(Id) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS Valoraciones (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ArticuloId INTEGER NOT NULL,
                Puntuacion INTEGER NOT NULL,
                Comentario TEXT,
                PalabrasClave TEXT,
                IdUsuario TEXT,
                FOREIGN KEY(ArticuloId) REFERENCES Articulos(Id) ON DELETE CASCADE
            );";
        comando.ExecuteNonQuery();
    }

    // --- OPERACIONES CRUD ---

    public static void InsertarArticulo(Articulo item)
    {
        using var conexion = CrearConexion();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            // 1. Insertar en la tabla base 'Articulos'
            var cmdBase = new SqliteCommand(@"
                INSERT INTO Articulos (Titulo, Anio, FechaAdquisicion, Tipo) 
                VALUES (@tit, @anio, @fecha, @tipo);
                SELECT last_insert_rowid();", conexion, transaccion);
            
            cmdBase.Parameters.AddWithValue("@tit", item.Titulo);
            cmdBase.Parameters.AddWithValue("@anio", item.Anio);
            cmdBase.Parameters.AddWithValue("@fecha", item.FechaAdquisicion.ToString("o")); // Formato ISO 8601 para fechas
            cmdBase.Parameters.AddWithValue("@tipo", item is Libro ? "Libro" : "Audiolibro");
            
            long nuevoId = (long)cmdBase.ExecuteScalar()!;

            // 2. Insertar en la tabla específica
            if (item is Libro libro)
            {
                var cmdLibro = new SqliteCommand(@"
                    INSERT INTO Libros (ArticuloId, Isbn10, Disponible) 
                    VALUES (@id, @isbn, @disp)", conexion, transaccion);
                cmdLibro.Parameters.AddWithValue("@id", nuevoId);
                cmdLibro.Parameters.AddWithValue("@isbn", libro.Isbn10);
                cmdLibro.Parameters.AddWithValue("@disp", libro.Disponible ? 1 : 0);
                cmdLibro.ExecuteNonQuery();
            }
            else if (item is Audiolibro audio)
            {
                var cmdAudio = new SqliteCommand(@"
                    INSERT INTO Audiolibros (ArticuloId, InicioDisponibilidad, FinDisponibilidad) 
                    VALUES (@id, @ini, @fin)", conexion, transaccion);
                cmdAudio.Parameters.AddWithValue("@id", nuevoId);
                cmdAudio.Parameters.AddWithValue("@ini", audio.InicioDisponibilidad.ToString("o"));
                cmdAudio.Parameters.AddWithValue("@fin", audio.FinDisponibilidad.ToString("o"));
                cmdAudio.ExecuteNonQuery();
            }

            item.Id = (int)nuevoId;
            transaccion.Commit();
        }
        catch { transaccion.Rollback(); throw; }
    }

    public static void EliminarArticulo(int id)
    {
        using var conexion = CrearConexion();
        var comando = new SqliteCommand("DELETE FROM Articulos WHERE Id = @id", conexion);
        comando.Parameters.AddWithValue("@id", id);
        comando.ExecuteNonQuery();
    }
    
    public static void ActualizarArticulo(int id, Articulo item)
    {
        using var conexion = CrearConexion();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            // 1. Actualizamos la tabla base
            var cmdBase = new SqliteCommand(@"
            UPDATE Articulos 
            SET Titulo = @tit, Anio = @anio, FechaAdquisicion = @fecha 
            WHERE Id = @id", conexion, transaccion);
        
            cmdBase.Parameters.AddWithValue("@tit", item.Titulo);
            cmdBase.Parameters.AddWithValue("@anio", item.Anio);
            cmdBase.Parameters.AddWithValue("@fecha", item.FechaAdquisicion.ToString("o"));
            cmdBase.Parameters.AddWithValue("@id", id);
            cmdBase.ExecuteNonQuery();

            // 2. Actualizamos la tabla específica según el tipo
            if (item is Libro libro)
            {
                var cmdLibro = new SqliteCommand(@"
                UPDATE Libros SET Isbn10 = @isbn, Disponible = @disp 
                WHERE ArticuloId = @id", conexion, transaccion);
                cmdLibro.Parameters.AddWithValue("@isbn", libro.Isbn10);
                cmdLibro.Parameters.AddWithValue("@disp", libro.Disponible ? 1 : 0);
                cmdLibro.Parameters.AddWithValue("@id", id);
                cmdLibro.ExecuteNonQuery();
            }
            else if (item is Audiolibro audio)
            {
                var cmdAudio = new SqliteCommand(@"
                UPDATE Audiolibros 
                SET InicioDisponibilidad = @ini, FinDisponibilidad = @fin 
                WHERE ArticuloId = @id", conexion, transaccion);
                cmdAudio.Parameters.AddWithValue("@ini", audio.InicioDisponibilidad.ToString("o"));
                cmdAudio.Parameters.AddWithValue("@fin", audio.FinDisponibilidad.ToString("o"));
                cmdAudio.Parameters.AddWithValue("@id", id);
                cmdAudio.ExecuteNonQuery();
            }

            transaccion.Commit();
        }
        catch { transaccion.Rollback(); throw; }
    }
    
    public static void ActualizarDisponibilidad(int articuloId, bool disponible)
    {
        using var conexion = CrearConexion();
        var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE Libros SET Disponible = @disponible WHERE ArticuloId = @id";
        comando.Parameters.AddWithValue("@disponible", disponible ? 1 : 0);
        comando.Parameters.AddWithValue("@id", articuloId);
        comando.ExecuteNonQuery();
    }

    public static void InsertarValoracion(int articuloId, Valoracion v)
    {
        using var conexion = CrearConexion();
        var comando = conexion.CreateCommand();
        comando.CommandText = @"INSERT INTO Valoraciones (ArticuloId, Puntuacion, Comentario, PalabrasClave, IdUsuario) 
                           VALUES (@artId, @punt, @com, @pal, @user)";
        comando.Parameters.AddWithValue("@artId", articuloId);
        comando.Parameters.AddWithValue("@punt", v.Puntuacion);
        comando.Parameters.AddWithValue("@com", (object)v.Comentario ?? DBNull.Value);
        comando.Parameters.AddWithValue("@pal", (object)v.PalabrasClave ?? DBNull.Value);
        comando.Parameters.AddWithValue("@user", (object)v.IdUsuario ?? DBNull.Value);
        comando.ExecuteNonQuery();
    }

    public static ObservableCollection<Valoracion> ObtenerValoraciones(int articuloId)
    {
        var lista = new ObservableCollection<Valoracion>();
        using var conexion = CrearConexion();
        var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT * FROM Valoraciones WHERE ArticuloId = @artId";
        comando.Parameters.AddWithValue("@artId", articuloId);

        using var reader = comando.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Valoracion {
                Puntuacion = reader.GetInt32(reader.GetOrdinal("Puntuacion")),
                Comentario = reader.IsDBNull(reader.GetOrdinal("Comentario")) ? null : reader.GetString(reader.GetOrdinal("Comentario")),
                PalabrasClave = reader.IsDBNull(reader.GetOrdinal("PalabrasClave")) ? null : reader.GetString(reader.GetOrdinal("PalabrasClave")),
                IdUsuario = reader.IsDBNull(reader.GetOrdinal("IdUsuario")) ? null : reader.GetString(reader.GetOrdinal("IdUsuario"))
            });
        }
        return lista;
    }
    
    // Permite buscar por múltiples títulos o filtros (autor, año, etc.)
    public static List<Articulo> BuscarItems(string texto = "", string tipo = "Todos")
    {
        var resultados = new List<Articulo>();
        using var conexion = CrearConexion();
        
        // 1. Consulta base con JOINs para traer datos de ambas tablas específicas
        string sql = @"
            SELECT a.*, l.Isbn10, l.Disponible, au.InicioDisponibilidad, au.FinDisponibilidad 
            FROM Articulos a
            LEFT JOIN Libros l ON a.Id = l.ArticuloId
            LEFT JOIN Audiolibros au ON a.Id = au.ArticuloId
            WHERE a.Titulo LIKE @texto";

        // 2. Filtro dinámico por tipo
        // Nota: "Libros" (plural) en el ViewModel vs "Libro" (singular) en BD
        if (tipo != "Todos")
        {
            sql += " AND a.Tipo = @tipo";
        }

        var comando = new SqliteCommand(sql, conexion);
        comando.Parameters.AddWithValue("@texto", $"%{texto}%");
        
        // Mapeo: Si el combo dice "Libros", buscamos "Libro" en la BD
        if (tipo != "Todos")
        {
            string tipoBD = tipo == "Libros" ? "Libro" : "Audiolibro";
            comando.Parameters.AddWithValue("@tipo", tipoBD);
        }

        using var reader = comando.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(reader.GetOrdinal("Id"));
            string tipoRow = reader.GetString(reader.GetOrdinal("Tipo"));
            string titulo = reader.GetString(reader.GetOrdinal("Titulo"));
            int anio = reader.GetInt32(reader.GetOrdinal("Anio"));
            DateTime fechaAdq = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaAdquisicion")));

            Articulo nuevoArticulo;
            if (tipoRow == "Libro")
            {
                nuevoArticulo = new Libro(titulo, anio, fechaAdq, reader.GetString(reader.GetOrdinal("Isbn10")))
                {
                    Disponible = reader.GetInt32(reader.GetOrdinal("Disponible")) == 1
                };
            }
            else
            {
                DateTime inicio = DateTime.Parse(reader.GetString(reader.GetOrdinal("InicioDisponibilidad")));
                DateTime fin = DateTime.Parse(reader.GetString(reader.GetOrdinal("FinDisponibilidad")));
                nuevoArticulo = new Audiolibro(titulo, anio, fechaAdq, inicio, fin);
            }
            
            nuevoArticulo.Id = id;
            
            if (nuevoArticulo is Valorable v)
            {
                v.Valoraciones = ObtenerValoraciones(id);
            }
            
            resultados.Add(nuevoArticulo);
        }
        return resultados;
    }
    
    // Método necesario para cargar la lista al iniciar la app
    public static List<Articulo> ObtenerTodos()
    {
        return BuscarItems(); // Reutilizamos el buscador sin filtros para traerlo todo
    }
}