using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO.Ports;
using DCSWireUtils;

namespace VirtualCockpit
{


    public static class Cockpit
    {
        public static Components components = new Components();
    }


    static class Program
    {
        static DCSWireUtils.Message msg = new DCSWireUtils.Message();
        static char[] buffer = new char[64];
        static int serialIndex = 0;
        static public SerialPort port = new SerialPort("COM3");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // create a new thread for the UDP listener
            UdpListener.UdpListener listener = new UdpListener.UdpListener();
            listener.StateUpdated += HandleUpdates;
            Thread listenerThread = new Thread(listener.StartListener);
            listenerThread.Start();

            // open the serial port
            //var port = new SerialPort("COM3");
            port.BaudRate = 9600;
            port.Open();
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            // run the forms application
            Application.Run(new Form1());

        }

        static public void HandleUpdates(object sender, UdpListener.StateUpdatedEventArgs e)
        {
            var args = e.dimension.Split('_');
            var controlGroup = args[0];
            var control = args[1];

            // AAP
            if(controlGroup == "AAP")
            {
                if (control == "CDUPWR")
                {
                    Cockpit.components.AAP.CDUPWR.SetState(e.value);
                }
                else if (control == "EGIPWR")
                {
                    Cockpit.components.AAP.EGIPWR.SetState(e.value);
                }
                else if (control == "PAGE")
                {
                    Cockpit.components.AAP.PAGE.SetState(e.value);
                }
                else if (control == "STEER")
                {
                    Cockpit.components.AAP.STEER.SetState(e.value);
                }
                else if (control == "STEERPT")
                {
                    Cockpit.components.AAP.STEERPT.SetState(e.value);
                }
            }
        }

        static public void SendUDP(string msg)
        {
            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try
            {
                client.Connect("localhost", 7778);
                Byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
                client.Send(data, data.Length);
                client.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            foreach (char c in indata)
            {
                // wait for start
                if (serialIndex == 0)
                {
                    if (c != '$')
                    {
                        break;
                    }
                }
                buffer[serialIndex] = c;
                // check for end of message
                if (c == 13)
                {
                    msg.Decode(buffer);
                    serialIndex = 0;
                    UdpListener.StateUpdatedEventArgs args = new UdpListener.StateUpdatedEventArgs();
                    args.dimension = msg.controlGroup + "_" + msg.control;
                    UInt16 value;
                    UInt16.TryParse(msg.value, out value);
                    args.value = value;
                    HandleUpdates(null, args);
                }
                else
                {
                    serialIndex = serialIndex + 1;
                }
            }
        }

        static public void SendSerial(DCSWireUtils.Message msg, SerialPort port)
        {
            int length = Array.IndexOf(msg.raw, (char)13) + 1;
            port.Write(msg.raw, 0, length);
        }
    }
}
