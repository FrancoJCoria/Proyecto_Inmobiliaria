namespace Inmobiliaria.Models;

/// <summary>
/// Opción que ya estaba elegida en un desplegable. Las vistas la reciben por ViewBag para
/// dejar marcada la selección vigente sin cargar el catálogo completo.
/// </summary>
public class OpcionDesplegable
{
    public int Id { get; set; }

    public string Etiqueta { get; set; } = "";

    public override string ToString() => Etiqueta;
}
