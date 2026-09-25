using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Inmobiliaria.Models;


public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
{
    public RepositorioInquilino(IConfiguration configuration) : base(configuration) { }

//----------------------------------------------------------------CREAR INQUILINO-------------------------------------------

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

//----------------------------------------------------------------BAJA LOGICA INQUILINO-------------------------------------------

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

//----------------------------------------------------------------MODIFICAR LOS INQUILINOS-------------------------------------------

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

//----------------------------------------------------------------LISTAR TODOS LOS INQUILINOS (PAGINADO)-------------------------------------------
    // Sin defaults a propósito: si los tuviera, ObtenerTodos() sin argumentos sería
    // ambiguo entre esta versión y la lista completa de abajo.
    public IList<Inquilino> ObtenerTodos(int pagina, int tamanoPagina)
    {
        try{
        var inquilinos = new List<Inquilino>();
        int offset = (pagina - 1) * tamanoPagina;
        using var connection = new MySqlConnection(connectionString);
        string consultaSql = @"SELECT id_inquilino, dni, nombre, apellido, telefono, email, estado FROM Inquilino 
        ORDER BY id_inquilino ASC
        LIMIT @limit OFFSET @offset;";
        using var command = new MySqlCommand(consultaSql, connection);
        command.Parameters.AddWithValue("@limit", tamanoPagina);
        command.Parameters.AddWithValue("@offset", offset);

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
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en ObtenerTodos Inquilino: {e.Message}");
            throw;
        }
    }

//----------------------------------------------------------------LISTAR TODOS LOS INQUILINOS (SIN PAGINAR - PARA SELECTS)-------------------------------------------
    // Lista completa para los desplegables, con filtro opcional en el servidor. Antes esta
    // llamada caía en la versión paginada con sus defaults y por eso el desplegable de
    // reservas solo mostraba los primeros 5 inquilinos.
    public IList<Inquilino> ObtenerTodos(string? busqueda = null)
    {
        try
        {
            var inquilinos = new List<Inquilino>();
            using var connection = new MySqlConnection(connectionString);

            // Solo se ofrecen los inquilinos activos en los desplegables: no tiene sentido
            // reservar a uno que está dado de baja.
            string consultaSql = @"SELECT id_inquilino, dni, nombre, apellido, telefono, email, estado
            FROM Inquilino
            WHERE estado = 1";

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consultaSql += " AND (nombre LIKE @busqueda OR apellido LIKE @busqueda OR dni LIKE @busqueda)";
            }

            consultaSql += " ORDER BY apellido, nombre ASC;";

            using var command = new MySqlCommand(consultaSql, connection);
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                // El % va en el valor del parámetro, nunca en el texto de la consulta.
                command.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");
            }

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
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en ObtenerTodos sin paginar Inquilino: {e.Message}");
            throw;
        }
    }

//------------------------------------------------------------CONTAR INQUILINOS (TOTAL REAL PARA LA PAGINACION)--------------------------------
    // Ojo: aca NO se filtra por estado a proposito. El listado paginado de arriba
    // tampoco lo hace, porque la vista muestra el badge Activo/Inactivo. Si el conteo
    // filtrara y el listado no, el titulo y la tabla dejarian de coincidir.
    public int Contar()
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);

            string consultaSql = "SELECT COUNT(*) FROM Inquilino;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            conexion.Open();
            return Convert.ToInt32(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en Contar Inquilino: {e.Message}");
            throw;
        }
    }

//-------------------------------------------------------------OBETENER INQUILINO POR ID --------------------------------------------
    public Inquilino? ObtenerPorId(int id)
    {
        try
        {
            Inquilino? inquilino = null;
            using var connection = new MySqlConnection(connectionString);
            string consultaSql = @"SELECT id_inquilino, dni, nombre, apellido, telefono, email, estado 
            FROM Inquilino 
            WHERE id_inquilino = @id;";

            using var command = new MySqlCommand(consultaSql, connection);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                inquilino = new Inquilino
                {
                    Id_inquilino = reader.GetInt32("id_inquilino"),
                    Dni = reader.GetString("dni"),
                    Nombre = reader.GetString("nombre"),
                    Apellido = reader.GetString("apellido"),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? "" : reader.GetString("telefono"),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                    Estado = reader.GetBoolean("estado")
                };
            }
            return inquilino;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en ObtenerPorId Inquilino: {e.Message}");
            throw;
        }
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