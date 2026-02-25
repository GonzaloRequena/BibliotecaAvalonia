using System;
using BibliotecaAvalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ValoracionViewModel : ViewModelBase
{
    [ObservableProperty] private int _puntuacion = 5;
    [ObservableProperty] private string _comentario = "";
    [ObservableProperty] private string _usuario = "Anonimo";
    [ObservableProperty] private string _palabrasClave = "";
    
    public Action? CloseAction { get; set; }
    public bool OperacionConfirmada { get; set; } = false;

    [RelayCommand]
    private void Guardar()
    {
        if (Puntuacion < 0 || Puntuacion > 10) return;
        OperacionConfirmada = true;
        CloseAction?.Invoke();
    }
}