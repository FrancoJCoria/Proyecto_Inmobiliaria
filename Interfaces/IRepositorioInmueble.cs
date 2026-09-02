namespace Inmobiliaria.Models;

public interface IRepositorioInmueble
{
    int Alta(Inmueble i);

    int Baja(int id);

    int Modificacion(Inmueble i);

    IList<Inmueble> ObtenerTodos();

    Inmueble? ObtenerPorId(int id);
}
