using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecognitionController : Controller
    {
        BBaseContext db;
        public RecognitionController(BBaseContext context)
        {
            this.db = context;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var stats = (from ans in db.Answers
                         group ans by ans.Answer into ans_gr
                         select new Stat() { Class = ans_gr.Key, Num_Images = ans_gr.Count() }).ToList<Stat>();
            List<string> msg = new List<string>();
            foreach (Stat stat in stats)
            {
                msg.Add(stat.ToString());
            }
            if (!msg.Any())
                msg.Add("No images in Dataset");

            return new List<string>(msg);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public ActionResult<ImageResult> Get(int id)
        {
            var img = db.Answers.Where(i => i.Id == id).FirstOrDefault();
            if (img == null)
                return NotFound();
            else
                return new ImageResult()
                {
                    FileName = img.File_Name,
                    Class = img.Answer
                };
        }

        // POST api/<controller>
        [HttpPost]
        public int Post([FromBody] List<string> value)
            {            
            if (value == null)
                return -1;
            else
            {
                var image = Convert.FromBase64String(value[1]);
                var calc_cl = new NewRecognition(image, db);
                int result = calc_cl.Run_For_File(value[0],
                    "C:\\Users\\mosca\\source\\repos\\Prac_2019\\model.onnx", new int[] { 1, 1, 28, 28 });

                return result;
            }

        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete()
        {
            var blobs = db.Blobs;
            foreach (var blob in blobs)
            {
                db.Blobs.Remove(blob);
            }

            var answers = db.Answers;
            foreach (var answer in answers)
            {
                db.Answers.Remove(answer);
            }
            db.SaveChanges();
        }
    }

    public class ImageResult
    {
        public string FileName { get; set; }
        public int Class { get; set; }
    }

    public class ImageInfo
    {
        public string Filename { get; set; }
        public byte[] Image { get; set; }
    }

    public class Stat
    {
        public int Class { get; set; }
        public int Num_Images { get; set; }

        public override string ToString()
        {
            return "In class " + Class.ToString() + " were recognized " + Num_Images.ToString() + " images\n";
        }
    }
}
