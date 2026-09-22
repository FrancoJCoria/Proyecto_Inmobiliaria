//id_pago, concepto, fecha_pago, importe, estado, id_reserva, id_usuario_creador, id_usuario_anulador
using MySqlConnector;

namespace Inmobiliaria.Models;

public class RepositorioPago : RepositorioBase, IRepositorioPago
{
    public RepositorioPago(IConfiguration configuration) : base(configuration) { }

    //--------------------------------------------------------------------------------------CREAR PAGO------------------------------------------//
    public int Alta(Pago pago)
    {
        try
        {
            int idGenerado = 0;
            using var conexion = new MySqlConnection(connectionString);

            string consultaSql = @"INSERT INTO Pago (concepto, fecha_pago, importe, estado, id_reserva, id_usuario_creador, id_usuario_anulador)
            VALUES (@concepto, @fecha_pago, @importe, @estado, @id_reserva, @id_usuario_creador, @id_usuario_anulador);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@concepto", pago.Concepto);
            comando.Parameters.AddWithValue("@fecha_pago", pago.Fecha_pago);
            comando.Parameters.AddWithValue("@importe", pago.Importe);
            comando.Parameters.AddWithValue("@estado", pago.Estado);
            comando.Parameters.AddWithValue("@id_reserva", pago.Id_reserva);
            comando.Parameters.AddWithValue("@id_usuario_creador", pago.Id_usuario_creador);
            comando.Parameters.AddWithValue("@id_usuario_anulador", pago.Id_usuario_anulador == 0 ? (object)DBNull.Value : pago.Id_usuario_anulador);

            conexion.Open();
            object? resultado = comando.ExecuteScalar();
            if (resultado != null && int.TryParse(resultado.ToString(), out int id))
            {
                idGenerado = id;
            }
            return idGenerado;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }

    //--------------------------------------------------------------------------------------MODIFICAR PAGO (SOLO CONCEPTO)------------------------//
    public int Modificacion(Pago pago)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"UPDATE Pago SET concepto = @concepto WHERE id_pago = @id_pago;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@concepto", pago.Concepto);
            comando.Parameters.AddWithValue("@id_pago", pago.Id_pago);

            conexion.Open();
            return comando.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }

    //--------------------------------------------------------------------------------------ANULAR PAGO (BAJA LOGICA)-----------------------------//
    public int Anular(Pago pago)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"UPDATE Pago SET estado = 0, id_usuario_anulador = @id_usuario_anulador WHERE id_pago = @id_pago;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@id_usuario_anulador", pago.Id_usuario_anulador);
            comando.Parameters.AddWithValue("@id_pago", pago.Id_pago);

            conexion.Open();
            return comando.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }

    public IList<Pago> ObtenerPorReserva(int idReserva)
    {
        var lista = new List<Pago>();
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT id_pago, concepto, fecha_pago, importe, estado, id_reserva, id_usuario_creador, id_usuario_anulador
        FROM Pago WHERE id_reserva = @id_reserva
        ORDER BY fecha_pago ASC, id_pago ASC;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id_reserva", idReserva);
        conexion.Open();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            lista.Add(LeerPago(lector));
        }
        return lista;
    }

    public Pago? ObtenerPorId(int id)
    {
        Pago? pago = null;
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT id_pago, concepto, fecha_pago, importe, estado, id_reserva, id_usuario_creador, id_usuario_anulador
        FROM Pago WHERE id_pago = @id_pago;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id_pago", id);
        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (lector.Read())
        {
            pago = LeerPago(lector);
        }
        return pago;
    }

    private static Pago LeerPago(MySqlDataReader lector)
    {
        return new Pago
        {
            Id_pago = lector.GetInt32("id_pago"),
            Concepto = lector.IsDBNull(lector.GetOrdinal("concepto"))
                ? ""
                : lector.GetString("concepto"),
            Fecha_pago = lector.GetDateTime("fecha_pago"),
            Importe = lector.GetDecimal("importe"),
            Estado = lector.GetBoolean("estado"),
            Id_reserva = lector.GetInt32("id_reserva"),
            Id_usuario_creador = lector.GetInt32("id_usuario_creador"),
            Id_usuario_anulador = lector.IsDBNull(lector.GetOrdinal("id_usuario_anulador"))
                ? 0
                : lector.GetInt32("id_usuario_anulador")
        };
    }
}