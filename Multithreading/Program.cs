using System;
using System.Threading;

namespace Multithreading
{
    public class Program
    {

        static void Main(string[] args)
        {
            //task1PingPong();
            //task2ThreadPool();

            Console.WriteLine("AsyncCount");
            ProcessDataAsync();
        }

        private static void task1PingPong()
        {
            //Task1 - Ping/Pong
            var writer = new PingPongWriterWithMonitor(count: 3);

            var pingThread = new Thread(new ThreadStart(writer.WritePing));
            var pongThread = new Thread(new ThreadStart(writer.WritePong));

            Console.WriteLine("Go!\n");

            pingThread.Start();
            pongThread.Start();

            pingThread.Join();
            pongThread.Join();

            Console.WriteLine("\nDone!");
        }

        public static void task2ThreadPool()
        {
            MyThreadPool tp = new MyThreadPool();

            for (int i = 0; i < 100; i++)
            {
                tp.QueueWork(i, new Multithreading.MyThreadPool.WorkDelegate(PerformWork));
            }

            Console.ReadLine();
            //tp.Shutdown();
        }

        static private void PerformWork(object o)
        {
            int i = (int)o;
            Console.WriteLine("Work Performed: " + i.ToString());
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("End Work Performed: " + i.ToString());
        }

        static async void ProcessDataAsync()
        {
            Task<int> task = HandleFileAsync("C:\\OSPanel\\enable1.txt");

            await task;
            Console.WriteLine("Count: " + task.Result);
        }

        static async Task<int> HandleFileAsync(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string str = await reader.ReadToEndAsync();

                return str.Count(x => Char.IsDigit(x));
            }
        }
    }
}