using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models;

public class TipoInmueble
{
    public int Id_tipo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = "";

    public override string ToString() => Nombre;
}
