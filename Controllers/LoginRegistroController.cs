using Login_Y_Registro.Context;
using Login_Y_Registro.Models;
using Login_Y_Registro.Models.ViewModels;
using Login_Y_Registro.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Login_Y_Registro.Controllers
{
    public class LoginRegistroController : Controller
    {
        LoginContext context;
        public ILogger<LoginRegistroController> logger;
        private readonly EmailService _emailService;

        public LoginRegistroController(LoginContext dbContext,ILogger<LoginRegistroController> _logger, EmailService emailService)
        {

            context = dbContext;
            logger = _logger;
            _emailService = emailService;


        }
        public IActionResult iniciarSesion()
        {
         return View();
        }

        [HttpPost]
        public IActionResult iniciarSesion(UsuarioViewModel usuario)
        {
           
            var usuarioIngresado = context.Usuarios.FirstOrDefault(u => u.Correo == usuario.correo);

            if (usuarioIngresado == null || usuarioIngresado.Contraseña != ConvertirSha256(usuario.contraseña))
            {
                ModelState.AddModelError("correo", "Correo o contraseña incorrectas.");
                return View(usuario);
            }
            else
            {
                return RedirectToAction("Index", "Home");

            }
        }

        public IActionResult registrarUsuario()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> registrarUsuario(UsuarioViewModel u)
        {
            var usuarioRepetido = context.Usuarios.FirstOrDefault(user=> user.Correo == u.correo);

            if(usuarioRepetido != null) 
            {
                ModelState.AddModelError("correo", "Ya existe una cuenta registrada con esa direccion de correo");
                return View(u);
            }
            else if (u.contraseña == u.confirmarContraseña)
            {

                string contraseñaBasheada = ConvertirSha256(u.contraseña);

                string token = Guid.NewGuid().ToString(); //Genera token unico

                Usuario usuarioFinal = new Usuario
                {
                    Correo = u.correo,
                    Contraseña = contraseñaBasheada,
                    Confirmado=false,
                    TokenConfirmacion= token
                    
                };

                context.Usuarios.Add(usuarioFinal);
                context.SaveChanges();

                //Enviar correo de confirmacion

                string linkConfirmacion = Url.Action("ConfirmarCorreo", "LoginRegistro", new { token }, Request.Scheme);
                string mensaje = $"<h3>Bienvenido a nuestra aplicación</h3><p>Haz click <a href='{linkConfirmacion}'>aquí</a> para confirmar tu correo.</p>";

                await _emailService.EnviarCorreo(u.correo, "Confirma tu cuenta", mensaje);

                return RedirectToAction("iniciarSesion", "LoginRegistro");
            }
            else
            {
                ModelState.AddModelError("contraseña", "Las contraseñas no coinciden");
                ModelState.AddModelError("repetirContraseña", "Las contraseñas no coinciden");
                return View(u);
            }
           
        }

        public IActionResult ConfirmarCorreo(string token)
        {
            var usuario = context.Usuarios.FirstOrDefault(u => u.TokenConfirmacion == token);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Confirmado = true;
            usuario.TokenConfirmacion = null;
            context.SaveChanges();

            return View("ConfirmacionExitosa");
        }


        public IActionResult RecuperarContraseña()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarContraseña(string correo)
        {
            var usuario = context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null)
            {
                ModelState.AddModelError("correo", "No existe una cuenta con ese correo.");
                return View();
            }

            string token = Guid.NewGuid().ToString(); // Genera un token único
            usuario.TokenConfirmacion = token;
            context.SaveChanges();

            // Crear el enlace para restablecer la contraseña
            string linkRecuperacion = Url.Action("RestablecerContraseña", "LoginRegistro", new { token }, Request.Scheme);
            string mensaje = $"<p>Para restablecer tu contraseña, haz click <a href='{linkRecuperacion}'>aquí</a>.</p>";

            // Enviar el correo con el enlace
            await _emailService.EnviarCorreo(usuario.Correo, "Recuperación de contraseña", mensaje);

            return View("RecuperacionEnviada"); // Puedes crear una vista simple que le avise al usuario que se envió el correo
        }

        public IActionResult RestablecerContraseña(string token)
        {
            var usuario = context.Usuarios.FirstOrDefault(u => u.TokenConfirmacion == token);
            if (usuario == null)
            {
                return NotFound(); // Si no se encuentra el token, retorna 404
            }

            // Si el token es válido, se muestra la vista para cambiar la contraseña
            return View(new RestablecerContraseñaViewModel { Token = token });
        }

        [HttpPost]
        public IActionResult RestablecerContraseña(RestablecerContraseñaViewModel model)
        {
            var usuario = context.Usuarios.FirstOrDefault(u => u.TokenConfirmacion == model.Token);
            if (usuario == null)
            {
                return NotFound(); // Si no se encuentra el token, retorna 404
            }

            // Verificar que las contraseñas coinciden
            if (model.NuevaContraseña != model.ConfirmarContraseña)
            {
                ModelState.AddModelError("ConfirmarContraseña", "Las contraseñas no coinciden.");
                return View(model);
            }

            // Actualizar la contraseña
            usuario.Contraseña = ConvertirSha256(model.NuevaContraseña); // Asumí que usas SHA256 para la contraseña
            usuario.TokenConfirmacion = null; // Eliminar el token después de usarlo
            context.SaveChanges();

            return RedirectToAction("IniciarSesion"); 
        }   









        //METODO PARA CIFRAR LAS CONTRASEÑAS
        public static string ConvertirSha256(string texto)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                {
                    Sb.Append(b.ToString("x2"));
                }

            }
            return Sb.ToString();
        }



    }
}
