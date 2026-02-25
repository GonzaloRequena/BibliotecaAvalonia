namespace BibliotecaAvalonia.Models;

public interface Prestable
{
    public static int DiasMaximosPrestamo = 31;
    bool Disponible { get; set; }
    void Prestar();
    void Devolver();
}