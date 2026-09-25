namespace Inmobiliaria.Models;

public interface IRepositorioTipoInmueble
{
    int Alta(TipoInmueble t);

    int Baja(int id);

    int Modificacion(TipoInmueble t);

    // Filtro en el servidor para el desplegable de tipos: trae los que coinciden con
    // "busqueda" por nombre y como mucho "limite" filas.
    IList<TipoInmueble> ObtenerTodos(string? busqueda = null, int limite = 20);

    TipoInmueble? ObtenerPorId(int id);
}
