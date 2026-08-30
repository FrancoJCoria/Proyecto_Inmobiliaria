namespace Inmobiliaria.Models;

public interface IRepositorioReserva
{
    int Alta(Reserva r);
    int Baja(Reserva r);

    int Modificacion(Reserva r);
}