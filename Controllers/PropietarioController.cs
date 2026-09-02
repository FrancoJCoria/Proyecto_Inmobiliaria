using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers;

public class PropietarioController : Controller
{
    private readonly IRepositorioPropietario _repositorio;

    public PropietarioController(IRepositorioPropietario repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index()
    {
        var propietarios = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = propietarios.Count();
        return View(propietarios);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var propietario = BuscarPorId(id);
        if (propietario == null)
        {
            return NotFound();
        }
        return View(propietario);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Propietario propietario)
    {
        if (!ModelState.IsValid)
        {
            return View(propietario);
        }

        int idGenerado = _repositorio.Alta(propietario);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el propietario.");
            return View(propietario);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var propietario = BuscarPorId(id);
        if (propietario == null)
        {
            return NotFound();
        }
        return View(propietario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Propietario propietario)
    {
        if (!ModelState.IsValid)
        {
            return View(propietario);
        }

        propietario.Id_propietario = id;
        int filasAfectadas = _repositorio.Modificacion(propietario);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el propietario.");
            return View(propietario);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var propietario = BuscarPorId(id);
        if (propietario == null)
        {
            return NotFound();
        }
        return View(propietario);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        var propietario = BuscarPorId(id);
        if (propietario == null)
        {
            return NotFound();
        }

        propietario.Estado = false;
        int filasAfectadas = _repositorio.Baja(propietario);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }

    private Propietario? BuscarPorId(int id)
    {
        return _repositorio.ObtenerTodos().FirstOrDefault(p => p.Id_propietario == id);
    }
}
