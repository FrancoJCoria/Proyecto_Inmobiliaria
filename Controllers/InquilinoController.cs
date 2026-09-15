using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;

[Authorize]
public class InquilinoController : Controller
{
    private readonly IRepositorioInquilino _repositorio;

    public InquilinoController(IRepositorioInquilino repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index(int pagina = 1)
    {
        if (pagina < 1) pagina = 1;
        int tamanoPagina = 5;

        var inquilinos = _repositorio.ObtenerTodos(pagina, tamanoPagina);
        ViewBag.PaginaActual = pagina;
        ViewData["Cantidad"] = inquilinos.Count();
        ViewBag.Otro = "Bienvenido al listado de inquilinos";

        return View(inquilinos);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var inquilino = _repositorio.ObtenerPorId(id);
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
        var inquilino = _repositorio.ObtenerPorId(id);
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
    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        var inquilino = _repositorio.ObtenerPorId(id);
        if (inquilino == null)
        {
            return NotFound();
        }
        return View(inquilino);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
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