using System.ComponentModel.DataAnnotations;

namespace Login_Y_Registro.Models.ViewModels
{
    public class UsuarioViewModel
    {
      [EmailAddress]
      [Required]
      public  string? correo { get; set; }
     [Required]
     public  string? contraseña { get; set; }

      public  string? confirmarContraseña { get; set; }


    }
}
