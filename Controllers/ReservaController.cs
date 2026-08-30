using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers;

public class ReservaController : Controller
{
    private readonly IRepositorioReserva _repositorio;

    public ReservaController(IRepositorioReserva repositorio)
    {
        _repositorio = repositorio;
    }

    //------------------------------------------------------------------------------------------CREAR LA RESERVA------------------------------------------//
    [HttpPost]
    public IActionResult Create([FromBody] Reserva reserva) 
    {
        try
        {
            //verificamos que los datos de la reserva no sean nulos
            if (reserva == null)
            {
                return BadRequest("Los datos de la reserva son nulos");
            }

            //ejecutamos el alta en el repositorio y guardamos el id generado
            int idGenerado = _repositorio.Alta(reserva);

            //devolvemos la respuesta en formato json
            return Ok(new {
                mensaje = "Reserva creada",
                id = idGenerado, 
                reserva = reserva});
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al crear reserva", detalle = e.Message });
        }
    }

    //------------------------------------------------------------------------------------------DAR DE BAJA LA RESERVA------------------------------------//
    [HttpPost]
    public IActionResult Delete([FromBody] Reserva r)
    {
        //verificamos que el objeto o el id de la reserva no sean vacos o nulos
        if (r == null || r.Id_reserva == 0)
        {
            return BadRequest(new { error = "Se requiere el ID de la reserva para darla de baja" });
        }

        try
        {
            //aplicamos baja logia cambiando el estado a false
            r.Estado = false;
            int filasAfectadas = _repositorio.Baja(r);

            //si no se afectó ninguna fila, significa que el id no existe
            if (filasAfectadas == 0)
            {
                return NotFound(new { error = $"No se encontró ningún id {r.Id_reserva}." });
            }

            return Ok(new
            {
                mensaje = "Reserva dada de baja con éxito",
                filasAfectadas = filasAfectadas});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al dar de baja", detalle = ex.Message });
        }
    }

    //------------------------------------------------------------------------------------------MODIFICAR LA RESERVA--------------------------------------//
    [HttpPut]
    [HttpPost]
    public IActionResult Edit(int id, [FromBody] Reserva reservaParams)
    {
        //verificamos que los datos a modificar no sean nulos
        if (reservaParams == null)
        {
            return BadRequest(new { error = "Los datos de la reserva son requeridos" });
        }

        //asignamos el id recibido por parametro al objeto de la reserva
        reservaParams.Id_reserva = id;

        try
        {
            //ejecutamos la modificación en el repositorio
            int filasAfectadas = _repositorio.Modificacion(reservaParams);

            //si no se actualizo ninguna fila, retornamos error
            if (filasAfectadas == 0)
            {
                return NotFound(new { error = $"No se encontro ninguna reserva con el ID {id} para modificar" });
            }

            return Ok(new
            {
                mensaje = "Reserva modificada",
                filasAfectadas = filasAfectadas,
                reserva = reservaParams});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error en el servidor al modificar reserva", detalle = ex.Message });
        }
    }
}