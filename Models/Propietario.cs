using System.ComponentModel.DataAnnotations;
//descomentar para ORM
//using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria.Models;

//[Table("Propietario")]
public class Propietario{
    
    //[Key]
    //[Column("id_propietario")]
    public int Id_propietario { get; set; }

    //[Column("dni")]
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(8, ErrorMessage = "El DNI debe tener 8 caracteres")]
    public string Dni { get; set; } = string.Empty;

    //[Column("nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    //[Column("apellido")]
    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; } = string.Empty;

    //[Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    //[Column("email")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    public string Email { get; set; } = string.Empty;

    //[Column("estado")]
    public bool Estado { get; set; }

    public override string ToString() => $"{Nombre} {Apellido} {Dni}";
}