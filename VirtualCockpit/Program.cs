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
    static class Program
    {
        static DCSWireUtils.Message msg = new DCSWireUtils.Message();
		static public Cockpit cockpit;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			// initialize the cockpit
			cockpit = VirtualCockpit.Components.initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // create a new thread for the UDP listener
            UdpListener.UdpListener listener = new UdpListener.UdpListener();
            listener.StateUpdated += HandleMessage;
            Thread listenerThread = new Thread(listener.StartListener);
            listenerThread.Start();

			// create a new SerialAgent to send and receive serial data
			DCSWire.SerialAgent serialAgent = new DCSWire.SerialAgent("COM3", 9600);
			serialAgent.MessageReady += HandleMessage;
			// start listening
			serialAgent.port.Open();

            // run the forms application
            Application.Run(new Form1());

        }

		static public void HandleMessage(object sender, MessageReadyEventArgs e)
		{
			// here we should be able to handle any generic message regardless of who sent it
			cockpit.panels[e.message.controlGroup].multiPositionSwitches[e.message.control].SetState(e.message.value);
		}

        static public void HandleUpdates(object sender, UdpListener.StateUpdatedEventArgs e)
        {
            var args = e.dimension.Split('_');
            var controlGroup = args[0];
            var control = args[1];

            // AAP
			cockpit.panels["controlGroup"].
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
    }
}
