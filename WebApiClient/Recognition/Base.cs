using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recognition
{
    public class DataClass
    {
        public int Id { get; set; }
        public int Answer { get; set; }
        public virtual ImgBloB Image { get; set; }
        public string File_Name { get; set; }
    }

    public class ImgBloB
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }

        [Required]
        public virtual DataClass DataClass { get; set; }
    }

    public class BBaseContext : DbContext
    {
        public BBaseContext() : base("DBCon")
        { }

        public DbSet<DataClass> Answers { get; set; }
        public DbSet<ImgBloB> Blobs { get; set; }
    }
}
