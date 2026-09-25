namespace Inmobiliaria.Models;

public interface IRepositorioInmueble
{
    int Alta(Inmueble i);

    int Baja(int id);

    int Modificacion(Inmueble i);

    int ModificarPortada(int id, string url);

    // Filtro en el servidor para los desplegables: trae los inmuebles cuya direccion
    // coincide con "busqueda" y como mucho "limite" filas. La versión paginada NO lleva
    // defaults, así "ObtenerTodos()" sin argumentos cae inequívocamente en esta.
    IList<Inmueble> ObtenerTodos(string? busqueda = null, int limite = 20);

    // Listado paginado de la pagina de Inmuebles, con los filtros de listado.
    IList<Inmueble> ObtenerTodos(int pagina, int tamanoPagina, int? idPropietario = null, bool? disponible = null, bool? estado = null);

    Inmueble? ObtenerPorId(int id);

    IList<Inmueble> BuscarDisponiblesPorFechas(DateTime fechaInicio, DateTime fechaFin, int pagina, int tamanoPagina);
    
    IList<Inmueble> ObtenerMasReservados(int pagina, int tamanoPagina);
    IList<Inmueble> ObtenerMenosReservados(int cantidad, int pagina, int tamanoPagina);

    int Contar(int? idPropietario = null, bool? disponible = null, bool? estado = null);

    int ContarMasReservados();

    int ContarMenosReservados(int cantidad);

    int ContarDisponiblesPorFechas(DateTime fechaInicio, DateTime fechaFin);
}
