namespace Inmobiliaria.Models;

public interface IRepositorioInmueble
{
    int Alta(Inmueble i);

    int Baja(int id);

    int Modificacion(Inmueble i);

    int ModificarPortada(int id, string url);

    IList<Inmueble> ObtenerTodos(int pagina = 1, int tamanoPagina = 5, int? idPropietario = null, bool? disponible = null);

    Inmueble? ObtenerPorId(int id);

    IList<Inmueble> BuscarDisponiblesPorFechas(DateTime fechaInicio, DateTime fechaFin);
    
    IList<Inmueble> ObtenerMasReservados();
    IList<Inmueble> ObtenerMenosReservados(int cantidad);
}
