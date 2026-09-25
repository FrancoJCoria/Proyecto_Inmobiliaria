using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
namespace Inmobiliaria.Controllers;

[Authorize]
public class InmuebleController : Controller
{
    private readonly IRepositorioInmueble _repositorio;
    private readonly IRepositorioPropietario _repositorioPropietario;
    private readonly IRepositorioImagenInmueble _repositorioImagen;

    // No se inyecta IRepositorioTipoInmueble porque el nombre del tipo ya viene en el
    // LEFT JOIN de ObtenerPorId: ningun desplegable de este controller lo necesita.
    public InmuebleController(IRepositorioInmueble repositorio, IRepositorioPropietario repositorioPropietario, IRepositorioImagenInmueble repositorioImagen)
    {
        _repositorio = repositorio;
        _repositorioPropietario = repositorioPropietario;
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
        ViewBag.FiltroDisponible = disponible;
        ViewBag.FiltroEstado = estado;
        CargarPropietarioDelFiltro(idPropietario);
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
        CargarSeleccionado(null);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Inmueble inmueble)
    {
        if (!ModelState.IsValid)
        {
            CargarSeleccionado(inmueble);
            return View(inmueble);
        }

        inmueble.Estado = true;
        int idGenerado = _repositorio.Alta(inmueble);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el inmueble.");
            CargarSeleccionado(inmueble);
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
        CargarSeleccionado(inmueble);
        return View(inmueble);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Inmueble inmueble)
    {
        if (!ModelState.IsValid)
        {
            CargarSeleccionado(inmueble);
            return View(inmueble);
        }

        inmueble.Id_inmueble = id;
        int filasAfectadas = _repositorio.Modificacion(inmueble);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el inmueble.");
            CargarSeleccionado(inmueble);
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

    // Los desplegables de propietario y tipo se resuelven por AJAX contra BuscarPropietarios
    // y BuscarTipos: la vista nunca recibe el catálogo completo. Lo único que se trae por id
    // es la etiqueta de lo que ya estaba elegido, para que Edit muestre el valor vigente y
    // Create no muestre nada. Son consultas por clave primaria, no listados.
    private void CargarSeleccionado(Inmueble? inmueble)
    {
        ViewBag.PropietarioSeleccionado = null;
        ViewBag.TipoSeleccionado = null;

        if (inmueble == null)
        {
            return;
        }

        // El nombre del tipo ya viene en el LEFT JOIN de ObtenerPorId, asi que no hace
        // falta consultar TipoInmueble.
        if (inmueble.Id_tipo > 0 && !string.IsNullOrEmpty(inmueble.NombreTipo))
        {
            ViewBag.TipoSeleccionado = new OpcionDesplegable
            {
                Id = inmueble.Id_tipo,
                Etiqueta = inmueble.NombreTipo
            };
        }

        // Del propietario si hace falta el DNI para armar la misma etiqueta que arma
        // BuscarPropietarios, asi que se resuelve con una consulta por id.
        if (inmueble.Id_propietario > 0)
        {
            var propietario = _repositorioPropietario.ObtenerPorId(inmueble.Id_propietario);
            if (propietario != null)
            {
                ViewBag.PropietarioSeleccionado = new OpcionDesplegable
                {
                    Id = propietario.Id_propietario,
                    Etiqueta = $"{propietario.Apellido}, {propietario.Nombre} ({propietario.Dni})"
                };
            }
        }
    }

    public IActionResult Informes()
    {
        return View();
    }

    // El desplegable de propietario del filtro y del informe se resuelve por AJAX contra
    // BuscarPropietarios. Acá solo se trae la etiqueta del propietario que ya venia
    // elegido en la url, para que el filtro muestre la seleccion vigente.
    private void CargarPropietarioDelFiltro(int? idPropietario)
    {
        ViewBag.PropietarioSeleccionado = null;

        // Un id 0 o negativo viene del placeholder "-- Todos --" y no es un filtro real.
        if (!idPropietario.HasValue || idPropietario.Value <= 0)
        {
            return;
        }

        var propietario = _repositorioPropietario.ObtenerPorId(idPropietario.Value);
        if (propietario != null)
        {
            ViewBag.PropietarioSeleccionado = new OpcionDesplegable
            {
                Id = propietario.Id_propietario,
                Etiqueta = $"{propietario.Apellido}, {propietario.Nombre}"
            };
        }
    }

    // Informe: inmuebles que le corresponden a un propietario específico.
    // El propietario se elige de un desplegable con búsqueda en el servidor, no de una
    // lista con todos los propietarios cargados.
    public IActionResult PorPropietario(int? idPropietario, int pagina = 1)
    {
        const int tamanoPagina = 5;

        // Un id 0 o negativo viene del placeholder "-- Todos --" y no es un filtro real.
        if (idPropietario.HasValue && idPropietario.Value <= 0)
        {
            idPropietario = null;
        }

        CargarPropietarioDelFiltro(idPropietario);

        int total = _repositorio.Contar(idPropietario);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var inmuebles = _repositorio.ObtenerTodos(pagina, tamanoPagina, idPropietario);
        ViewData["Cantidad"] = total;
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
