using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers;

public class InmuebleController : Controller
{
    private readonly IRepositorioInmueble _repositorio;
    private readonly IRepositorioPropietario _repositorioPropietario;
    private readonly IRepositorioTipoInmueble _repositorioTipo;

    public InmuebleController(IRepositorioInmueble repositorio, IRepositorioPropietario repositorioPropietario, IRepositorioTipoInmueble repositorioTipo)
    {
        _repositorio = repositorio;
        _repositorioPropietario = repositorioPropietario;
        _repositorioTipo = repositorioTipo;
    }

    public IActionResult Index()
    {
        var inmuebles = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = inmuebles.Count();
        CargarListas();
        return View(inmuebles);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var inmueble = _repositorio.ObtenerPorId(id);
        if (inmueble == null)
        {
            return NotFound();
        }
        CargarListas();
        return View(inmueble);
    }

    [HttpGet]
    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Inmueble inmueble)
    {
        if (!ModelState.IsValid)
        {
            CargarListas();
            return View(inmueble);
        }

        inmueble.Estado = "Activo";
        int idGenerado = _repositorio.Alta(inmueble);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el inmueble.");
            CargarListas();
            return View(inmueble);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var inmueble = _repositorio.ObtenerPorId(id);
        if (inmueble == null)
        {
            return NotFound();
        }
        CargarListas();
        return View(inmueble);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Inmueble inmueble)
    {
        if (!ModelState.IsValid)
        {
            CargarListas();
            return View(inmueble);
        }

        inmueble.Id_inmueble = id;
        int filasAfectadas = _repositorio.Modificacion(inmueble);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el inmueble.");
            CargarListas();
            return View(inmueble);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var inmueble = _repositorio.ObtenerPorId(id);
        if (inmueble == null)
        {
            return NotFound();
        }
        CargarListas();
        return View(inmueble);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        int filasAfectadas = _repositorio.Baja(id);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }

    private void CargarListas()
    {
        ViewBag.Propietarios = _repositorioPropietario.ObtenerTodos();
        ViewBag.Tipos = _repositorioTipo.ObtenerTodos();
    }
}
