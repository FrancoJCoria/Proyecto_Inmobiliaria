using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Inmobiliaria.Models;

public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
{
    public RepositorioPropietario(IConfiguration configuration) : base(configuration) { }

    public int Alta(Propietario propietarioParams)
    {
        try{    
            int idGenerado = 0;
            using var conexion = new MySqlConnection(connectionString);
            
            string consultaSql = @"INSERT INTO Propietario (dni, nombre, apellido, telefono, email, estado)
            VALUES (@dni, @nombre, @apellido, @telefono, @email, @estado);
            SELECT LAST_INSERT_ID();"; 

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@dni", propietarioParams.Dni);
            comando.Parameters.AddWithValue("@nombre", propietarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", propietarioParams.Apellido);
            comando.Parameters.AddWithValue("@telefono", propietarioParams.Telefono);
            comando.Parameters.AddWithValue("@email", propietarioParams.Email);
            comando.Parameters.AddWithValue("@estado", propietarioParams.Estado);

            conexion.Open();
            idGenerado = Convert.ToInt32(comando.ExecuteScalar());
            propietarioParams.Id_propietario = idGenerado;

            return idGenerado;
        }catch(Exception e)
        {
            Console.WriteLine($"Error en Alta Propietario Models {e.Message}");
            throw;
        }
    }

    public int Baja(Propietario propietarioParams)
    {
        try{
            int filasAfectadas = 0;
            
            using var conexion = new MySqlConnection(connectionString);
            
            string consultaSql = @"UPDATE Propietario 
            SET estado = @estado WHERE dni = @dni";
            
            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@estado", propietarioParams.Estado);
            comando.Parameters.AddWithValue("@dni", propietarioParams.Dni);

            conexion.Open();
            filasAfectadas = comando.ExecuteNonQuery();
            return filasAfectadas;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Fallo en baja Propietario Models {e.Message}");
            throw;
        }
    }

    public int Modificacion(Propietario propietarioParams)
    {
        try{    
            int filasAfectadas = 0;

            using var conexion = new MySqlConnection(connectionString);

            string consultaSql = @"UPDATE Propietario SET nombre = @nombre, 
            apellido = @apellido, dni = @dni, telefono = @telefono, email = @email, estado = @estado 
            WHERE id_propietario = @id_propietario";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@nombre", propietarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", propietarioParams.Apellido);
            comando.Parameters.AddWithValue("@dni", propietarioParams.Dni);
            comando.Parameters.AddWithValue("@telefono", propietarioParams.Telefono);
            comando.Parameters.AddWithValue("@email", propietarioParams.Email);
            comando.Parameters.AddWithValue("@estado", propietarioParams.Estado);
            comando.Parameters.AddWithValue("@id_propietario", propietarioParams.Id_propietario);

            conexion.Open();
            filasAfectadas = comando.ExecuteNonQuery();

            return filasAfectadas;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en ModificacionPropietarioModels {e.Message}");
            throw;
        }
    }

    public IList<Propietario> ObtenerTodos()
    {
        try{
            var lista = new List<Propietario>();

            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"SELECT id_propietario, nombre, apellido, dni, telefono, email, estado 
                                FROM Propietario WHERE estado = 1";

            using var comando = new MySqlCommand(consultaSql, conexion);

            conexion.Open();
            using var leerLista = comando.ExecuteReader();

            while (leerLista.Read())
            {
                var p = new Propietario
                {
                    Id_propietario = leerLista.GetInt32("id_propietario"),
                    Nombre = leerLista.GetString("nombre"),
                    Apellido = leerLista.GetString("apellido"),
                    Dni = leerLista.GetString("dni"),
                    Telefono = leerLista.IsDBNull(leerLista.GetOrdinal("telefono")) ? "" : leerLista.GetString("telefono"),
                    Email = leerLista.GetString("email"),
                    Estado = leerLista.GetBoolean("estado")};

                lista.Add(p);
            }

            return lista;
        }catch(Exception e)
        {
            Console.WriteLine($"Fallo en modificacion propietario models {e.Message}");
            throw;
        }
    }
}