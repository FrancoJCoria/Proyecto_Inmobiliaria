using Microsoft.AspNetCore.Mvc;
using Proyecto_Inmobiliaria.Models;

namespace Proyecto_Inmobiliaria.Controllers;

public class InquilinoController : Controller
{
    public readonly IRepositorioInquilino _repositorio;

    public InquilinoController(IRepositorioInquilino repositorio)
    {
        _repositorio = repositorio;
    }


    public IActionResult Index()
    {
        var inquilinos = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = inquilinos.Count();
        ViewBag.Datos = new Inquilino { Id_inquilino = 1, Nombre = "Juan Pérez", Dni = "12345678" };
        ViewBag.Otro = "Bienvenido al listado de inquilinos";
        return View(inquilinos);
    }

    [HttpGet]
    public IActionResult Create()
		{
			return View();
		}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(IFormCollection form, Inquilino inquilino)
    {
        if (ModelState.IsValid) // Pregunta si el modelo es valido
        {
            int idGenerado = _repositorio.Alta(inquilino);
            if (idGenerado > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "No se pudo crear el inquilino.");
            }
        }
        return View(inquilino);
    }

    [HttpPatch]
    public IActionResult Delete([FromBody] Inquilino inquilino)
    {
        if(inquilino == null || string.IsNullOrEmpty(inquilino.Dni))
        {
            return BadRequest(new { error = "Se requiere el DNI para dar de baja al inquilino." });
        }
        int filasAfectadas = _repositorio.Baja(inquilino);
        if(filasAfectadas == 0)
        {
            return NotFound(new { error = $"No se encontró ningún inquilino con el DNI {inquilino.Dni}." });
        }
        return Ok(new
        {
            mensaje = "Inquilino dado de baja con éxito",
            filasAfectadas = filasAfectadas
        });
    }

    [HttpPut]
    public IActionResult Edit(int id, [FromBody] Inquilino inquilino)
    {
        if(inquilino == null)
        {
            return BadRequest(new { error = "Los datos del inquilino son requeridos." });
        }

        inquilino.Id_inquilino = id;
        int filasAfectadas = _repositorio.Modificacion(inquilino);
        if(filasAfectadas == 0)
        {
            return NotFound(new { error = $"No se encontró ningún inquilino con el ID {id}." });
        }
        return Ok(new
        {
            mensaje = "Inquilino modificado con éxito",
            filasAfectadas = filasAfectadas
        });
    }
    
}

