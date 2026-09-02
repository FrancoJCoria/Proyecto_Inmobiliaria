using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using MySqlConnector;

namespace Inmobiliaria.Controllers;

public class TipoInmuebleController : Controller
{
    private readonly IRepositorioTipoInmueble _repositorio;

    public TipoInmuebleController(IRepositorioTipoInmueble repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index()
    {
        var tipos = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = tipos.Count();
        return View(tipos);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TipoInmueble tipo)
    {
        if (!ModelState.IsValid)
        {
            return View(tipo);
        }

        int idGenerado = _repositorio.Alta(tipo);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el tipo de inmueble.");
            return View(tipo);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var tipo = _repositorio.ObtenerPorId(id);
        if (tipo == null)
        {
            return NotFound();
        }
        return View(tipo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TipoInmueble tipo)
    {
        if (!ModelState.IsValid)
        {
            return View(tipo);
        }

        tipo.Id_tipo = id;
        int filasAfectadas = _repositorio.Modificacion(tipo);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el tipo de inmueble.");
            return View(tipo);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var tipo = _repositorio.ObtenerPorId(id);
        if (tipo == null)
        {
            return NotFound();
        }
        return View(tipo);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        var tipo = _repositorio.ObtenerPorId(id);
        if (tipo == null)
        {
            return NotFound();
        }

        try
        {
            _repositorio.Baja(id);
        }
        catch (MySqlException)
        {
            ViewBag.Error = "No se puede eliminar este tipo porque hay inmuebles que lo usan.";
            return View("Delete", tipo);
        }
        return RedirectToAction("Index");
    }
}
