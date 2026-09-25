using MySqlConnector;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop.Infrastructure;

namespace Inmobiliaria.Models;

public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
{
    public RepositorioPropietario(IConfiguration configuration) : base(configuration) { }


//------------------------------------------------------------CREAR/ALTA PROPIETARIO-----------------------------------------------------------------
    public int Alta(Propietario propietarioParams)
    {
        try{    

            int idGenerado = 0;
            using var conexion = new MySqlConnection(connectionString);
            conexion.Open();

            // Verificar si el DNI ya existe en la base de datos
            string consultaExisteDni = "SELECT COUNT(1) FROM Propietario WHERE dni = @dni;";
            using (var cmdExiste = new MySqlCommand(consultaExisteDni, conexion))
            {
                cmdExiste.Parameters.AddWithValue("@dni", propietarioParams.Dni);
                int cantidad = Convert.ToInt32(cmdExiste.ExecuteScalar());

                //si cantidad mayor a cero, es porque ese dni esta en uso
                if (cantidad > 0)
                {
                    return -1; 
                }
            }

            //en caso de que no exista se continua con la creacion (INSERT)
            string consultaSql = @"INSERT INTO Propietario (dni, nombre, apellido, telefono, email, estado)
            VALUES (@dni, @nombre, @apellido, @telefono, @email, @estado);
            SELECT LAST_INSERT_ID();"; 

            using var comando = new MySqlCommand(consultaSql, conexion);

            //mapeo de parametros para evitar inyecciones
            comando.Parameters.AddWithValue("@dni", propietarioParams.Dni);
            comando.Parameters.AddWithValue("@nombre", propietarioParams.Nombre);
            comando.Parameters.AddWithValue("@apellido", propietarioParams.Apellido);
            comando.Parameters.AddWithValue("@telefono", propietarioParams.Telefono);
            comando.Parameters.AddWithValue("@email", propietarioParams.Email);
            comando.Parameters.AddWithValue("@estado", propietarioParams.Estado);

            
            idGenerado = Convert.ToInt32(comando.ExecuteScalar());
            propietarioParams.Id_propietario = idGenerado;

            return idGenerado;
        }catch(Exception e)
        {
            Console.WriteLine($"Error en Alta Propietario Models {e.Message}");
            throw;
        }
    }

    //------------------------------------------------------------BAJA LOGICA PROPIETARIO-----------------------------------------------------------------
    public int Baja(Propietario propietarioParams)
    {
        try{
            int filasAfectadas = 0;
            
            using var conexion = new MySqlConnection(connectionString);
            
            string consultaSql = @"UPDATE Propietario 
            SET estado = @estado WHERE dni = @dni";//Solo desactiva cambiando estado sin borrar fila
            
            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@estado", propietarioParams.Estado);
            comando.Parameters.AddWithValue("@dni", propietarioParams.Dni);

            conexion.Open();
            filasAfectadas = comando.ExecuteNonQuery();
            return filasAfectadas;//Devuelve 1 si lo encontro y cambio, 0 si no
        }
        catch(Exception e)
        {
            Console.WriteLine($"Fallo en baja Propietario Models {e.Message}");
            throw;
        }
    }

//------------------------------------------------------------MODIFICAR PROPIETARIO-----------------------------------------------------------------

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

//------------------------------------------------------------LISTAR TODOS LOS PROPIETARIOS (PAGINADO)-----------------------------------------------------------------
    // Sin defaults a propósito: si los tuviera, ObtenerTodos() sin argumentos sería
    // ambiguo entre esta versión y la lista completa de abajo.
    public IList<Propietario> ObtenerTodos(int pagina, int tamanoPagina)
    {
        try{
            var lista = new List<Propietario>();//conetenedor chill
            int offset = (pagina - 1) * tamanoPagina;
            using var conexion = new MySqlConnection(connectionString);
            string consultaSql = @"SELECT id_propietario, nombre, apellido, dni, telefono, email, estado 
                                FROM Propietario WHERE estado = 1
                                ORDER BY id_propietario ASC
                                LIMIT @limit OFFSET @offset;"; //Limit va a indicar cuantos traer y offset cuantos saltar desde un inicio

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@limit", tamanoPagina);
            comando.Parameters.AddWithValue("@offset", offset);
            conexion.Open();
            
            using var leerLista = comando.ExecuteReader(); // Abre el cursor hacia adelante para leer registros

                while (leerLista.Read()) //va iterando fila por fila hasta encontrar el resultado
                {
                    var p = new Propietario
                    {
                        Id_propietario = leerLista.GetInt32("id_propietario"), 
                        Nombre = leerLista.GetString("nombre"),                
                        Apellido = leerLista.GetString("apellido"),
                        Dni = leerLista.GetString("dni"),
                        Telefono = leerLista.IsDBNull(leerLista.GetOrdinal("telefono")) ? "" : leerLista.GetString("telefono"), 
                        Email = leerLista.IsDBNull(leerLista.GetOrdinal("email")) ? "" : leerLista.GetString("email"),
                        Estado = leerLista.GetBoolean("estado")                 
                    };

                    lista.Add(p);//se agrega
                }

                return lista; // Retorna la lista con los 5 elementos de la pagina
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fallo en ObtenerTodos Propietario Models: {e.Message}");
                throw;
            }
    }
    //------------------------------------------------------------CONTAR PROPIETARIOS (TOTAL REAL PARA LA PAGINACION)-----------------------------------------------------------------
    // Tiene que usar exactamente el mismo WHERE que el listado paginado de arriba, asi
    // el numero del titulo coincide con la suma de filas de todas las paginas.
    public int Contar()
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);

            string consultaSql = "SELECT COUNT(*) FROM Propietario WHERE estado = 1;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            conexion.Open();
            return Convert.ToInt32(comando.ExecuteScalar());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en Contar Propietario Models: {e.Message}");
            throw;
        }
    }

    //------------------------------------------------------------BUSCAR PROPIETARIO POR ID-----------------------------------------------------------------
    public Propietario? ObtenerPorId(int id)
    {
        try
        {
            Propietario? propietario = null;
            using var conexion = new MySqlConnection(connectionString);

            string consultaSql = @"SELECT id_propietario, nombre, apellido, dni, telefono, email, estado 
            FROM Propietario 
            WHERE id_propietario = @id;"; 

            using var comando = new MySqlCommand(consultaSql, conexion);
            comando.Parameters.AddWithValue("@id", id);

            conexion.Open();
            using var lector = comando.ExecuteReader(); // Abre lectura del registro

            if (lector.Read()) // Si encontró una fila coincidente
            {
                propietario = new Propietario{
                    Id_propietario = lector.GetInt32("id_propietario"),
                    Nombre = lector.GetString("nombre"),
                    Apellido = lector.GetString("apellido"),
                    Dni = lector.GetString("dni"),
                    Telefono = lector.IsDBNull(lector.GetOrdinal("telefono")) ? "" : lector.GetString("telefono"),
                    Email = lector.IsDBNull(lector.GetOrdinal("email")) ? "" : lector.GetString("email"),
                    Estado = lector.GetBoolean("estado")
                };
            }

            return propietario; // Retorna la entidad encontrada o null
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo en ObtenerPorId Propietario Models: {e.Message}");
            throw;
        }
    }

    //------------------------------------------------------------FILTRAR PARA LOS DESPLEGABLES-----------------------------------------------------------------
    // Devuelve solo los propietarios que coinciden con el termino escrito en el buscador del
    // desplegable, y como mucho "limite" filas. Esta es la unica forma en que un desplegable
    // obtiene sus opciones: nunca se pide la tabla completa.
    public IList<Propietario> ObtenerTodos(string? busqueda = null, int limite = 20)
    {
        try
        {
            var lista = new List<Propietario>();
            using var conexion = new MySqlConnection(connectionString);

            // Solo se ofrecen los propietarios activos en los desplegables: no tiene sentido
            // asignarle un inmueble a uno que está dado de baja.
            string consultaSql = @"SELECT id_propietario, nombre, apellido, dni, telefono, email, estado 
            FROM Propietario 
            WHERE estado = 1";

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consultaSql += " AND (nombre LIKE @busqueda OR apellido LIKE @busqueda OR dni LIKE @busqueda)";
            }

            consultaSql += " ORDER BY apellido, nombre ASC LIMIT @limite;";

            using var comando = new MySqlCommand(consultaSql, conexion);
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                // El % va en el valor del parámetro, nunca en el texto de la consulta.
                comando.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");
            }
            // Acota la cantidad de filas que vuelven de la base, que es lo que evita que
            // un termino corto se trajeera el catalogo entero.
            comando.Parameters.AddWithValue("@limite", limite);

            conexion.Open();
            using var leerLista = comando.ExecuteReader();

            while (leerLista.Read())
            {
                lista.Add(new Propietario
                {
                    Id_propietario = leerLista.GetInt32("id_propietario"),
                    Nombre = leerLista.GetString("nombre"),
                    Apellido = leerLista.GetString("apellido"),
                    Dni = leerLista.GetString("dni"),
                    Telefono = leerLista.IsDBNull(leerLista.GetOrdinal("telefono")) ? "" : leerLista.GetString("telefono"),
                    Email = leerLista.IsDBNull(leerLista.GetOrdinal("email")) ? "" : leerLista.GetString("email"),
                    Estado = leerLista.GetBoolean("estado")});
            }

            return lista;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fallo al filtrar propietarios para el desplegable: {e.Message}");
            throw;
        }
    }
}