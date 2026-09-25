namespace Inmobiliaria.Models;

public interface IRepositorioInquilino
{
    int Alta(Inquilino i);

    int Baja(int id);

    int Modificacion(Inquilino i);

    // Filtro en el servidor para los desplegables: trae los inquilinos que coinciden
    // con "busqueda" (por nombre, apellido o dni) y como mucho "limite" filas. Ojo: la
    // versión paginada NO lleva defaults, así "ObtenerTodos()" sin argumentos cae
    // inequívocamente en esta y no en la otra.
    IList<Inquilino> ObtenerTodos(string? busqueda = null, int limite = 20);
    // Listado paginado de la pagina de Inquilinos, sin filtro de texto.
    IList<Inquilino> ObtenerTodos(int pagina, int tamanoPagina);
    int Contar();
    Inquilino? ObtenerPorId(int id);
}