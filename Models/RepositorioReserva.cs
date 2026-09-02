//id_reserva, fecha_inicio, fecha_fin, fecha_fin_efectiva, monto_diario, estado, id_inmueble, id_inquilino, id_usuario_creador, id_usuario_finalizador
using MySqlConnector;

namespace Inmobiliaria.Models;

public class RepositorioReserva : RepositorioBase, IRepositorioReserva
{
    public RepositorioReserva(IConfiguration configuration) : base(configuration){}

//------------------------------------------------------------------------------------------CREAR LA RESERVA------------------------------------------//
    public int Alta(Reserva reservaParams)
    {
        try{
            //variable para guardar el ID generado en la base de datos y retornarlo    
            int idGenerado = 0;

            //variable que guarda la conexion a la bd
            using var conexion = new MySqlConnection(connectionString);

            //consultasql: inserta los datos y el last insert, agarra y devuelve el id generado
            string consultaSql = @"INSERT INTO Reserva(fecha_inicio, fecha_fin, fecha_fin_efectiva, monto_diario, estado, id_inmueble, id_inquilino, id_usuario_creador, id_usuario_finalizador)
            VALUES (@fecha_inicio, @fecha_fin, @fecha_fin_efectiva, @monto_diario, @estado, @id_inmueble, @id_inquilino, @id_usuario_creador, @id_usuario_finalizador);
            SELECT LAST_INSERT_ID();";

            //comando para la consulta y conexion
            using var comando = new MySqlCommand(consultaSql, conexion);

            //Asignacionde todos los parametros requeridos
            comando.Parameters.AddWithValue("@fecha_inicio", reservaParams.Fecha_inicio);
            comando.Parameters.AddWithValue("@fecha_fin", reservaParams.Fecha_fin);
            comando.Parameters.AddWithValue("@fecha_fin_efectiva", reservaParams.Fecha_fin_efectiva == DateTime.MinValue ? (object)DBNull.Value : reservaParams.Fecha_fin_efectiva);
            comando.Parameters.AddWithValue("@monto_diario", reservaParams.Monto_diario);
            comando.Parameters.AddWithValue("@estado", reservaParams.Estado);
            comando.Parameters.AddWithValue("@id_inmueble", reservaParams.Id_inmueble);
            comando.Parameters.AddWithValue("@id_inquilino", reservaParams.Id_inquilino);
            comando.Parameters.AddWithValue("@id_usuario_creador", reservaParams.Id_usuario_creador);
            comando.Parameters.AddWithValue("@id_usuario_finalizador", reservaParams.Id_usuario_finalizador == 0 ? (object)DBNull.Value : reservaParams.Id_usuario_finalizador);

            conexion.Open();
            
            //ejecutamos y obtenemos el ID generado mediante ExecuteScalar
            object resultado = comando.ExecuteScalar();
            if (resultado != null && int.TryParse(resultado.ToString(), out int id))
            {
                idGenerado = id;
            }

            return idGenerado;
        }catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }
 
//------------------------------------------------------------------------------------------BAJA LOGICA DE LA RESERVA----------------------------------//
    public int Baja(Reserva r)
    {
        try
        {
            //variable que guarda la conexion a la bd
            using var conexion = new MySqlConnection(connectionString);

            //consultasql: actualiza el estado de la reserva a inactivo (baja lógica) según su identificador
            string consultaSql = @"UPDATE Reserva SET estado = 0 WHERE id_reserva = @id_reserva;";

            //comando para la consulta y conexion
            using var comando = new MySqlCommand(consultaSql, conexion);

            //Asignación del parámetro de identificación (id_reserva)
            comando.Parameters.AddWithValue("@id_reserva", r.Id_reserva);

            conexion.Open();

            //ejecutamos el comando y retornamos las filas afectadas
            int filasAfectadas = comando.ExecuteNonQuery();

            return filasAfectadas;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }

 //------------------------------------------------------------------------------------------MODIFICAR LA RESERVA--------------------------------------//
    public int Modificacion(Reserva reservaParams)
    {
        try
        {
            //variable que guarda la conexion a la bd
            using var conexion = new MySqlConnection(connectionString);

            //consultasql: actualiza los datos de la reserva segun el id_reserva
            string consultaSql = @"UPDATE Reserva SET fecha_inicio = @fecha_inicio,  fecha_fin = @fecha_fin, 
            fecha_fin_efectiva = @fecha_fin_efectiva, 
            monto_diario = @monto_diario, 
            estado = @estado, 
            id_inmueble = @id_inmueble, 
            id_inquilino = @id_inquilino, 
            id_usuario_creador = @id_usuario_creador, 
            id_usuario_finalizador = @id_usuario_finalizador 
            WHERE id_reserva = @id_reserva;";

            //comando para la consulta y conexion
            using var comando = new MySqlCommand(consultaSql, conexion);

            //Asignacion de todos los parametros requeridos
            comando.Parameters.AddWithValue("@id_reserva", reservaParams.Id_reserva);
            comando.Parameters.AddWithValue("@fecha_inicio", reservaParams.Fecha_inicio);
            comando.Parameters.AddWithValue("@fecha_fin", reservaParams.Fecha_fin);
            comando.Parameters.AddWithValue("@fecha_fin_efectiva", reservaParams.Fecha_fin_efectiva == DateTime.MinValue ? (object)DBNull.Value : reservaParams.Fecha_fin_efectiva);
            comando.Parameters.AddWithValue("@monto_diario", reservaParams.Monto_diario);
            comando.Parameters.AddWithValue("@estado", reservaParams.Estado);
            comando.Parameters.AddWithValue("@id_inmueble", reservaParams.Id_inmueble);
            comando.Parameters.AddWithValue("@id_inquilino", reservaParams.Id_inquilino);
            comando.Parameters.AddWithValue("@id_usuario_creador", reservaParams.Id_usuario_creador);
            comando.Parameters.AddWithValue("@id_usuario_finalizador", reservaParams.Id_usuario_finalizador == 0 ? (object)DBNull.Value : reservaParams.Id_usuario_finalizador);

            conexion.Open();

            //ejecutamos el comando y retornamos las filas afectadas
            int filasAfectadas = comando.ExecuteNonQuery();

            return filasAfectadas;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }

    public IList<Reserva> ObtenerTodos()
    {
        var lista = new List<Reserva>();
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT id_reserva, fecha_inicio, fecha_fin, fecha_fin_efectiva, monto_diario,
        estado, id_inmueble, id_inquilino, id_usuario_creador, id_usuario_finalizador FROM Reserva";

        using var comando = new MySqlCommand(consultaSql, conexion);
        conexion.Open();
        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            lista.Add(LeerReserva(lector));
        }
        return lista;
    }

    public Reserva? ObtenerPorId(int id)
    {
        Reserva? reserva = null;
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT id_reserva, fecha_inicio, fecha_fin, fecha_fin_efectiva, monto_diario,
        estado, id_inmueble, id_inquilino, id_usuario_creador, id_usuario_finalizador
        FROM Reserva WHERE id_reserva = @id_reserva";

        using var comando = new MySqlCommand(consultaSql, conexion);
        comando.Parameters.AddWithValue("@id_reserva", id);
        conexion.Open();
        using var lector = comando.ExecuteReader();

        if (lector.Read())
        {
            reserva = LeerReserva(lector);
        }
        return reserva;
    }

    private static Reserva LeerReserva(MySqlDataReader lector)
    {
        return new Reserva
        {
            Id_reserva = lector.GetInt32("id_reserva"),
            Fecha_inicio = lector.GetDateTime("fecha_inicio"),
            Fecha_fin = lector.GetDateTime("fecha_fin"),
            Fecha_fin_efectiva = lector.IsDBNull(lector.GetOrdinal("fecha_fin_efectiva"))
                ? DateTime.MinValue
                : lector.GetDateTime("fecha_fin_efectiva"),
            Monto_diario = lector.GetDecimal("monto_diario"),
            Estado = lector.GetBoolean("estado"),
            Id_inmueble = lector.GetInt32("id_inmueble"),
            Id_inquilino = lector.GetInt32("id_inquilino"),
            Id_usuario_creador = lector.GetInt32("id_usuario_creador"),
            Id_usuario_finalizador = lector.IsDBNull(lector.GetOrdinal("id_usuario_finalizador"))
                ? 0
                : lector.GetInt32("id_usuario_finalizador")
        };
    }
}
