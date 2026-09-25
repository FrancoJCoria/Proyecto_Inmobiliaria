namespace Inmobiliaria.Models;

public interface IRepositorioTipoInmueble
{
    int Alta(TipoInmueble t);

    int Baja(int id);

    int Modificacion(TipoInmueble t);

    // Lista completa para los desplegables, con filtro opcional en el servidor por nombre.
    IList<TipoInmueble> ObtenerTodos(string? busqueda = null);

    TipoInmueble? ObtenerPorId(int id);
}
