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
        comando.Parameters.AddWithValue("@estado", 0);
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

    public int ModificarPortada(int id, string url)
    {
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE Inmueble SET portada = @portada WHERE id_inmueble = @id";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@portada", string.IsNullOrEmpty(url) ? DBNull.Value : url);
        comando.Parameters.AddWithValue("@id", id);

        conexion.Open();
        return comando.ExecuteNonQuery();
    }

    public IList<Inmueble> ObtenerTodos(int pagina = 1, int tamanoPagina = 5)
    {        
        
        var lista = new List<Inmueble>();
        int offset = (pagina - 1) * tamanoPagina;
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"SELECT id_inmueble, direccion, cupo, precio_dia, porcentaje_reserva,
        disponible, portada, id_propietario, id_tipo, estado
        FROM Inmueble WHERE estado = 1
        ORDER BY id_inmueble ASC
        LIMIT @limit OFFSET @offset;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@limit", tamanoPagina);
        comando.Parameters.AddWithValue("@offset", offset);
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
            Direccion = lector.IsDBNull(lector.GetOrdinal("direccion"))
                ? ""
                : lector.GetString("direccion"),
            Cupo = lector.IsDBNull(lector.GetOrdinal("cupo"))
                ? 0
                : lector.GetInt32("cupo"),
            Precio_dia = lector.IsDBNull(lector.GetOrdinal("precio_dia"))
                ? 0m
                : lector.GetDecimal("precio_dia"),
            Porcentaje_reserva = lector.IsDBNull(lector.GetOrdinal("porcentaje_reserva"))
                ? 0m
                : lector.GetDecimal("porcentaje_reserva"),
            Disponible = LeerBool(lector, "disponible"),
            Portada = lector.IsDBNull(lector.GetOrdinal("portada"))
                ? ""
                : lector.GetString("portada"),
            Id_propietario = lector.GetInt32("id_propietario"),
            Id_tipo = lector.GetInt32("id_tipo"),
            Estado = LeerBool(lector, "estado")
        };
    }

    private static bool LeerBool(MySqlDataReader lector, string columna)
    {
        int ordinal = lector.GetOrdinal(columna);
        if (lector.IsDBNull(ordinal)) return false;

        object valor = lector.GetValue(ordinal);
        if (valor is bool booleano) return booleano;
        if (valor is string texto)
        {
            if (texto == "1" || texto.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (texto == "0" || texto.Trim().Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return Convert.ToInt32(valor) != 0;
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



    public IList<Inmueble> BuscarDisponiblesPorFechas(DateTime fechaInicio, DateTime fechaFin)
    {
        var lista = new List<Inmueble>();
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT i.id_inmueble, i.direccion, i.cupo, i.precio_dia, i.porcentaje_reserva,
        i.disponible, i.portada, i.id_propietario, i.id_tipo, i.estado
        FROM Inmueble i
        WHERE i.estado = 1 AND i.disponible = 1
        AND i.id_inmueble NOT IN (
        SELECT r.id_inmueble
        FROM Reserva r
        WHERE r.estado = 1 
        AND r.fecha_inicio <= @fechaFin 
        AND COALESCE(r.fecha_fin_efectiva, r.fecha_fin) >= @fechaInicio) ORDER BY i.id_inmueble ASC;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@fechaInicio", fechaInicio);
        comando.Parameters.AddWithValue("@fechaFin", fechaFin);

        conexion.Open();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            lista.Add(LeerInmueble(lector));
        }
        return lista;
    }
}
