namespace Inmobiliaria.Models;
public interface IRepositorioUsuario 
{
    int Alta (Usuario u);
    int Baja (Usuario u);

    int Modificacion (Usuario u);
    int ActualizarClave(int id, string clave);
    Usuario? ObtenerPorId(int id);
    Usuario? ObetenerPorEmail(string email);

    IList<Usuario> ObtenerTodos(int pagina = 1, int tamanoPagina = 5);

}