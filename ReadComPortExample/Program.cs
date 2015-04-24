using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ReadComPortExample
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort port = new SerialPort("COM1");

            port.BaudRate = 9600;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DataBits = 8;
            port.Handshake = Handshake.None;

            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            
            port.Open();

            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
            port.Close();
        }

        private static void DataReceivedHandler(
            object sender,
            SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine("Data received:");
            Console.WriteLine(indata);
        }
    }
}
