using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace Inmobiliaria.Controllers;

[Authorize]
public class ReservaController : Controller
{
    private readonly IRepositorioReserva _repositorio;
    private readonly IRepositorioInquilino _repositorioInquilino;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioInmueble _repositorioInmueble;
    private readonly IRepositorioPago _repositorioPago;

    public ReservaController(IRepositorioReserva repositorio, IRepositorioInquilino repositorioInquilino, IRepositorioUsuario repositorioUsuario, IRepositorioInmueble repositorioInmueble, IRepositorioPago repositorioPago)
    {
        _repositorio = repositorio;
        _repositorioInquilino = repositorioInquilino;
        _repositorioUsuario = repositorioUsuario;
        _repositorioInmueble = repositorioInmueble;
        _repositorioPago = repositorioPago;
    }

    public IActionResult Index(int pagina = 1)
    {
        const int tamanoPagina = 5;

        int total = _repositorio.Contar();
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        // El nombre del inmueble y del inquilino ya viene en el JOIN de la consulta,
        // así que esta vista no necesita ninguna de las listas de los desplegables.
        var reservas = _repositorio.ObtenerTodos(pagina, tamanoPagina);
        ViewData["Cantidad"] = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(reservas);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var reserva = _repositorio.ObtenerPorId(id);
        if (reserva == null)
        {
            return NotFound();
        }
        CargarUsuariosDeDetails(reserva);
        return View(reserva);
    }

