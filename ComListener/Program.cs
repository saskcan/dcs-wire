using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ComListener
{
    class Program
    {
        static char[] buffer;
        static int next = 0;

        static void Main(string[] args)
        {
            var port = new SerialPort("COM3");
            port.BaudRate = 9600;
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.Open();
            Console.ReadKey();
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            foreach(char c in indata)
            {
                if(next == 0)
                {
                    if(c == '$')
                    {
                        buffer[next] = c;
                        next = next + 1;
                    }
                }
                else
                {
                    buffer[next] = c;
                    if(c == 13)
                    {
                        var msg = new Message();
                        Array.Copy(buffer, msg.raw, )
                    }
                }

            }
            Console.WriteLine(indata);
        }
    }
}
