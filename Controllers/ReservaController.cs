using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers;

public class ReservaController : Controller
{
    private readonly IRepositorioReserva _repositorio;
    private readonly IRepositorioInquilino _repositorioInquilino;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioInmueble _repositorioInmueble;

    public ReservaController(IRepositorioReserva repositorio, IRepositorioInquilino repositorioInquilino, IRepositorioUsuario repositorioUsuario, IRepositorioInmueble repositorioInmueble)
    {
        _repositorio = repositorio;
        _repositorioInquilino = repositorioInquilino;
        _repositorioUsuario = repositorioUsuario;
        _repositorioInmueble = repositorioInmueble;
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

        reserva.Id_reserva = id;
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

    private void CargarListas()
    {
        ViewBag.Inquilinos = _repositorioInquilino.ObtenerTodos();
        ViewBag.Usuarios = _repositorioUsuario.ObtenerTodos();
        ViewBag.Inmuebles = _repositorioInmueble.ObtenerTodos();
    }
}
