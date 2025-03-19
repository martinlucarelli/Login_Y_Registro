using Login_Y_Registro.Models;
using Microsoft.EntityFrameworkCore;

namespace Login_Y_Registro.Context
{
    public class LoginContext:DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }

        public LoginContext(DbContextOptions<LoginContext> options) : base(options) { }


    }
}
