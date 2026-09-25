using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;

[Authorize]
public class TipoInmuebleController : Controller
{
    private readonly IRepositorioTipoInmueble _repositorio;

    public TipoInmuebleController(IRepositorioTipoInmueble repositorio)
    {
        _repositorio = repositorio;
    }

    // La página de tipos muestra la tabla completa, así que acá se pide un tope alto a
    // propósito. Esto no es un desplegable: es el contenido de la página. Los tipos son
    // una tabla chica de consulta, por eso el número puede ser generoso.
    private const int TopoParaElListadoCompleto = 200;

    public IActionResult Index()
    {
        var tipos = _repositorio.ObtenerTodos(null, TopoParaElListadoCompleto);
        ViewData["Cantidad"] = tipos.Count();
        return View(tipos);
    }

    // Devuelve solo el id y la etiqueta porque es lo único que el <select> necesita.
    [HttpGet]
    public IActionResult BuscarTipos(string? termino)
    {
        if (!BusquedaDesplegable.EsTerminoUtil(termino))
        {
            return Json(Array.Empty<object>());
        }

        var tipos = _repositorio.ObtenerTodos(termino!.Trim(), BusquedaDesplegable.MaximoOpciones);
        return Json(tipos.Select(t => new
        {
            id = t.Id_tipo,
            etiqueta = t.Nombre
        }));
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
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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