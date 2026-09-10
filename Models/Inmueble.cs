using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models;

public class Inmueble
{
    public int Id_inmueble { get; set; }

    [Required(ErrorMessage = "La direccion es obligatoria")]
    public string Direccion { get; set; } = "";

    [Range(1, 100, ErrorMessage = "El cupo debe ser entre 1 y 100 personas")]
    public int Cupo { get; set; }

    [Range(1, 99999999, ErrorMessage = "El precio por dia debe ser mayor a cero")]
    public decimal Precio_dia { get; set; }

    [Range(0, 100, ErrorMessage = "El porcentaje de reserva debe ser entre 0 y 100")]
    public decimal Porcentaje_reserva { get; set; }

    public bool Disponible { get; set; }

    public string? Portada { get; set; }

    public IFormFile? PortadaFile { get; set; }

    public IList<ImagenInmueble> Imagenes { get; set; } = new List<ImagenInmueble>();

    [Range(1, int.MaxValue, ErrorMessage = "Hay que elegir un propietario")]
    public int Id_propietario { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hay que elegir un tipo de inmueble")]
    public int Id_tipo { get; set; }

    public string Estado { get; set; } = "Activo";

    public override string ToString() => $"{Direccion} (cupo {Cupo})";
}
