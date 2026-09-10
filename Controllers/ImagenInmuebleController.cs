using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers;

public class ImagenInmuebleController : Controller
{
    private readonly IRepositorioImagenInmueble _repositorio;

    public ImagenInmuebleController(IRepositorioImagenInmueble repositorio)
    {
        _repositorio = repositorio;
    }

    [HttpPost]
    public async Task<IActionResult> Alta(int id, List<IFormFile> imagenes, [FromServices] IWebHostEnvironment environment)
    {
        try
        {
            if (imagenes == null || imagenes.Count == 0)
                return BadRequest("No se recibieron archivos.");

            string wwwPath = environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Uploads");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Inmuebles");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, id.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in imagenes)
            {
                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    var rutaArchivo = Path.Combine(path, nombreArchivo);

                    using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    ImagenInmueble imagen = new ImagenInmueble
                    {
                        InmuebleId = id,
                        Url = $"/Uploads/Inmuebles/{id}/{nombreArchivo}",
                        Estado = true
                    };
                    _repositorio.Alta(imagen);
                }
            }
            return Ok(_repositorio.BuscarPorInmueble(id));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al subir imágenes", detalle = e.Message });
        }
    }

    [HttpPost]
    public IActionResult Eliminar(int id, [FromServices] IWebHostEnvironment environment)
    {
        try
        {
            var entidad = _repositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                return NotFound(new { error = "Imagen no encontrada" });
            }

            try
            {
                string rutaEliminar = Path.Combine(environment.WebRootPath, "Uploads", "Inmuebles",
                    Path.GetFileName(entidad.Url));
                if (System.IO.File.Exists(rutaEliminar))
                    System.IO.File.Delete(rutaEliminar);
            }
            catch
            {
            }

            _repositorio.Baja(id);
            return Ok(_repositorio.BuscarPorInmueble(entidad.InmuebleId));
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al eliminar imagen", detalle = e.Message });
        }
    }
}
