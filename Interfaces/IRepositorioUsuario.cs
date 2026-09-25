namespace Inmobiliaria.Models;
public interface IRepositorioUsuario 
{
    int Alta (Usuario u);
    int Baja (Usuario u);

    int Modificacion (Usuario u);
    int ActualizarClave(int id, string clave);
    Usuario? ObtenerPorId(int id);
    Usuario? ObetenerPorEmail(string email);

    // Lista completa, sin paginar, para resolver nombres de usuario. Con "busqueda"
    // filtra en el servidor por nombre, apellido o email. La versión paginada NO lleva
    // defaults, así "ObtenerTodos()" sin argumentos cae inequívocamente en esta.
    IList<Usuario> ObtenerTodos(string? busqueda = null);
    IList<Usuario> ObtenerTodos(int pagina, int tamanoPagina);
    int Contar();

}