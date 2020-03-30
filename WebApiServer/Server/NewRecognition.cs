using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Server
{
    public class NewRecognition
    {
        float[] f_data { get; set; }
        byte[] b_data { get; set; }
        BBaseContext baseContext { get; set; }


        public NewRecognition(byte[] dat, BBaseContext context)
        {
            float[] f_data = new float[dat.Length];
            for (int i = 0; i < dat.Length; ++i)
            {
                float buf = (float)dat[i];
                f_data[i] = buf / 255.0f;
            }
            this.f_data = f_data;
            this.b_data = dat;
            baseContext = context;
        }

        void Add_File(List<NamedOnnxValue> inputs, int[] dimentions, string input_meta_key)
        {
            Tensor<float> t = LoadTensorFromFileBytes(b_data);

            inputs.Add(NamedOnnxValue.CreateFromTensor<float>(input_meta_key, t));
        }

        int Most_Possible_Result(float[] arr)
        {
            var norm_arr = arr.Select(j => j / 1000.0);
            double[] exp = new double[arr.Length];
            int i = 0;
            foreach (var x in norm_arr)
            {
                exp[i++] = Math.Exp((double)x);
            }
            var sum_exp = exp.Sum();
            var softmax = exp.Select(j => j / sum_exp);

            double[] sorted = new double[arr.Length];
            Array.Copy(softmax.ToArray(), sorted, arr.Length);
            Array.Sort(sorted, (x, y) => -x.CompareTo(y));
            var max_val = sorted[0];
            var max_ind = softmax.ToList().IndexOf(sorted[0]);

            return max_ind;

        }

        public void Load_Model(string path, ref InferenceSession session)
        {
            try
            {
                session = new InferenceSession(path);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        static DenseTensor<float> LoadTensorFromFileBytes(byte[] fileBytes)
        {
            Bitmap tmp = new Bitmap(new MemoryStream(fileBytes));
            Bitmap img = new Bitmap(tmp, new Size(28, 28));
            float[,,,] data = new float[1, 1, img.Height, img.Width];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    data[0, 0, j, i] = ((float)img.GetPixel(i, j).R / 255);                    
                }
            }
            tmp.Dispose();
            img.Dispose();
            return data.ToTensor<float>();
        }

        public int Run_For_File(string file, string model_path, int[] dimentions)
        {

            byte[] byte_im = b_data;

            using (BBaseContext db = new BBaseContext())
            {
                var answer = db.Blobs.Where(p => p.File_Name == file).FirstOrDefault();
                if (answer != null)
                {
                    var im = answer.Image;
                    if (im.SequenceEqual(byte_im))
                    {
                        return answer.Answer;
                    }
                }
            }

            var session = new InferenceSession(model_path);
            var inputMeta = session.InputMetadata;

            var inputs = new List<NamedOnnxValue>();

            foreach (var name in inputMeta.Keys)
            {
                Add_File(inputs, dimentions, name);
            }

            var results = session.Run(inputs);

            foreach (var result in results)
            {
                var arr = result.AsEnumerable<float>().ToArray();
                var expected_cl = Most_Possible_Result(arr);

                using (BBaseContext db = new BBaseContext())
                {
                    db.Blobs.Add(new ImgBloB { Answer = expected_cl, File_Name = file, Image = byte_im });
                    db.SaveChanges();
                }

                return expected_cl;
            }

            return -1;

        }
    }
}
