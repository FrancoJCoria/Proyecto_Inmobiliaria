using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Inmobiliaria.Models;


public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
{
    public RepositorioInquilino(IConfiguration configuration) : base(configuration) { }

    public int Alta(Inquilino inquilino)
    {
        int idGenerado = 0;
        using var connection = new MySqlConnection(connectionString);
        string consultaSql = @"INSERT INTO Inquilino (dni, nombre, apellido, telefono, email, estado)
        VALUES (@dni, @nombre, @apellido, @telefono, @email, @estado);
        SELECT LAST_INSERT_ID();";
        using var command = new MySqlCommand(consultaSql, connection);
        BindParams(command, inquilino);
        connection.Open();
        idGenerado = Convert.ToInt32(command.ExecuteScalar());
        inquilino.Id_inquilino = idGenerado;
        return idGenerado;
    }

    public int Baja(int id)
    {
        int filasAfectadas = 0;
        using var connection = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE Inquilino SET estado = @estado WHERE id_inquilino = @id";
        using var command = new MySqlCommand(consultaSql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@estado", false);
        connection.Open();
        filasAfectadas = command.ExecuteNonQuery();
        return filasAfectadas;
    }

    public int Modificacion(Inquilino inquilino)
    {
        int filasAfectadas = 0;
        using var connection = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE Inquilino SET dni = @dni, nombre = @nombre, apellido = @apellido, telefono = @telefono, email = @email, estado = @estado WHERE id_inquilino = @id";
        using var command = new MySqlCommand(consultaSql, connection);
        BindParams(command, inquilino);
        BindId(command, inquilino);
        connection.Open();
        filasAfectadas = command.ExecuteNonQuery();
        return filasAfectadas;
    }

    public IList<Inquilino> ObtenerTodos()
    {
        var inquilinos = new List<Inquilino>();
        using var connection = new MySqlConnection(connectionString);
        string consultaSql = @"SELECT id_inquilino, dni, nombre, apellido, telefono, email, estado FROM Inquilino";
        using var command = new MySqlCommand(consultaSql, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            inquilinos.Add(new Inquilino
            {
                Id_inquilino = reader.GetInt32("id_inquilino"),
                Dni = reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Telefono = reader.GetString("telefono"),
                Email = reader.GetString("email"),
                Estado = reader.GetBoolean("estado")
            });
        }
        return inquilinos;
    }

    private static void BindId(MySqlCommand cmd, Inquilino inquilino)
    {
        cmd.Parameters.AddWithValue("@id", inquilino.Id_inquilino);
    }

    private static void BindParams(MySqlCommand cmd, Inquilino inquilino)
    {
        cmd.Parameters.AddWithValue("@dni", inquilino.Dni);
        cmd.Parameters.AddWithValue("@nombre", inquilino.Nombre);
        cmd.Parameters.AddWithValue("@apellido", inquilino.Apellido);
        cmd.Parameters.AddWithValue("@telefono", inquilino.Telefono);
        cmd.Parameters.AddWithValue("@email", inquilino.Email);
        cmd.Parameters.AddWithValue("@estado", inquilino.Estado);
    }
}