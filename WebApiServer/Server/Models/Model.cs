using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class DataClass
    {
        public int Id { get; set; }
        public int Answer { get; set; }
        public string File_Name { get; set; }
    }

    public class ImgBloB : DataClass
    {
        public byte[] Image { get; set; }
    }
}
