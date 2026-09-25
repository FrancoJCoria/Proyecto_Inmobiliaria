namespace Inmobiliaria.Models;

public interface IRepositorioPago
{
    int Alta(Pago p);
    int Modificacion(Pago p);
    int Anular(Pago p);
    IList<Pago> ObtenerPorReserva(int idReserva, int pagina, int tamanoPagina);
    Pago? ObtenerPorId(int id);

    int ContarPorReserva(int idReserva);

    decimal TotalPagadoActivo(int idReserva);
}