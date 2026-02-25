using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BibliotecaAvalonia.Models;

public partial class Libro : Articulo, Prestable, Valorable
{
    [ObservableProperty]
    private string _isbn10;
    
    public bool Disponible { get; set; } = true;
    public ObservableCollection<Valoracion> Valoraciones { get; set; }
    
    // Constructor que coopera con la base Articulo
    public Libro(string titulo, int anio, DateTime fechaAdquisicion, string isbn10) 
        : base(titulo, anio, fechaAdquisicion)
    {
        if (!ValidarIsbn(isbn10))
        {
            throw new ArgumentException("El ISBN-10 introducido no es válido (comprueba los dígitos y el código de control).");
        }
        _isbn10 = isbn10;
    }

    public void Prestar() => Disponible = false;
    public void Devolver() => Disponible = true;

    public static bool ValidarIsbn(string isbn)
    {
        if (string.IsNullOrEmpty(isbn)) return false;
        isbn = isbn.Trim();
        
        // 1. Longitud obligatoria de 10
        if (string.IsNullOrEmpty(isbn) || isbn.Length != 10) return false;
        
        int suma = 0;

        for (int i = 0; i < 10; i++)
        {
            int valorDigito;

            // Si es el último dígito y es una 'X', vale 10
            if (i == 9 && (isbn[i] == 'X' || isbn[i] == 'x'))
            {
                valorDigito = 10;
            }
            else if (char.IsDigit(isbn[i]))
            {
                valorDigito = isbn[i] - '0';
            }
            else
            {
                // Si hay una letra que no sea 'X' al final, no es válido
                return false;
            }

            // Multiplicamos por 10, 9, 8... hasta 1
            suma += valorDigito * (10 - i);
        }

        // 2. Comprobar si es múltiplo de 11
        return suma % 11 == 0;
    }

    public override double NotaMedia => ((Valorable)this).CalcularPromedio();}