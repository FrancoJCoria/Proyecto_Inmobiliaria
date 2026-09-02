using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers;

public class UsuarioController : Controller
{
    private readonly IRepositorioUsuario _repositorio;

    public UsuarioController(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index()
    {
        var usuarios = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = usuarios.Count();
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Usuario usuario)
    {
        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        if (_repositorio.ObetenerPorEmail(usuario.Email) != null)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
            return View(usuario);
        }

        usuario.Estado = true;
        int idGenerado = _repositorio.Alta(usuario);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el usuario.");
            return View(usuario);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Usuario usuario)
    {
        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        usuario.Id_usuario = id;
        int filasAfectadas = _repositorio.Modificacion(usuario);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el usuario. Puede estar dado de baja.");
            return View(usuario);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmado(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Estado = false;
        int filasAfectadas = _repositorio.Baja(usuario);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }
}
