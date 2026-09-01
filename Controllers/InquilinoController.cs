using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers;

public class InquilinoController : Controller
{
    private readonly IRepositorioInquilino _repositorio;

    public InquilinoController(IRepositorioInquilino repositorio)
    {
        _repositorio = repositorio;
    }


     // GET:  
    public IActionResult Index()
    {
        var inquilinos = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = inquilinos.Count();
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
    [ValidateAntiForgeryToken]
    public IActionResult Delete( int id)
    {
        if(id <= 0)
        {
            return BadRequest(new { error = "Se requiere un ID válido para dar de baja al inquilino." });
        }
        int filasAfectadas = _repositorio.Baja(id);
        if(filasAfectadas == 0)
        {
            return NotFound(new { error = $"No se encontró ningún inquilino con el ID {id}." });
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var inquilinos = _repositorio.ObtenerTodos();
        var inquilino = inquilinos.FirstOrDefault(i => i.Id_inquilino == id);
        if (inquilino == null)
        {
            return NotFound();
        }
        return View(inquilino);
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Inquilino inquilino)
    {
        if(inquilino == null || !ModelState.IsValid) 
        {
            return BadRequest(new { error = "Los datos del inquilino son requeridos." });
        }

        inquilino.Id_inquilino = id;
        int filasAfectadas = _repositorio.Modificacion(inquilino);
        if(filasAfectadas == 0)
        {
            return NotFound(new { error = $"No se encontró ningún inquilino con el ID {id}." });
        }
        return RedirectToAction("Index");
    }
    
}

