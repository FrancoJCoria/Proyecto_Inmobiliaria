namespace Inmobiliaria.Models;

public interface IRepositorioPago
{
    int Alta(Pago p);
    int Modificacion(Pago p);
    int Anular(Pago p);
    IList<Pago> ObtenerPorReserva(int idReserva);
    Pago? ObtenerPorId(int id);
}