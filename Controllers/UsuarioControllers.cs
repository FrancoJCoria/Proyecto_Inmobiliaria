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


    //creacion del usuario
    [HttpPost]
    public IActionResult Create([FromBody] Usuario user)
    {
        if(user == null)
        {
            return BadRequest("Los datos del Usuario son nulos");
        }

        //Data Annotations-> en vez de recorrer cada campo, se decoran las propiedades de la clase, es una solucion nativa de APS.NET
        //asi seria la sitaxis en la clase (models.usuario) [Required(ErrorMessage = "El email es obligatorio")]
        //ASP.NET valida automaticamente las reglas y almacena los errores en ModelState, el json regresa la lista de los campos que fallaron
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            return Ok(new
            {
                mensaje= "usuario creado",
                usuario = user
            });
        }catch(Exception e)
        {
            return StatusCode(500, new {error = "Error al crear Usuario", detalle = e.Message});
        }
    }
}