using Login_Y_Registro.Context;
using Login_Y_Registro.Models;
using Login_Y_Registro.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Login_Y_Registro.Controllers
{
    public class LoginRegistroController : Controller
    {
        LoginContext context;
        public ILogger<LoginRegistroController> logger;

        public LoginRegistroController(LoginContext dbContext,ILogger<LoginRegistroController> _logger)
        {

            context = dbContext;
            logger = _logger;


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
        public IActionResult registrarUsuario(UsuarioViewModel u)
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

                Usuario usuarioFinal = new Usuario
                {
                    Correo = u.correo,
                    Contraseña = contraseñaBasheada
                };

                context.Usuarios.Add(usuarioFinal);
                context.SaveChanges();

                return RedirectToAction("iniciarSesion", "LoginRegistro");
            }
            else
            {
                ModelState.AddModelError("contraseña", "Las contraseñas no coinciden");
                ModelState.AddModelError("repetirContraseña", "Las contraseñas no coinciden");
                return View(u);
            }
           
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
