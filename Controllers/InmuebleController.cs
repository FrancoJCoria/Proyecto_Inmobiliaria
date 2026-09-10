using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers;

public class InmuebleController : Controller
{
    private readonly IRepositorioInmueble _repositorio;
    private readonly IRepositorioPropietario _repositorioPropietario;
    private readonly IRepositorioTipoInmueble _repositorioTipo;
    private readonly IRepositorioImagenInmueble _repositorioImagen;

    public InmuebleController(IRepositorioInmueble repositorio, IRepositorioPropietario repositorioPropietario, IRepositorioTipoInmueble repositorioTipo, IRepositorioImagenInmueble repositorioImagen)
    {
        _repositorio = repositorio;
        _repositorioPropietario = repositorioPropietario;
        _repositorioTipo = repositorioTipo;
        _repositorioImagen = repositorioImagen;
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

    [HttpGet]
    public IActionResult Imagenes(int id)
    {
        var inmueble = _repositorio.ObtenerPorId(id);
        if (inmueble == null)
        {
            return NotFound();
        }
        inmueble.Imagenes = _repositorioImagen.BuscarPorInmueble(id);
        CargarListas();
        return View(inmueble);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Portada(Inmueble entidad, [FromServices] IWebHostEnvironment environment)
    {
        try
        {
            var inmueble = _repositorio.ObtenerPorId(entidad.Id_inmueble);
            if (inmueble == null)
            {
                return NotFound();
            }

            if (inmueble.Portada != null)
            {
                string rutaEliminar = Path.Combine(environment.WebRootPath, "Uploads", "Inmuebles",
                    Path.GetFileName(inmueble.Portada));
                if (System.IO.File.Exists(rutaEliminar))
                    System.IO.File.Delete(rutaEliminar);
            }

            if (entidad.PortadaFile != null)
            {
                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "Uploads");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, "Inmuebles");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fileName = "portada_" + entidad.Id_inmueble + Path.GetExtension(entidad.PortadaFile.FileName);
                string rutaFisicaCompleta = Path.Combine(path, fileName);

                using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
                {
                    entidad.PortadaFile.CopyTo(stream);
                }

                entidad.Portada = Path.Combine("/Uploads/Inmuebles", fileName);
            }
            else
            {
                entidad.Portada = string.Empty;
            }

            _repositorio.ModificarPortada(entidad.Id_inmueble, entidad.Portada);
            TempData["Mensaje"] = "Portada actualizada correctamente";
            return RedirectToAction(nameof(Imagenes), new { id = entidad.Id_inmueble });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction(nameof(Imagenes), new { id = entidad.Id_inmueble });
        }
    }

    private void CargarListas()
    {
        ViewBag.Propietarios = _repositorioPropietario.ObtenerTodos();
        ViewBag.Tipos = _repositorioTipo.ObtenerTodos();
    }
}
