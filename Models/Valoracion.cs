using CommunityToolkit.Mvvm.ComponentModel;

namespace BibliotecaAvalonia.Models;

public partial class Valoracion : ObservableObject
{
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public string? IdUsuario { get; set; }
    
    [ObservableProperty]
    private string? _palabrasClave;
}