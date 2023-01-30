using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreading
{
    public class TheadSafeStringBuffer
    {
        private StringBuilder _stringBuilder;

        private object locker = new();  // объект-заглушка

        public string String { get { return _stringBuilder.ToString(); }  }

        public TheadSafeStringBuffer()
        {
            this._stringBuilder = new StringBuilder();
        }

        public void append(string s)
        {
            lock (locker)
            {
                _stringBuilder.Append(s);
            }
        }

    }
}
