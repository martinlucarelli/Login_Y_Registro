using Login_Y_Registro.Context;
using Microsoft.AspNetCore.Mvc;

namespace Login_Y_Registro.Controllers
{
    public class LoginRegistroController : Controller
    {
        LoginContext context;



        public IActionResult iniciarSesion()
        {

            return View();
        }

        //[HttpPost]
        
  
    }
}
