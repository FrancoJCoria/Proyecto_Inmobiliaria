using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;

[Authorize]
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

    public IActionResult Index(int pagina = 1, int? idPropietario = null, bool? disponible = null, bool? estado = null)
    {
        const int tamanoPagina = 5;

        // Total real de inmuebles según los filtros, para calcular cuántas páginas hay.
        int total = _repositorio.Contar(idPropietario, disponible, estado);
        int totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina);
        if (totalPaginas < 1)
        {
            totalPaginas = 1;
        }

        // Se ajusta la página solicitada para que nunca esté fuera del rango válido.
        if (pagina < 1)
        {
            pagina = 1;
        }
        if (pagina > totalPaginas)
        {
            pagina = totalPaginas;
        }

        // Se trae SOLO la página actual (LIMIT/OFFSET en el servidor), no todo el listado.
        var inmuebles = _repositorio.ObtenerTodos(pagina, tamanoPagina, idPropietario, disponible, estado);
        ViewData["Cantidad"] = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.FiltroPropietario = idPropietario;
        ViewBag.FiltroDisponible = disponible;
        ViewBag.FiltroEstado = estado;
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
        inmueble.Imagenes = _repositorioImagen.BuscarPorInmueble(id);
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

        inmueble.Estado = true;
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
    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        var inmueble = _repositorio.ObtenerPorId(id);
        if (inmueble == null)
        {
            return NotFound();
        }
        return View(inmueble);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
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

    public IActionResult Informes()
    {
        return View();
    }

    // Informe: inmuebles que le corresponden a un propietario específico.
    // El propietario se elige de un desplegable con búsqueda en el servidor, no de una
    // lista con todos los propietarios cargados.
    public IActionResult PorPropietario(int? idPropietario, string? busquedaPropietario, int pagina = 1)
    {
        const int tamanoPagina = 5;

        // El buscador del desplegable se resuelve en el servidor.
        ViewBag.Propietarios = _repositorioPropietario.ObtenerTodos(busquedaPropietario);
        ViewBag.BusquedaPropietario = busquedaPropietario;

        // Un id 0 o negativo viene del placeholder "-- Todos --" y no es un filtro real.
        if (idPropietario.HasValue && idPropietario.Value <= 0)
        {
            idPropietario = null;
        }

        int total = _repositorio.Contar(idPropietario);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var inmuebles = _repositorio.ObtenerTodos(pagina, tamanoPagina, idPropietario);
        ViewData["Cantidad"] = total;
        ViewBag.PropietarioSeleccionado = idPropietario;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(inmuebles);
    }

    public IActionResult MasReservados(int pagina = 1)
    {
        const int tamanoPagina = 5;

        int total = _repositorio.ContarMasReservados();
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        // Los nombres de propietario y tipo vienen en el JOIN de la consulta, así que
        // este informe no carga la lista completa de propietarios ni de tipos.
        var inmuebles = _repositorio.ObtenerMasReservados(pagina, tamanoPagina);
        ViewData["Cantidad"] = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(inmuebles);
    }

    public IActionResult MenosReservados(int? dias, int pagina = 1)
    {
        const int tamanoPagina = 5;

        int plazo = dias ?? 30;
        if (plazo < 1)
        {
            plazo = 30;
        }

        int total = _repositorio.ContarMenosReservados(plazo);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var inmuebles = _repositorio.ObtenerMenosReservados(plazo, pagina, tamanoPagina);
        ViewData["Cantidad"] = total;
        ViewData["Dias"] = plazo;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(inmuebles);
    }

    [HttpGet]
    public IActionResult BuscarDisponibles(DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1)
    {
        const int tamanoPagina = 5;

        if (!fechaInicio.HasValue || !fechaFin.HasValue)
        {
            return View(new List<Inmueble>());
        }

        if (fechaInicio > fechaFin)
        {
            ViewBag.Error = "La fecha de inicio no puede ser posterior a la fecha de fin.";
            return View(new List<Inmueble>());
        }

        ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
        ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

        int total = _repositorio.ContarDisponiblesPorFechas(fechaInicio.Value, fechaFin.Value);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var lista = _repositorio.BuscarDisponiblesPorFechas(fechaInicio.Value, fechaFin.Value, pagina, tamanoPagina);
        ViewData["Cantidad"] = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(lista);
    }


}
