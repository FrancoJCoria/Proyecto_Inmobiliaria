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

    public IActionResult Index()
    {
        var reservas = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = reservas.Count();
        CargarListas();
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
        CargarListas();
        return View(reserva);
    }

    [HttpGet]
    public IActionResult Create()
    {
        CargarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Reserva reserva)
    {
        if (!ModelState.IsValid)
        {
            CargarListas();
            return View(reserva);
        }

        if (reserva.Fecha_fin < reserva.Fecha_inicio)
        {
            ModelState.AddModelError("Fecha_fin", "La fecha de fin no puede ser anterior a la de inicio.");
            CargarListas();
            return View(reserva);
        }

        reserva.Id_usuario_creador = ObtenerIdUsuarioLogueado() ?? 0;
        int idGenerado = _repositorio.Alta(reserva);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear la reserva.");
            CargarListas();
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
        CargarListas();
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Reserva reserva)
    {
        if (!ModelState.IsValid)
        {
            CargarListas();
            return View(reserva);
        }

        if (reserva.Fecha_fin < reserva.Fecha_inicio)
        {
            ModelState.AddModelError("Fecha_fin", "La fecha de fin no puede ser anterior a la de inicio.");
            CargarListas();
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
            CargarListas();
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

    private void CargarListas()
    {
        ViewBag.Inquilinos = _repositorioInquilino.ObtenerTodos();
        ViewBag.Usuarios = _repositorioUsuario.ObtenerTodos();
        ViewBag.Inmuebles = _repositorioInmueble.ObtenerTodos();
    }
}
