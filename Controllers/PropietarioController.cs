using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using System.Linq.Expressions;

namespace Inmobiliaria.Controllers;

public class PropietarioController : Controller
{
    private readonly IRepositorioPropietario _repositorio;

    public PropietarioController(IRepositorioPropietario repositorio)
    {
        _repositorio = repositorio;
    }

    //  depende del que se necesite: post, put, get
    [HttpPost]
    public IActionResult Create([FromBody] Propietario propie) 
    {
        try{
        if (propie == null)
        {
            return BadRequest("los datos del propietario son nulos");
        }

        int idGenerado = _repositorio.Alta(propie);

        // Devolver json
        return Ok(new {
            mensaje = "propietario creado",
            id = idGenerado,
            propietario = propie});
        }catch(Exception e)
        {
            return StatusCode(500, new { error = "Error al crearPropietario", detalle = e.Message });
        }
    }

    [HttpPost]
    public IActionResult Delete([FromBody] Propietario p)
    {
        if (p == null || string.IsNullOrEmpty(p.Dni))
        {
            return BadRequest(new { error = "Se requiere el DNI para dar de baja al propietario." });
        }

        try
        {
            p.Estado = false;
            int filasAfectadas = _repositorio.Baja(p);

            if (filasAfectadas == 0)
            {
                return NotFound(new { error = $"No se encontró ningún propietario con el DNI {p.Dni}." });
            }

            return Ok(new
            {
                mensaje = "Propietario dado de baja con éxito",
                filasAfectadas = filasAfectadas});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al dar de baja", detalle = ex.Message });
        }
    }

    [HttpPut]
    [HttpPost]
    public IActionResult Edit(int id, [FromBody] Propietario propietarioParams)
    {
        if (propietarioParams == null)
        {
            return BadRequest(new { error = "Los datos del propietario son requeridos" });
        }

        propietarioParams.Id_propietario = id;

        try
        {
            int filasAfectadas = _repositorio.Modificacion(propietarioParams);

            if (filasAfectadas == 0)
            {
                return NotFound(new { error = $"No se encontró ningún propietario con el ID {id} para modificar." });
            }

            return Ok(new
            {
                mensaje = "Propietario modificado",
                filasAfectadas = filasAfectadas,
                propietario = propietarioParams});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error en el servidor al modificar Propietario", detalle = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var lista = _repositorio.ObtenerTodos();
            return Ok(lista);
        }catch(Exception e)
        {
            return StatusCode(500, new{error = "Error al traer lista propietario", detalle = e.Message});
        }
        
    }
}