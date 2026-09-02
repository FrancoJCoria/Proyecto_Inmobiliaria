namespace Inmobiliaria.Models;

public interface IRepositorioTipoInmueble
{
    int Alta(TipoInmueble t);

    int Baja(int id);

    int Modificacion(TipoInmueble t);

    IList<TipoInmueble> ObtenerTodos();

    TipoInmueble? ObtenerPorId(int id);
}
