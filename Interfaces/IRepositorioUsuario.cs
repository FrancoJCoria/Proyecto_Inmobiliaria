namespace Inmobiliaria.Models;
public interface IRepositorioUsuario 
{
    int Alta (Usuario u);
    int Baja (Usuario u);

    int Modificacion (Usuario u);
    Usuario? ObtenerPorId(int id);
    Usuario? ObetenerPorEmail(string email);

    IList<Usuario> ObtenerTodos();

}