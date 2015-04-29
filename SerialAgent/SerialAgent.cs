using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DCSWireUtils;

namespace DCSWire 
{
	// A delegate type for hooking up message notifications
	public delegate void MessageReadyEventHandler(object sender, MessageReadyEventArgs e);

    public class SerialAgent 
    {
		int serialIndex;
        char[] buffer = new char[64];
        public SerialPort port;

		// An event that clients can use to be notified whenever a message is ready
		public event MessageReadyEventHandler MessageReady;

		// Invoke the Ready event; called whenever a message is ready
		protected virtual void OnMessageReady(MessageReadyEventArgs e)
		{
			if (MessageReady != null)
				MessageReady(this, e);
		}

		private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
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
					Message msg = new Message(buffer);
					serialIndex = 0;
					// invoke the event
					OnMessageReady(new MessageReadyEventArgs(msg));
				}
				else
				{
					serialIndex = serialIndex + 1;
				}
			}
		}

		public SerialAgent(string portName, int baudRate)
		{
			port = new SerialPort(portName, baudRate);
		}
    }
}
