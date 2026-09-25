namespace Inmobiliaria.Models;

public interface IRepositorioReserva
{
    int Alta(Reserva r);
    int Baja(Reserva r);

    int Modificacion(Reserva r);

    IList<Reserva> ObtenerTodos(int pagina, int tamanoPagina);

    Reserva? ObtenerPorId(int id);

    IList<Reserva> ObtenerVigentes(DateTime desde, DateTime hasta, int pagina, int tamanoPagina);

    IList<Reserva> ObtenerPorTerminar(int dias, int pagina, int tamanoPagina);

    int Contar();

    int ContarVigentes(DateTime desde, DateTime hasta);

    int ContarPorTerminar(int dias);
}