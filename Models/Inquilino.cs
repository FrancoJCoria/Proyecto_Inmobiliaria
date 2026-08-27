namespace Inmobiliaria.Models;
using System.ComponentModel.DataAnnotations;
public class Inquilino
{
    public int Id_inquilino { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(8, ErrorMessage = "El DNI debe tener 8 caracteres")]
    public string Dni { get; set; } = ""; 

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Estado { get; set; }

    public override string ToString()
    {
        return $"(Dni: {Dni}), Nombre: {Nombre}, Apellido: {Apellido}";
    }
}