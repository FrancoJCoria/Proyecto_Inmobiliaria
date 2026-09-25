using MySqlConnector;

namespace Inmobiliaria.Models;

public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
{
    public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration) { }

    public int Alta(TipoInmueble tipo)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"INSERT INTO TipoInmueble (nombre) VALUES (@nombre);
        SELECT LAST_INSERT_ID();";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@nombre", tipo.Nombre);

        conexion.Open();
        int idGenerado = Convert.ToInt32(comando.ExecuteScalar());
        tipo.Id_tipo = idGenerado;
        return idGenerado;
    }

    public int Baja(int id)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"DELETE FROM TipoInmueble WHERE id_tipo = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id", id);

        conexion.Open();
        return comando.ExecuteNonQuery();
    }

    public int Modificacion(TipoInmueble tipo)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE TipoInmueble SET nombre = @nombre WHERE id_tipo = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@nombre", tipo.Nombre);
        comando.Parameters.AddWithValue("@id", tipo.Id_tipo);

        conexion.Open();
        return comando.ExecuteNonQuery();
    }

    // Lista completa para el desplegable de tipos, con filtro opcional en el servidor.
    public IList<TipoInmueble> ObtenerTodos(string? busqueda = null)
    {
        var lista = new List<TipoInmueble>();
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = "SELECT id_tipo, nombre FROM TipoInmueble WHERE 1 = 1";

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            consultaSql += " AND nombre LIKE @busqueda";
        }

        consultaSql += " ORDER BY nombre ASC;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            // El % va en el valor del parámetro, nunca en el texto de la consulta.
            comando.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");
        }

        conexion.Open();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            lista.Add(new TipoInmueble
            {
                Id_tipo = lector.GetInt32("id_tipo"),
                Nombre = lector.GetString("nombre")
            });
        }
        return lista;
    }

    public TipoInmueble? ObtenerPorId(int id)
    {
        TipoInmueble? tipo = null;
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"SELECT id_tipo, nombre FROM TipoInmueble WHERE id_tipo = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id", id);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (lector.Read())
        {
            tipo = new TipoInmueble
            {
                Id_tipo = lector.GetInt32("id_tipo"),
                Nombre = lector.GetString("nombre")
            };
        }
        return tipo;
    }
}
