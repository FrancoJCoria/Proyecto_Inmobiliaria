//using MySql.Data.MySqlConnector;
//En la interfaz de cada repositorio van los metodos que vayamos a utilizar, el trabajo pide alta, modificacion y baja para primera entrega.
//Basicamente es para saber que metodos tenemos, es como un menu de restaurante, te da los nombres de los metodos.
namespace Inmobiliaria.Models;

public interface IRepositorioPropietario
{
    int Alta(Propietario p);
    int Baja(Propietario p);
    int Modificacion(Propietario p);
    // Lista completa, sin paginar, para los desplegables. Con "busqueda" filtra en el
    // servidor por apellido, nombre o dni. La versión paginada NO lleva defaults, así
    // "ObtenerTodos()" sin argumentos cae inequívocamente en esta.
    IList<Propietario> ObtenerTodos(string? busqueda = null);
    IList<Propietario> ObtenerTodos(int pagina, int tamanoPagina);
    int Contar();
    Propietario? ObtenerPorId(int id);
}