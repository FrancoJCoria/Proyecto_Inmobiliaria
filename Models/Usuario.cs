using System.ComponentModel.DataAnnotations;
namespace Inmobiliaria.Models;
public class Usuario
{
    public int Id_usuario{ get; set; }

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La clave debe tener al menos 6 caracteres")]
    [MaxLength(12, ErrorMessage = "La clave debe contener un maximo de 12 caracteres")]
    public string Clave {get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre {get; set; }

    [Required(ErrorMessage = "El Apellido es obligatorio")]
    public string Apellido {get; set; }


    public string Avatar { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio")]
    public string Rol { get; set; }
    
    public bool Estado { get; set; }

    public override string ToString() => $"{Nombre} {Apellido} {Email} {Rol}";

}