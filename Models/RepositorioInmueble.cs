using MySqlConnector;

namespace Inmobiliaria.Models;

public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
{
    public RepositorioInmueble(IConfiguration configuration) : base(configuration) { }

    public int Alta(Inmueble inmueble)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"INSERT INTO Inmueble (direccion, cupo, precio_dia, porcentaje_reserva,
        disponible, portada, id_propietario, id_tipo, estado)
        VALUES (@direccion, @cupo, @precio_dia, @porcentaje_reserva, @disponible, @portada,
        @id_propietario, @id_tipo, @estado);
        SELECT LAST_INSERT_ID();";

        using var comando = new MySqlCommand(consultaSql, conexion);
        BindParams(comando, inmueble);

        conexion.Open();
        int idGenerado = Convert.ToInt32(comando.ExecuteScalar());
        inmueble.Id_inmueble = idGenerado;
        return idGenerado;
    }

    public int Baja(int id)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE Inmueble SET estado = @estado WHERE id_inmueble = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@estado", "Inactivo");
        comando.Parameters.AddWithValue("@id", id);

        conexion.Open();
        return comando.ExecuteNonQuery();
    }

    public int Modificacion(Inmueble inmueble)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE Inmueble SET direccion = @direccion, cupo = @cupo,
        precio_dia = @precio_dia, porcentaje_reserva = @porcentaje_reserva, disponible = @disponible,
        portada = @portada, id_propietario = @id_propietario, id_tipo = @id_tipo, estado = @estado
        WHERE id_inmueble = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        BindParams(comando, inmueble);
        comando.Parameters.AddWithValue("@id", inmueble.Id_inmueble);

        conexion.Open();
        return comando.ExecuteNonQuery();
    }

    public IList<Inmueble> ObtenerTodos()
    {
        var lista = new List<Inmueble>();
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"SELECT id_inmueble, direccion, cupo, precio_dia, porcentaje_reserva,
        disponible, portada, id_propietario, id_tipo, estado
        FROM Inmueble WHERE estado = 'Activo'";

        using var comando = new MySqlCommand(consultaSql, conexion);
        conexion.Open();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            lista.Add(LeerInmueble(lector));
        }
        return lista;
    }

    public Inmueble? ObtenerPorId(int id)
    {
        Inmueble? inmueble = null;
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"SELECT id_inmueble, direccion, cupo, precio_dia, porcentaje_reserva,
        disponible, portada, id_propietario, id_tipo, estado
        FROM Inmueble WHERE id_inmueble = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id", id);

        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (lector.Read())
        {
            inmueble = LeerInmueble(lector);
        }
        return inmueble;
    }

    private static Inmueble LeerInmueble(MySqlDataReader lector)
    {
        return new Inmueble
        {
            Id_inmueble = lector.GetInt32("id_inmueble"),
            Direccion = lector.GetString("direccion"),
            Cupo = lector.GetInt32("cupo"),
            Precio_dia = lector.GetDecimal("precio_dia"),
            Porcentaje_reserva = lector.GetDecimal("porcentaje_reserva"),
            Disponible = lector.GetBoolean("disponible"),
            Portada = lector.IsDBNull(lector.GetOrdinal("portada")) ? "" : lector.GetString("portada"),
            Id_propietario = lector.GetInt32("id_propietario"),
            Id_tipo = lector.GetInt32("id_tipo"),
            Estado = lector.IsDBNull(lector.GetOrdinal("estado")) ? "" : lector.GetString("estado")
        };
    }

    private static void BindParams(MySqlCommand comando, Inmueble inmueble)
    {
        comando.Parameters.AddWithValue("@direccion", inmueble.Direccion);
        comando.Parameters.AddWithValue("@cupo", inmueble.Cupo);
        comando.Parameters.AddWithValue("@precio_dia", inmueble.Precio_dia);
        comando.Parameters.AddWithValue("@porcentaje_reserva", inmueble.Porcentaje_reserva);
        comando.Parameters.AddWithValue("@disponible", inmueble.Disponible);
        comando.Parameters.AddWithValue("@portada", inmueble.Portada);
        comando.Parameters.AddWithValue("@id_propietario", inmueble.Id_propietario);
        comando.Parameters.AddWithValue("@id_tipo", inmueble.Id_tipo);
        comando.Parameters.AddWithValue("@estado", inmueble.Estado);
    }
}
