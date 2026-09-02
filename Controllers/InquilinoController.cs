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

    public IActionResult Index()
    {
        var inquilinos = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = inquilinos.Count();
        ViewBag.Otro = "Bienvenido al listado de inquilinos";
        return View(inquilinos);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var inquilinos = _repositorio.ObtenerTodos();
        var inquilino = inquilinos.FirstOrDefault(i => i.Id_inquilino == id);
        if (inquilino == null)
        {
            return NotFound();
        }
        return View(inquilino);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Inquilino inquilino)
    {
        if (ModelState.IsValid)
        {
            int idGenerado = _repositorio.Alta(inquilino);
            if (idGenerado > 0)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "No se pudo crear el inquilino.");
        }
        return View(inquilino);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Inquilino inquilino)
    {
        if (!ModelState.IsValid)
        {
            return View(inquilino);
        }

        inquilino.Id_inquilino = id;
        int filasAfectadas = _repositorio.Modificacion(inquilino);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el inquilino.");
            return View(inquilino);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var inquilinos = _repositorio.ObtenerTodos();
        var inquilino = inquilinos.FirstOrDefault(i => i.Id_inquilino == id);
        if (inquilino == null)
        {
            return NotFound();
        }
        return View(inquilino);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        int filasAfectadas = _repositorio.Baja(id);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }
}
