using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers;

public class UsuarioController : Controller
{
    private readonly IRepositorioUsuario _repositorio;

    public UsuarioController(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Usuario user)
    {
        if (user == null)
            return BadRequest("Los datos del Usuario son nulos");

        //Data Annotations-> en vez de recorrer cada campo, se decoran las propiedades de la clase, es una solucion nativa de APS.NET
        //asi seria la sitaxis en la clase (models.usuario) [Required(ErrorMessage = "El email es obligatorio")]
        //ASP.NET valida automaticamente las reglas y almacena los errores en ModelState, el json regresa la lista de los campos que fallaron
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            user.Estado = true; 
            var idGenerado = _repositorio.Alta(user);

            return Ok(new
            {
                mensaje = "Usuario creado exitosamente",
                id = idGenerado,
                usuario = user
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al crear Usuario", detalle = e.Message });
        }
    }

    [HttpPost]
    public IActionResult Delete([FromBody] Usuario user)
    {
        if (user == null)
            return BadRequest("Los datos ingresados son nulos");

        if (user.Id_usuario <= 0)
            return BadRequest("Es necesario un ID de usuario válido");

        try
        {
            user.Estado = false; 
            int filas = _repositorio.Baja(user);

            if (filas == 0)
                return NotFound(new { mensaje = "Usuario no encontrado o no se pudo dar de baja" });

            return Ok(new { mensaje = "Usuario dado de baja exitosamente" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al eliminar el usuario", detalle = e.Message });
        }
    }
}