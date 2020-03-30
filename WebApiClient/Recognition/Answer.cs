using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recognition
{
    public class Answer
    {
        public int Class { get; set; }
        public string File_Name { get; set; }

        public Answer() { }

        public Answer(int expected_cl, string path_to_file)
        {
            Class = expected_cl;
            File_Name = path_to_file;
        }

        public override string ToString()
        {
            return File_Name + " is " + Class.ToString();
        }
    }

    public class Stat
    {
        public int Class { get; set; }
        public int Num_Images { get; set; }

        public Stat() { }

        public Stat(int class_num, int img_num)
        {
            Class = class_num;
            Num_Images = img_num;
        }

        public override string ToString()
        {
            return "In class " + Class.ToString() + " were recognized " + Num_Images.ToString() + " images\n";
        }
    }
}
