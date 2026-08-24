using MySqlConnector;
namespace Inmobiliaria.Models;

public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
{
    public RepositorioUsuario(IConfiguration configuration) : base(configuration){}

    public int Alta(Usuario usuarioParams)
    {
        try
        {
            int idGenerado = 0;
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"INSERT INTO Usuario ( email, clave, nombre, apellido, avatar, rol, estado)
            VALUES (@email, @clave, @nombre, @apellido, @avatar, @rol, @estado);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@email", usuarioParams.Email);
            comando.Parameters.AddWithValue("@clave", usuarioParams.Clave);
            comando.Parameters.AddWithValue("@nombre", usuarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", usuarioParams.Apellido);
            comando.Parameters.AddWithValue("@avatar", (object)usuarioParams.Avatar ?? DBNull.Value);
            comando.Parameters.AddWithValue("@rol", usuarioParams.Rol);
            comando.Parameters.AddWithValue("@estado", usuarioParams.Estado);
            
            conexion.Open();
            idGenerado = Convert.ToInt32(comando.ExecuteScalar());
            usuarioParams.Id_usuario = idGenerado;

            return idGenerado;
        }catch(Exception e)
        {
            Console.WriteLine( "Fallo en Alta Usuario Models", e); 
            throw;
        }
    }

    public int Baja (Usuario usuarioParams)
    {
        try
        {
            int filaAfectada = 0;  
            using var conexion = new MySqlConnection(connectionString);

            string consultaSql = @"UPDATE Usuario
            SET estado = @estado 
            WHERE id_usuario = @id_usuario;;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue(@"estado", usuarioParams.Estado);
            comando.Parameters.AddWithValue(@"id_usuario", usuarioParams.Id_usuario);

            conexion.Open();
            filaAfectada = comando.ExecuteNonQuery();

            return filaAfectada;
        }catch(Exception e)
        {
            Console.WriteLine($"Error usuarioBajaModels: {e.Message}");
            throw;
        }
       
    }

    public int Modificacion(Usuario usuarioParams)
    {
        try
        {
            int filaAfectada = 0;
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"UPDATE Usuario (email, clave, nombre, apellido, avatar, rol) 
            VALUES (@email, @clave, @nombre, @apellido, @avatar, @rol, @estado) 
            WHERE id_usuario = @id_usuario AND estado = 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@email", usuarioParams.Email);
            comando.Parameters.AddWithValue("@clave", usuarioParams.Clave);
            comando.Parameters.AddWithValue("@nombre", usuarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", usuarioParams.Apellido);
            comando.Parameters.AddWithValue("@avatar", usuarioParams.Avatar);
            comando.Parameters.AddWithValue("@rol", usuarioParams.Rol);
            comando.Parameters.AddWithValue("@id_usuario", usuarioParams.Id_usuario);
            
            conexion.Open();
            filaAfectada = comando.ExecuteNonQuery();

            return filaAfectada;
        }catch(Exception e)
        {
            Console.WriteLine($"Error modificacionUsuarioController: {e}");
            throw;
        }
    }
}