    [HttpGet]
    public IActionResult Create()
    {
        CargarSeleccionado(null);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Reserva reserva)
    {
        if (!ModelState.IsValid)
        {
            CargarSeleccionado(reserva);
            return View(reserva);
        }

        if (reserva.Fecha_fin < reserva.Fecha_inicio)
        {
            ModelState.AddModelError("Fecha_fin", "La fecha de fin no puede ser anterior a la de inicio.");
            CargarSeleccionado(reserva);
            return View(reserva);
        }

        reserva.Id_usuario_creador = ObtenerIdUsuarioLogueado() ?? 0;
        int idGenerado = _repositorio.Alta(reserva);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear la reserva.");
            CargarSeleccionado(reserva);
            return View(reserva);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var reserva = _repositorio.ObtenerPorId(id);
        if (reserva == null)
        {
            return NotFound();
        }
        CargarSeleccionado(reserva);
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Reserva reserva)
    {
        if (!ModelState.IsValid)
        {
            CargarSeleccionado(reserva);
            return View(reserva);
        }

        if (reserva.Fecha_fin < reserva.Fecha_inicio)
        {
            ModelState.AddModelError("Fecha_fin", "La fecha de fin no puede ser anterior a la de inicio.");
            CargarSeleccionado(reserva);
            return View(reserva);
        }

        var original = _repositorio.ObtenerPorId(id);
        if (original == null)
        {
            return NotFound();
        }

        reserva.Id_reserva = id;
        reserva.Id_usuario_creador = original.Id_usuario_creador;
        reserva.Id_usuario_finalizador = original.Id_usuario_finalizador;
        int filasAfectadas = _repositorio.Modificacion(reserva);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar la reserva.");
            CargarSeleccionado(reserva);
            return View(reserva);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        var reserva = _repositorio.ObtenerPorId(id);
        if (reserva == null)
        {
            return NotFound();
        }
        return View(reserva);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public IActionResult DeleteConfirmado(int id)
    {
        var reserva = _repositorio.ObtenerPorId(id);
        if (reserva == null)
        {
            return NotFound();
        }

        reserva.Estado = false;
        int filasAfectadas = _repositorio.Baja(reserva);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Finalizar(int id)
    {
        var reserva = _repositorio.ObtenerPorId(id);
        if (reserva == null)
        {
            return NotFound();
        }

        if (YaFinalizada(reserva))
        {
            TempData["Error"] = "La reserva ya fue finalizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        DateTime fechaSugerida = DateTime.Today;
        if (fechaSugerida <= reserva.Fecha_inicio)
        {
            fechaSugerida = reserva.Fecha_inicio.AddDays(1);
        }
        if (fechaSugerida >= reserva.Fecha_fin)
        {
            fechaSugerida = reserva.Fecha_fin.AddDays(-1);
        }

        ViewBag.Multa = CalcularMulta(reserva, fechaSugerida);
        ViewBag.FechaEfectiva = fechaSugerida;
        return View(reserva);
    }

    [HttpPost]
    [ActionName("Finalizar")]
    [ValidateAntiForgeryToken]
    public IActionResult FinalizarConfirmado(int id, DateTime? fechaFinEfectiva, string? accion)
    {
        var reserva = _repositorio.ObtenerPorId(id);
        if (reserva == null)
        {
            return NotFound();
        }

        if (YaFinalizada(reserva))
        {
            TempData["Error"] = "La reserva ya fue finalizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (fechaFinEfectiva == null)
        {
            ModelState.AddModelError("", "Debe indicar la fecha en que se hará efectiva la terminación.");
            ViewBag.Multa = 0m;
            return View(reserva);
        }

        if (fechaFinEfectiva <= reserva.Fecha_inicio)
        {
            ModelState.AddModelError("", "La fecha de terminación debe ser posterior a la fecha de inicio.");
            ViewBag.Multa = CalcularMulta(reserva, fechaFinEfectiva.Value);
            ViewBag.FechaEfectiva = fechaFinEfectiva.Value;
            return View(reserva);
        }

        if (fechaFinEfectiva >= reserva.Fecha_fin)
        {
            ModelState.AddModelError("", "La fecha de terminación debe ser anterior a la fecha de fin original: se trata de una terminación anticipada.");
            ViewBag.Multa = 0m;
            ViewBag.FechaEfectiva = fechaFinEfectiva.Value;
            return View(reserva);
        }

        decimal multa = CalcularMulta(reserva, fechaFinEfectiva.Value);
        ViewBag.Multa = multa;
        ViewBag.FechaEfectiva = fechaFinEfectiva.Value;

        // Si se pidió solo "calcular", se informa la multa sin finalizar ni registrar nada.
        if (accion == "calcular")
        {
            return View(reserva);
        }

        int idUsuarioFinalizador = ObtenerIdUsuarioLogueado() ?? 0;

        reserva.Fecha_fin_efectiva = fechaFinEfectiva.Value;
        reserva.Id_usuario_finalizador = idUsuarioFinalizador;
        int filasAfectadas = _repositorio.Modificacion(reserva);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo finalizar la reserva.");
            return View(reserva);
        }

        var pagoMulta = new Pago
        {
            Concepto = "Multa por terminación anticipada",
            Fecha_pago = fechaFinEfectiva.Value,
            Importe = multa,
            Estado = true,
            Id_reserva = reserva.Id_reserva,
            Id_usuario_creador = idUsuarioFinalizador
        };
        int idPago = _repositorioPago.Alta(pagoMulta);

        if (idPago == 0)
        {
            TempData["Error"] = "La reserva se finalizó pero no se pudo registrar la multa como pago.";
        }
        else
        {
            TempData["Mensaje"] = $"Reserva finalizada. Se registró la multa de {multa.ToString("C")} como pago.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

public IActionResult Informes()
    {
        return View();
    }

    public IActionResult Vigentes(DateTime? desde, DateTime? hasta, int pagina = 1)
    {
        const int tamanoPagina = 5;

        DateTime desdeFecha = desde ?? DateTime.Today.AddMonths(-1);
        DateTime hastaFecha = hasta ?? DateTime.Today.AddMonths(1);
        if (hastaFecha < desdeFecha)
        {
            hastaFecha = desdeFecha;
        }

        int total = _repositorio.ContarVigentes(desdeFecha, hastaFecha);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        // El nombre del inmueble y del inquilino vienen en el JOIN de la consulta, así
        // que estos informes no cargan la lista completa de inmuebles ni de inquilinos.
        var reservas = _repositorio.ObtenerVigentes(desdeFecha, hastaFecha, pagina, tamanoPagina);
        ViewData["Cantidad"] = total;
        ViewData["Desde"] = desdeFecha;
        ViewData["Hasta"] = hastaFecha;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(reservas);
    }

    public IActionResult PorTerminar(int? dias, int pagina = 1)
    {
        const int tamanoPagina = 5;

        int plazo = dias ?? 30;
        if (plazo < 1)
        {
            plazo = 30;
        }

        int total = _repositorio.ContarPorTerminar(plazo);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var reservas = _repositorio.ObtenerPorTerminar(plazo, pagina, tamanoPagina);
        ViewData["Cantidad"] = total;
        ViewData["Dias"] = plazo;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        return View(reservas);
    }

    private static decimal CalcularMulta(Reserva reserva, DateTime fechaFinEfectiva)
    {
        int duracionOriginal = (reserva.Fecha_fin.Date - reserva.Fecha_inicio.Date).Days;
        int diasTranscurridos = (fechaFinEfectiva.Date - reserva.Fecha_inicio.Date).Days;
        int diasRestantes = (reserva.Fecha_fin.Date - fechaFinEfectiva.Date).Days;
        if (duracionOriginal <= 0 || diasRestantes <= 0 || diasTranscurridos <= 0)
        {
            return 0m;
        }

        decimal montoRestante = diasRestantes * reserva.Monto_diario;
        bool cumpleMenosDeLaMitad = (double)diasTranscurridos / duracionOriginal < 0.5;
        decimal porcentaje = cumpleMenosDeLaMitad ? 0.5m : 0.25m;
        return montoRestante * porcentaje;
    }

    private static bool YaFinalizada(Reserva reserva)
    {
        return reserva.Fecha_fin_efectiva != DateTime.MinValue || reserva.Id_usuario_finalizador != 0;
    }

    private int? ObtenerIdUsuarioLogueado()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    // Los desplegables de inmueble e inquilino se resuelven por AJAX contra BuscarInmuebles
    // y BuscarInquilinos: la vista nunca recibe el catálogo completo. Lo único que se trae
    // por id es la etiqueta de lo que ya estaba elegido, para que Edit muestre el valor
    // vigente y Create no muestre nada. Son consultas por clave primaria, no listados.
    private void CargarSeleccionado(Reserva? reserva)
    {
        ViewBag.InmuebleSeleccionado = null;
        ViewBag.InquilinoSeleccionado = null;

        if (reserva == null)
        {
            return;
        }

        // Las etiquetas se arman con una consulta por id, y no con el nombre del JOIN de
        // la reserva, para que coincidan exactamente con las que devuelve BuscarInmuebles
        // y BuscarInquilinos. Si no coincidieran, al elegir una opcion por busqueda la
        // etiqueta del desplegable cambiaria sola.
        if (reserva.Id_inmueble > 0)
        {
            var inmueble = _repositorioInmueble.ObtenerPorId(reserva.Id_inmueble);
            if (inmueble != null)
            {
                ViewBag.InmuebleSeleccionado = new OpcionDesplegable
                {
                    Id = inmueble.Id_inmueble,
                    Etiqueta = $"{inmueble.Direccion} (cupo {inmueble.Cupo})"
                };
            }
        }

        if (reserva.Id_inquilino > 0)
        {
            var inquilino = _repositorioInquilino.ObtenerPorId(reserva.Id_inquilino);
            if (inquilino != null)
            {
                ViewBag.InquilinoSeleccionado = new OpcionDesplegable
                {
                    Id = inquilino.Id_inquilino,
                    Etiqueta = $"{inquilino.Apellido}, {inquilino.Nombre} ({inquilino.Dni})"
                };
            }
        }
    }

    // En Details solo hacen falta los nombres de los dos usuarios que tocaron la reserva,
    // asi que se consultan por id en vez de bajar la tabla Usuario entera para usar dos
    // filas en la vista.
    private void CargarUsuariosDeDetails(Reserva reserva)
    {
        ViewBag.UsuarioCreador = ObtenerNombreUsuario(reserva.Id_usuario_creador);
        ViewBag.UsuarioFinalizador = ObtenerNombreUsuario(reserva.Id_usuario_finalizador);
    }

    private string? ObtenerNombreUsuario(int idUsuario)
    {
        if (idUsuario <= 0)
        {
            return null;
        }

        var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);
        return usuario == null ? null : $"{usuario.Apellido}, {usuario.Nombre}";
    }

    //------------------------------------------------------------------------------------------BUSQUEDA EN LOS DESPLEGABLES------------------------------------------//
    // Devuelven solo el id y la etiqueta porque es lo único que el <select> necesita.
    // El filtro se hace en el servidor: si se paginara en el cliente, el usuario vería
    // una lista incompleta sin chances de encontrar lo que busca.
    [HttpGet]
    public IActionResult BuscarInquilinos(string? termino)
    {
        if (!BusquedaDesplegable.EsTerminoUtil(termino))
        {
            return Json(Array.Empty<object>());
        }

        var lista = _repositorioInquilino.ObtenerTodos(termino!.Trim(), BusquedaDesplegable.MaximoOpciones);
        return Json(lista.Select(i => new
        {
            id = i.Id_inquilino,
            etiqueta = $"{i.Apellido}, {i.Nombre} ({i.Dni})"
        }));
    }

    [HttpGet]
    public IActionResult BuscarInmuebles(string? termino)
    {
        if (!BusquedaDesplegable.EsTerminoUtil(termino))
        {
            return Json(Array.Empty<object>());
        }

        var lista = _repositorioInmueble.ObtenerTodos(termino!.Trim(), BusquedaDesplegable.MaximoOpciones);
        return Json(lista.Select(m => new
        {
            id = m.Id_inmueble,
            etiqueta = $"{m.Direccion} (cupo {m.Cupo})"
        }));
    }
}
