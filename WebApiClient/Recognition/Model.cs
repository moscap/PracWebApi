using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using System.Numerics.Tensors;
using ImRead;
using System.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Data;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;

namespace Recognition
{
    public class Model
    {
        private static readonly HttpClient client = new HttpClient();
        List<string> Files = null;
        CancellationTokenSource cts;
        bool Cancellation_Flag = false;
        public ConcurrentQueue<Answer> answers;
        object lock_obj = new object();


        public Model()
        {
            this.answers = new ConcurrentQueue<Answer>();
        }


        public void Load_Files(string path)
        {
            try
            {
                Files = Directory.GetFiles(path).ToList<string>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool Are_Files_Set()
        {
            return Files != null;
        }

        public float[] Load_Image(string path)
        {
            GrayscaleFloatImage image = ImageIO.FileToGrayscaleFloatImage(path);
            return image.rawdata;
        }

        public byte[] Load_Byte_Image(string path)
        {
            GrayscaleByteImage image = ImageIO.FileToGrayscaleByteImage(path);
            return image.rawdata;
        }

        public async Task<List<string>> Get_Stat()
        {
            var stat = new List<string>();
            string s;
            try
            {
                s = await client.GetStringAsync("https://localhost:5001/api/recognition");
            } catch(HttpRequestException ex)
            {
                stat.Add(ex.Message);
                return stat;
            }            
            var books = JsonConvert.DeserializeObject<List<string>>(s);
            return books;
        }

        public async Task<string> Clear_Database()
        {
            try
            {
                var s = await client.DeleteAsync("https://localhost:5001/api/recognition");
            }
            catch (HttpRequestException ex)
            {
                return ex.Message;
            }
            return "Database sucsessfully cleared!";
        }

        void Activate_Token()
        {
            Task t = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (Cancellation_Flag)
                    {
                        cts.Cancel();
                        return;
                    }
                    Thread.Sleep(0);
                }
            }, cts.Token);
        }

        public void Stop_Tasks()
        {
            Cancellation_Flag = true;
            answers.Enqueue(null);
        }



        void Run_For_File(string file, CancellationTokenSource cts)
        {
            if (cts.Token.IsCancellationRequested)
                return;

            var image = Load_Byte_Image(file);

            var msg = new List<string>();
            msg.Add(file);
            msg.Add(Convert.ToBase64String(image));
            var dataAsString = JsonConvert.SerializeObject(msg);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            int ans = -1;
            HttpResponseMessage res;
            try
            {
                var request = client.PostAsync("https://localhost:5001/api/recognition", content);
                res = request.Result;
            }
            catch (Exception ex)
            {
                answers.Enqueue(new Answer(ans, "Something bad with server\n" + ex.Message));
                cts.Cancel();
                return;
            }            

            if (res.IsSuccessStatusCode)
            {
                var string_task = res.Content.ReadAsStringAsync();
                var string_ans = string_task.Result;
                ans = JsonConvert.DeserializeObject<int>(string_ans);
            }

            answers.Enqueue(new Answer(ans, file));

        }

        public void Run_Parallel(string files)
        {
            cts = new CancellationTokenSource();
            Cancellation_Flag = false;
            Activate_Token();

            try
            {
                Load_Files(files);

                var count = Files.Count;

                var tasks = new Task[count];

                for (int i = 0; i < count; ++i)
                {
                    tasks[i] = Task.Factory.StartNew(file =>
                    {
                        Run_For_File((string)file, cts);
                    },
                    Files[i], cts.Token);
                }
                Task.WaitAll(tasks);
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Task is canceled");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            answers.Enqueue(null);
            cts.Cancel();
        }

        public override string ToString()
        {
            string ans = "";
            Answer buf;
            while (answers.TryDequeue(out buf))
            {
                ans += buf.ToString() + "\n";
            }

            return ans;
        }
    }
}
