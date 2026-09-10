namespace Inmobiliaria.Models;

public interface IRepositorioInmueble
{
    int Alta(Inmueble i);

    int Baja(int id);

    int Modificacion(Inmueble i);

    int ModificarPortada(int id, string url);

    IList<Inmueble> ObtenerTodos();

    Inmueble? ObtenerPorId(int id);
}
