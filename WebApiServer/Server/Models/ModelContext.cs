using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    public class BBaseContext : DbContext
    {
        public BBaseContext(DbContextOptions<BBaseContext> options) : base(options)
        { }
        public BBaseContext()
        {
        }

        public DbSet<DataClass> Answers { get; set; }
        public DbSet<ImgBloB> Blobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=usersdbnew;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}
