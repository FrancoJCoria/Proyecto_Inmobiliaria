using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace Inmobiliaria.Controllers;

public class UsuarioController : Controller
{
    private readonly IRepositorioUsuario _repositorio;

    public UsuarioController(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }


    [Authorize(Roles = "Administrador")]
    public IActionResult Index()
    {
        var usuarios = _repositorio.ObtenerTodos();
        ViewData["Cantidad"] = usuarios.Count();
        return View(usuarios);
    }


    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public IActionResult Details(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public IActionResult Create(Usuario usuario, [FromServices] IWebHostEnvironment environment)
    {
        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        if (_repositorio.ObetenerPorEmail(usuario.Email) != null)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
            return View(usuario);
        }

        usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuario.Clave);
        usuario.Estado = true;
        int idGenerado = _repositorio.Alta(usuario);
        if (idGenerado == 0)
        {
            ModelState.AddModelError("", "No se pudo crear el usuario.");
            return View(usuario);
        }

        if (usuario.AvatarFile != null && usuario.AvatarFile.Length > 0)
        {
            usuario.Avatar = GuardarAvatar(usuario, environment);
            _repositorio.Modificacion(usuario);
        }

        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        // autorización: el que no es administrador solo puede ver su propio perfil
        var idUsuarioLogueado = ObtenerIdUsuarioLogueado();
        if (!User.IsInRole("Administrador") && idUsuarioLogueado != id)
        {
            return RedirectToAction("AccesoDenegado", "Home");
        }

        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Edit(int id, Usuario usuario, [FromServices] IWebHostEnvironment environment)
    {
        var original = _repositorio.ObtenerPorId(id);
        if (original == null)
        {
            return NotFound();
        }

        // autorización: el que no es administrador solo puede modificar su propio perfil
        var idUsuarioLogueado = ObtenerIdUsuarioLogueado();
        var esAdministrador = User.IsInRole("Administrador");
        if (!esAdministrador && idUsuarioLogueado != id)
        {
            return RedirectToAction("AccesoDenegado", "Home");
        }

        // un no administrador no puede cambiar su propio rol
        if (!esAdministrador)
        {
            usuario.Rol = original.Rol;
            ModelState.Remove("Rol");
        }

        // si no se sube una foto nueva, se conserva la actual
        usuario.Avatar = original.Avatar;
        if (usuario.AvatarFile != null && usuario.AvatarFile.Length > 0)
        {
            EliminarAvatar(id, environment);
            usuario.Avatar = GuardarAvatar(usuario, environment);
        }

        // si la clave viene vacía se conserva la actual (ya hasheada)
        if (string.IsNullOrWhiteSpace(usuario.Clave))
        {
            usuario.Clave = original.Clave;
            ModelState.Remove("Clave");
        }
        else
        {
            usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuario.Clave);
        }

        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        usuario.Id_usuario = id;
        int filasAfectadas = _repositorio.Modificacion(usuario);
        if (filasAfectadas == 0)
        {
            ModelState.AddModelError("", "No se pudo modificar el usuario. Puede estar dado de baja.");
            return View(usuario);
        }
        return User.IsInRole("Administrador") ? RedirectToAction("Index") : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public IActionResult DeleteConfirmado(int id)
    {
        var usuario = _repositorio.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Estado = false;
        int filasAfectadas = _repositorio.Baja(usuario);
        if (filasAfectadas == 0)
        {
            return NotFound();
        }
        return RedirectToAction("Index");
    }

    //--------------------------------------AUTENTICACION: LOGIN------------------------------------------//

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        TempData["ReturnUrl"] = returnUrl;
        return View(new LoginView());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginView login)
    {
        if (ModelState.IsValid)
        {
            var usuario = _repositorio.ObetenerPorEmail(login.Usuario);
            if (usuario == null || !VerificarClave(login.Clave, usuario.Clave))
            {
                ModelState.AddModelError("", "El email o la contraseña no son correctos.");
                return View(login);
            }
            if (!usuario.Estado)
            {
                ModelState.AddModelError("", "El usuario está dado de baja.");
                return View(login);
            }

            // migración automática: si la clave guardada está en texto plano, se rehashea
            if (!EsHashBcrypt(usuario.Clave))
            {
                usuario.Clave = BCrypt.Net.BCrypt.HashPassword(login.Clave);
                _repositorio.ActualizarClave(usuario.Id_usuario, usuario.Clave);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id_usuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            var returnUrl = TempData["ReturnUrl"] as string;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        return View(login);
    }

    //--------------------------------------AUTENTICACION: LOGOUT------------------------------------------//

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private bool VerificarClave(string claveIngresada, string claveGuardada)
    {
        if (EsHashBcrypt(claveGuardada))
        {
            return BCrypt.Net.BCrypt.Verify(claveIngresada, claveGuardada);
        }
        return claveIngresada == claveGuardada;
    }

    private bool EsHashBcrypt(string claveGuardada)
    {
        return !string.IsNullOrEmpty(claveGuardada) && claveGuardada.StartsWith("$2");
    }

    private int? ObtenerIdUsuarioLogueado()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    //--------------------------------------AVATAR------------------------------------------//

    private string GuardarAvatar(Usuario usuario, IWebHostEnvironment environment)
    {
        string path = Path.Combine(environment.WebRootPath, "Uploads", "Avatares");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string extension = Path.GetExtension(usuario.AvatarFile!.FileName);
        string nombreArchivo = $"avatar_{usuario.Id_usuario}{extension}";
        string rutaCompleta = Path.Combine(path, nombreArchivo);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            usuario.AvatarFile.CopyTo(stream);
        }

        return $"/Uploads/Avatares/{nombreArchivo}";
    }

    private void EliminarAvatar(int id, IWebHostEnvironment environment)
    {
        string path = Path.Combine(environment.WebRootPath, "Uploads", "Avatares");
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var archivo in Directory.GetFiles(path, $"avatar_{id}.*"))
        {
            System.IO.File.Delete(archivo);
        }
    }
}
