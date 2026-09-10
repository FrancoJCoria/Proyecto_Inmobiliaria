using MySqlConnector;

namespace Inmobiliaria.Models;

public class RepositorioImagenInmueble : RepositorioBase, IRepositorioImagenInmueble
{
    public RepositorioImagenInmueble(IConfiguration configuration) : base(configuration) { }

    public int Alta(ImagenInmueble imagen)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"INSERT INTO ImagenInmueble (url_imagen, id_inmueble, estado)
            VALUES (@url, @idInmueble, @estado);
            SELECT LAST_INSERT_ID();";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@url", imagen.Url);
            comando.Parameters.AddWithValue("@idInmueble", imagen.InmuebleId);
            comando.Parameters.AddWithValue("@estado", imagen.Estado);

            conexion.Open();
            int idGenerado = Convert.ToInt32(comando.ExecuteScalar());
            imagen.Id = idGenerado;
            return idGenerado;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en Alta ImagenInmueble Models {e.Message}");
            throw;
        }
    }

    public int Baja(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"UPDATE ImagenInmueble SET estado = 0 WHERE id_imagen = @id";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@id", id);

            conexion.Open();
            return comando.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en Baja ImagenInmueble Models {e.Message}");
            throw;
        }
    }

    public ImagenInmueble? ObtenerPorId(int id)
    {
        ImagenInmueble? imagen = null;
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"SELECT id_imagen, url_imagen, id_inmueble, estado
            FROM ImagenInmueble WHERE id_imagen = @id";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@id", id);

            conexion.Open();
            using var lector = comando.ExecuteReader();

            if (lector.Read())
            {
                imagen = LeerImagen(lector);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en ObtenerPorId ImagenInmueble Models {e.Message}");
            throw;
        }
        return imagen;
    }

    public IList<ImagenInmueble> BuscarPorInmueble(int inmuebleId)
    {
        var lista = new List<ImagenInmueble>();
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"SELECT id_imagen, url_imagen, id_inmueble, estado
            FROM ImagenInmueble WHERE id_inmueble = @idInmueble AND estado = 1";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@idInmueble", inmuebleId);

            conexion.Open();
            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                lista.Add(LeerImagen(lector));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en BuscarPorInmueble ImagenInmueble Models {e.Message}");
            throw;
        }
        return lista;
    }

    public int EliminarTodasPorInmueble(int inmuebleId)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"UPDATE ImagenInmueble SET estado = 0 WHERE id_inmueble = @idInmueble";

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@idInmueble", inmuebleId);

            conexion.Open();
            return comando.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en EliminarTodasPorInmueble ImagenInmueble Models {e.Message}");
            throw;
        }
    }

    private static ImagenInmueble LeerImagen(MySqlDataReader lector)
    {
        return new ImagenInmueble
        {
            Id = lector.GetInt32("id_imagen"),
            Url = lector.GetString("url_imagen"),
            InmuebleId = lector.GetInt32("id_inmueble"),
            Estado = lector.GetBoolean("estado")
        };
    }
}
