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
using DCSWire;

namespace VirtualCockpit
{
    static class Program
    {
		static public Cockpit.Cockpit cockpit;
        static public SerialAgent serialAgent;
        static public Form1 ui;
        static public DCSBIOSAgent dcsBiosAgent;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			// initialize the cockpit
            cockpit = new Cockpit.Cockpit();
            cockpit.StateUpdated += SendMEssage;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

			// create a new SerialAgent to send and receive serial data
            //serialAgent = new SerialAgent("COM3", 9600);
            serialAgent = new SerialAgent("COM3", 9600);
			serialAgent.MessageReady += ReceiveMessage;
            serialAgent.Begin();

            // create a new DCSBIOSAgent and run it on its own thread
            dcsBiosAgent = new DCSBIOSAgent(IPAddress.Parse("239.255.50.10"), 5010, new byte[] {0x55, 0x55, 0x55, 0x55}, 65536);
            dcsBiosAgent.StateUpdated += ReceiveMessage;
            Thread DCSBIOSThread = new Thread(dcsBiosAgent.StartAgent);
            DCSBIOSThread.Start();

            // prepare a new windows forms object
            ui = new Form1();
            // hook up ui events to the HandleMessage message handler
            ui.MessageReady += ReceiveMessage;
            // run the forms application
            Application.Run(ui);
        }

        // messages arriving through Serial Port, the Virtual Cockpit and From UDP
		static public void ReceiveMessage(object sender, MessageReadyEventArgs e)
		{
            // get the message value type
            string type = e.message.type;
            if(type == "INT")
            {
                cockpit.panels[e.message.controlGroup].numericInterfaceables[e.message.control].SetValue(e.message.value);
            }
            else if(type == "STR")
            {
                cockpit.panels[e.message.controlGroup].textInterfaceables[e.message.control].Value = e.message.value;
            }
		}

        // messages created when the internal state is updated
        static public void SendMEssage(object sender, MessageReadyEventArgs e)
        {
            dcsBiosAgent.SendMessage(e.message);
            serialAgent.SendMessage(e.message);
        }
    }
}
