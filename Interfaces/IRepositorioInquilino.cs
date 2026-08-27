namespace Inmobiliaria.Models;

public interface IRepositorioInquilino
{
    int Alta(Inquilino i);

    int Baja(Inquilino i);

    int Modificacion(Inquilino i);

    IList<Inquilino> ObtenerTodos();
}