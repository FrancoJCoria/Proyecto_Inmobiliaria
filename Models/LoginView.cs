using System.ComponentModel.DataAnnotations;
namespace Inmobiliaria.Models;

public class LoginView
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Clave { get; set; } = string.Empty;
}