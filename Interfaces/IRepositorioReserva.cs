namespace Inmobiliaria.Models;

public interface IRepositorioReserva
{
    int Alta(Reserva r);
    int Baja(Reserva r);

    int Modificacion(Reserva r);

    IList<Reserva> ObtenerTodos();

    Reserva? ObtenerPorId(int id);

    IList<Reserva> ObtenerVigentes(DateTime desde, DateTime hasta);

    IList<Reserva> ObtenerPorTerminar(int dias);
}