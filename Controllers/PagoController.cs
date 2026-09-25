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

    public IActionResult Index(int idReserva, int pagina = 1)
    {
        const int tamanoPagina = 5;

        if (idReserva <= 0)
        {
            return RedirectToAction("Index", "Reserva");
        }

        var reserva = _repositorioReserva.ObtenerPorId(idReserva);
        if (reserva == null)
        {
            return NotFound();
        }

        int total = _repositorio.ContarPorReserva(idReserva);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var pagos = _repositorio.ObtenerPorReserva(idReserva, pagina, tamanoPagina);
        ViewBag.Reserva = reserva;
        // El total se consulta a la base, no se suma en la vista: si se sumara sobre el
        // Model, al paginar contaría solo los pagos de esta página.
        ViewBag.TotalPagado = _repositorio.TotalPagadoActivo(idReserva);
        ViewData["Cantidad"] = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
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
        CargarUsuariosDeDetails(pago);
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
return View(new Pago { Id_reserva = idReserva, Fecha_pago = DateTime.Now });
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
            return View(pago);
        }

        pago.Estado = true;
        pago.Id_usuario_creador = ObtenerIdUsuarioLogueado() ?? 0;
        int idGenerado = _repositorio.Alta(pago);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el pago.");
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

    // En Details solo hacen falta los nombres de los dos usuarios que tocaron el pago, asi
    // que se consultan por id en vez de bajar la tabla Usuario entera para usar dos filas.
    private void CargarUsuariosDeDetails(Pago pago)
    {
        ViewBag.UsuarioCreador = ObtenerNombreUsuario(pago.Id_usuario_creador);
        ViewBag.UsuarioAnulador = ObtenerNombreUsuario(pago.Id_usuario_anulador);
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
}