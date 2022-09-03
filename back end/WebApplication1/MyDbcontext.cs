using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Role;
using WebApplication1.User;

namespace WebApplication1
{
    public class MyDbcontext:IdentityDbContext<Myuser,Myrole,long>
    {
        public MyDbcontext(DbContextOptions<MyDbcontext> options)
            : base(options)
        {

        }

    }
}
