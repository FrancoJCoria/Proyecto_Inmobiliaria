namespace Inmobiliaria.Models;

public interface IRepositorioInquilino
{
    int Alta(Inquilino i);

    int Baja(int id);

    int Modificacion(Inquilino i);

    // Lista completa, sin paginar, para los desplegables. Con "busqueda" filtra en el
    // servidor por nombre, apellido o dni. Ojo: la versión paginada NO lleva defaults,
    // así "ObtenerTodos()" sin argumentos ca inequívocamente en esta y no en la otra.
    IList<Inquilino> ObtenerTodos(string? busqueda = null);
    IList<Inquilino> ObtenerTodos(int pagina, int tamanoPagina);
    int Contar();
    Inquilino? ObtenerPorId(int id);
}