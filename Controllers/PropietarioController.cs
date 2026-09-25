using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;

[Authorize]
public class PropietarioController : Controller
{
    private readonly IRepositorioPropietario _repositorio;

    public PropietarioController(IRepositorioPropietario repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index(int pagina = 1)
    {
        const int tamanoPagina = 5;

        // Primero se pregunta cuantos hay en total, para saber cuantas paginas existen.
        int total = _repositorio.Contar();
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));

        // Si piden una pagina que no existe (por ejemplo ?pagina=999) se recorta a la ultima.
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var propietarios = _repositorio.ObtenerTodos(pagina, tamanoPagina);

        // El total que va en el titulo es el real, no la cantidad de filas de esta pagina.
        ViewData["Cantidad"] = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;

        return View(propietarios);
    }
    // Devuelve solo el id y la etiqueta porque es lo único que el <select> necesita.
    // El filtro se hace en el servidor: traer todos los propietarios y filtrar en el
    // cliente mostraría una lista incompleta sin chances de encontrar lo que se busca.
    [HttpGet]
    public IActionResult BuscarPropietarios(string? termino)
    {
        var lista = _repositorio.ObtenerTodos(termino);
        return Json(lista.Select(p => new
        {
            id = p.Id_propietario,
            etiqueta = $"{p.Apellido}, {p.Nombre} ({p.Dni})"
        }));
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
    
        if (idGenerado == -1)//para comprobar si el dni ya existe
        {
            ModelState.AddModelError("Dni", "Ya existe un propietario registrado con este DNI.");
            return View(propietario);
        }

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
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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
        return _repositorio.ObtenerPorId(id);
    }
}
