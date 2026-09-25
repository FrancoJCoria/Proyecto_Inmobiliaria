namespace Inmobiliaria.Models;

public interface IRepositorioInmueble
{
    int Alta(Inmueble i);

    int Baja(int id);

    int Modificacion(Inmueble i);

    int ModificarPortada(int id, string url);

    // Lista completa, sin paginar, para los desplegables. Con "busqueda" filtra en el
    // servidor por dirección. La versión paginada NO lleva defaults, así
    // "ObtenerTodos()" sin argumentos cae inequívocamente en esta.
    IList<Inmueble> ObtenerTodos(string? busqueda = null);

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
