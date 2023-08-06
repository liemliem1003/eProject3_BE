using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Client.Models;

namespace Client.Data
{
    public class ClientContext : DbContext
    {
        public ClientContext (DbContextOptions<ClientContext> options)
            : base(options)
        {
        }

        public DbSet<Client.Models.User> User { get; set; } = default!;
    }
}
