using System.Security.Claims;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers;

[Authorize]
public class PagoController : Controller
{
    private readonly IRepositorioPago _repositorio;
    private readonly IRepositorioReserva _repositorioReserva;
    private readonly IRepositorioUsuario _repositorioUsuario;

    public PagoController(IRepositorioPago repositorio, IRepositorioReserva repositorioReserva, IRepositorioUsuario repositorioUsuario)
    {
        _repositorio = repositorio;
        _repositorioReserva = repositorioReserva;
        _repositorioUsuario = repositorioUsuario;
    }

    public IActionResult Index(int idReserva)
    {
        if (idReserva <= 0)
        {
            return RedirectToAction("Index", "Reserva");
        }

        var reserva = _repositorioReserva.ObtenerPorId(idReserva);
        if (reserva == null)
        {
            return NotFound();
        }

        var pagos = _repositorio.ObtenerPorReserva(idReserva);
        ViewBag.Reserva = reserva;
        ViewData["Cantidad"] = pagos.Count();
        CargarListas();
        return View(pagos);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var pago = _repositorio.ObtenerPorId(id);
        if (pago == null)
        {
            return NotFound();
        }
        CargarListas();
        return View(pago);
    }

    [HttpGet]
    public IActionResult Create(int idReserva = 0)
    {
        Reserva? reserva = null;
        if (idReserva > 0)
        {
            reserva = _repositorioReserva.ObtenerPorId(idReserva);
            if (reserva == null)
            {
                return NotFound();
            }
        }
        ViewBag.Reserva = reserva;
        CargarListas();
        return View(new Pago { Id_reserva = idReserva });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Pago pago)
    {
        var reserva = _repositorioReserva.ObtenerPorId(pago.Id_reserva);
        if (reserva == null)
        {
            ModelState.AddModelError("Id_reserva", "Debe elegir una reserva válida.");
        }
        else
        {
            ViewBag.Reserva = reserva;
        }

        if (!ModelState.IsValid)
        {
            CargarListas();
            return View(pago);
        }

        pago.Estado = true;
        pago.Id_usuario_creador = ObtenerIdUsuarioLogueado() ?? 0;
        int idGenerado = _repositorio.Alta(pago);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el pago.");
            CargarListas();
            return View(pago);
        }
        return RedirectToAction("Index", new { idReserva = pago.Id_reserva });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var pago = _repositorio.ObtenerPorId(id);
        if (pago == null)
        {
            return NotFound();
        }
        CargarListas();
        return View(pago);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Pago pago)
    {
        var original = _repositorio.ObtenerPorId(id);
        if (original == null)
        {
            return NotFound();
        }

        if (!original.Estado)
        {
            TempData["Error"] = "Un pago anulado no se puede modificar.";
            return RedirectToAction("Index", new { idReserva = original.Id_reserva });
        }

        if (string.IsNullOrWhiteSpace(pago.Concepto))
        {
            ModelState.AddModelError("Concepto", "El concepto es obligatorio.");
            CargarListas();
            return View(original);
        }

        pago.Id_pago = id;
        pago.Id_reserva = original.Id_reserva;
        pago.Fecha_pago = original.Fecha_pago;
        pago.Importe = original.Importe;
        pago.Estado = original.Estado;
        pago.Id_usuario_creador = original.Id_usuario_creador;
        pago.Id_usuario_anulador = original.Id_usuario_anulador;

        int filasAfectadas = _repositorio.Modificacion(pago);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el pago.");
            CargarListas();
            return View(original);
        }
        return RedirectToAction("Index", new { idReserva = pago.Id_reserva });
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Anular(int id)
    {
        var pago = _repositorio.ObtenerPorId(id);
        if (pago == null)
        {
            return NotFound();
        }
        CargarListas();
        return View(pago);
    }

    [HttpPost]
    [ActionName("Anular")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public IActionResult AnularConfirmado(int id)
    {
        var pago = _repositorio.ObtenerPorId(id);
        if (pago == null)
        {
            return NotFound();
        }

        if (!pago.Estado)
        {
            TempData["Error"] = "El pago ya está anulado.";
            return RedirectToAction("Index", new { idReserva = pago.Id_reserva });
        }

        pago.Id_usuario_anulador = ObtenerIdUsuarioLogueado() ?? 0;
        int filasAfectadas = _repositorio.Anular(pago);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index", new { idReserva = pago.Id_reserva });
    }

    private int? ObtenerIdUsuarioLogueado()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private void CargarListas()
    {
        ViewBag.Usuarios = _repositorioUsuario.ObtenerTodos();
    }
}