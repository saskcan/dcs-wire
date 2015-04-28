using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DCSWireUtils;

namespace ComListener
{
    class Program
    {
        static Message msg = new Message();
        static char[] buffer = new char[64];
        static int serialIndex = 0;

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
                // wait for start
                if(serialIndex == 0)
                {
                    if(c != '$')
                    {
                        break;
                    }
                }
                buffer[serialIndex] = c;
                // check for end of message
                if(c == 13)
                {
                    msg.Decode(buffer);
                    serialIndex = 0;
                }
                else
                {
                    serialIndex = serialIndex + 1;
                }
            }
        }
    }
}
