namespace Inmobiliaria.Models;

public interface IRepositorioInquilino
{
    int Alta(Inquilino i);

    int Baja(int id);

    int Modificacion(Inquilino i);

    IList<Inquilino> ObtenerTodos(int pagina = 1, int tamanoPagina = 5);
    Inquilino? ObtenerPorId(int id);
}