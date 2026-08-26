using MySqlConnector;
using System.Data;

namespace Inmobiliaria.Models;

public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
{
    // Constructor para pasarle la config al base y que agarre el connectionString
    public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

    //------------------------------------------------------------------------------------------CREAR EL USUARIO------------------------------------------//
    public int Alta(Usuario usuarioParams)
    {
        try
        {
            // Variable para guardar el id que nos da la base de datos y retornarlo al controller
            int idGenerado = 0;

            // Preparamos la conexion a la bd
            using var conexion = new MySqlConnection(connectionString);

            // Consulta sql: inserta los datos y con LAST_INSERT_ID() agarramos el id nuevo
            string consultaSql = @"INSERT INTO Usuario (email, clave, nombre, apellido, avatar, rol, estado)
            VALUES (@email, @clave, @nombre, @apellido, @avatar, @rol, @estado);
            SELECT LAST_INSERT_ID();";

            // Armamos el comando con la consulta y la conexion
            using var comando = new MySqlCommand(consultaSql, conexion);

            // Pasamos los parametros para que no nos metan inyeccion sql
            comando.Parameters.AddWithValue("@email", usuarioParams.Email);
            comando.Parameters.AddWithValue("@clave", usuarioParams.Clave);
            comando.Parameters.AddWithValue("@nombre", usuarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", usuarioParams.Apellido);
            
            // Si no mandaron foto de avatar, mandamos DBNull para que la base de datos no chille
            comando.Parameters.AddWithValue("@avatar", (object?)usuarioParams.Avatar ?? DBNull.Value);
            comando.Parameters.AddWithValue("@rol", usuarioParams.Rol);
            comando.Parameters.AddWithValue("@estado", usuarioParams.Estado);

            // Abrimos la conexion a la base de datos
            conexion.Open();

            // ExecuteScalar devuelve el primer valor que encuentra (el id nuevo) y lo pasamos a int
            idGenerado = Convert.ToInt32(comando.ExecuteScalar());

            // Le clavamos el id nuevo al objeto que nos pasaron
            usuarioParams.Id_usuario = idGenerado;

            // Devolvemos el id generado
            return idGenerado;
        }
        catch (Exception e)
        {
            // Mostramos el error por consola para saber que paso y lo tiramos para arriba
            Console.WriteLine($"Fallo en Alta Usuario Models: {e.Message}");
            throw;
        }
    }

    //------------------------------------------------------------------------------------------BAJA LOGICA DE USUARIO------------------------------------------//
    public int Baja(Usuario usuarioParams)
    {
        try
        {
            //Variable para ver cuantas filas se tocaron en la base de datos
            int filaAfectada = 0;

            //Preparamos la conexion a la base de datos
            using var conexion = new MySqlConnection(connectionString);

            //Solo cambiamos el estado a 0 (baja logica) buscando por el id
            string consultaSql = @"UPDATE Usuario
            SET estado = @estado 
            WHERE id_usuario = @id_usuario;";

            using var comando = new MySqlCommand(consultaSql, conexion);

            // Asignamos los parametros
            comando.Parameters.AddWithValue("@estado", usuarioParams.Estado);
            comando.Parameters.AddWithValue("@id_usuario", usuarioParams.Id_usuario);

            // Abrimos la conexion
            conexion.Open();

            // ExecuteNonQuery sirve para UPDATE/DELETE/INSERT y te dice cuantas filas modifico
            filaAfectada = comando.ExecuteNonQuery();

            // Retornamos si modifico algo (1 si salio bien, 0 si no encontro el id)
            return filaAfectada;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error usuarioBajaModels: {e.Message}");
            throw;
        }
    }

    //------------------------------------------------------------------------------------------MODIFICAR EL USUARIO------------------------------------------//
    public int Modificacion(Usuario usuarioParams)
    {
        try
        {
            // Variable para saber si se llego a actualizar la fila
            int filaAfectada = 0;

            //Preparamos la conexion a la base de datos
            using var conexion = new MySqlConnection(connectionString);

            //Modificamos los datos del usuario siempre y cuando este activo (estado = 1)
            string consultaSql = @"UPDATE Usuario 
            SET email = @email, clave = @clave, nombre = @nombre ,apellido = @apellido, avatar = @avatar, rol = @rol
            WHERE id_usuario = @id_usuario AND estado = 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);

            //Pasamos todos los datos a modificar
            comando.Parameters.AddWithValue("@email", usuarioParams.Email);
            comando.Parameters.AddWithValue("@clave", usuarioParams.Clave);
            comando.Parameters.AddWithValue("@nombre", usuarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", usuarioParams.Apellido);
            comando.Parameters.AddWithValue("@avatar", (object?)usuarioParams.Avatar ?? DBNull.Value);
            comando.Parameters.AddWithValue("@rol", usuarioParams.Rol);
            comando.Parameters.AddWithValue("@id_usuario", usuarioParams.Id_usuario);

            conexion.Open();

            //Ejecutamos el update
            filaAfectada = comando.ExecuteNonQuery();

            return filaAfectada;
        }
        catch (Exception e)
        {

            Console.WriteLine($"Error modificacionUsuarioModels: {e.Message}");
            throw;
        }
    }

    //------------------------------------------------------------------------------------------OBTENER USUARIO POR ID------------------------------------------//
    public Usuario? ObtenerPorId(int id)
    {
        // Arrancamos con user en null por si no lo encuentra en la base
        Usuario? user = null;

        using var conexion = new MySqlConnection(connectionString);

        // Traemos todos los datos del usuario buscando por su id
        string consultaSql = @"SELECT id_usuario, email, clave, nombre, apellido, avatar, rol, estado
        FROM Usuario
        WHERE id_usuario = @id;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id", id);

        conexion.Open();

        // Con ExecuteReader leemos los registros que devuelve el SELECT
        using var reader = comando.ExecuteReader();

        // Read() se fija si vino alguna fila
        if (reader.Read())
        {
            // Armamos el objeto usuario con lo que trajo la base de datos
            user = new Usuario
            {
                Id_usuario = reader.GetInt32("id_usuario"),
                Email = reader.GetString("email"),
                Clave = reader.GetString("clave"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                // Si el avatar en la base es null ponemos null en c#, sino rompe con GetString
                Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? null : reader.GetString("avatar"),
                Rol = reader.GetString("rol"),
                Estado = reader.GetBoolean("estado")
            };
        }

        // Devolvemos el usuario cargado o null si no existia
        return user;
    }

    //------------------------------------------------------------------------------------------OBTENER USUARIO POR EMAIL------------------------------------------//
    public Usuario? ObetenerPorEmail(string email)
    {
        Usuario? user = null;

        using var conexion = new MySqlConnection(connectionString);

        // Buscamos al usuario por su correo
        string consultaSql = @"SELECT id_usuario, email, clave, nombre, apellido, avatar, rol, estado
        FROM Usuario
        WHERE email = @email;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@email", email);

        conexion.Open();

        using var reader = comando.ExecuteReader();

        if (reader.Read())
        {
            user = new Usuario
            {
                Id_usuario = reader.GetInt32("id_usuario"),
                Email = reader.GetString("email"),
                Clave = reader.GetString("clave"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? null : reader.GetString("avatar"),
                Rol = reader.GetString("rol"),
                Estado = reader.GetBoolean("estado")
            };
        }

        return user;
    }
}