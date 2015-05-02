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
					// decode the message
					SerialMessage smsg = new SerialMessage(buffer);
					// extract the message
					Message msg = new Message(smsg);
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

		public void SendMessage(Message msg)
		{
			SerialMessage smsg = new SerialMessage(msg);
            int length = Array.IndexOf(smsg.raw, (char)13) + 1;
            port.Write(smsg.raw, 0, length);
		}

		public SerialAgent(string portName, int baudRate)
		{
			port = new SerialPort(portName, baudRate);
			port.DataReceived += DataReceivedHandler;
		}
    }
}
