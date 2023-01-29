using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreading
{
    public class PingPongWriterWithMonitor
    {
        private readonly int _count;
        private bool _ping = true;

        public PingPongWriterWithMonitor(int count)
        {
            _count = count;
        }

        public void WritePing()
        {
            for (int i = 0; i < _count; i++)
            {
                lock (this)
                {
                    //If we need to do pong (not ping), we are waining for the other thread
                    //Если нам нужно выполнить pong(не ping), мы ждем другой поток

                    if (!_ping)
                    {
                        //In this time - other thread is writing ping
                        //В это время, другой поток будет печатать ping
                        Monitor.Wait(this);
                    }

                    Console.WriteLine("Ping!");
                    _ping = false;
                    Monitor.Pulse(this);
                    //Writing "Ping!" and notyfying other threat about it
                    //Печатаем Ping и уведомляем другой потоко об этом
                }
            }
        }

        public void WritePong()
        {
            //То же самое, только с pong
            //The same operation, but with "pong"
            for (int i = 0; i < _count; i++)
            {
                lock (this)
                {
                    if (_ping)
                    {
                        Monitor.Wait(this);
                    }
                    Console.WriteLine("Pong!");
                    _ping = true;
                    Monitor.Pulse(this);
                }
            }
        }
    }
}
