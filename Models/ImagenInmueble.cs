namespace Inmobiliaria.Models;

public class ImagenInmueble
{
    public int Id { get; set; }
    public int InmuebleId { get; set; }
    public string Url { get; set; } = "";
    public bool Estado { get; set; } = true;
    public IFormFile? Archivo { get; set; } = null;

    public override string ToString() => $"Imagen {Id} del inmueble {InmuebleId}: {Url}";
}
