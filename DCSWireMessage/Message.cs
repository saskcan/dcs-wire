using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCSWireUtils
{
	/// <summary>
	/// A generic message container designed with the needs of DCS in mind
	/// </summary>
	public class Message
	{
        public string controlGroup;
        public string control;
        public string type;
        public string value;
        public bool sendControlGroupViaUDP;

        public string Descriptor
        {
            get
            {
                return controlGroup + "_" + control;
            }
        }

		public Message(dynamic x)
		{
			controlGroup = x.controlGroup;
			control = x.control;
			type = x.type;
			value = x.value;
		}

		public Message() { }

        public Message(string cg, string c, string t, string v = null, bool sCVUDP = true)
        {
            controlGroup = cg;
            control = c;
            type = t;
            value = v;
            sendControlGroupViaUDP = sCVUDP;
        }
	}

	/// <summary>
	/// A message designed to be read from/written to a serial interface
	/// </summary>
    public class SerialMessage : Message
    {
        public char[] raw;

        public void Encode()
        {
            int start = 1;
            raw[0] = '$';
            controlGroup.CopyTo(0, raw, start, controlGroup.Length);
            raw[controlGroup.Length + 1] = ',';
            start = controlGroup.Length + 2;
            control.CopyTo(0, raw, start, control.Length);
            raw[start + control.Length] = ',';
            start = start + control.Length + 1;
            type.CopyTo(0, raw, start, type.Length);
            raw[start + type.Length] = ',';
            start = start + type.Length + 1;
            value.CopyTo(0, raw, start, value.Length);
            raw[start + value.Length] = (char)13;
        }

        public bool Decode(char[] buffer)
        {
            // find start and end positions of message
            int start = 0;
            int end = 0;
            bool foundStart = false;
            for (int i = 0; i < buffer.Length; i = i + 1)
            {
                if (!foundStart)
                {
                    if (buffer[i] == '$')
                    {
                        start = i;
                        foundStart = true;
                    }
                }
                else
                {
                    if (buffer[i] == 13)
                    {
                        end = i;
                        break;
                    }
                }
            }

            // either the start or the end of the message could not be found
            if (!foundStart || start >= end)
            {
                return false;
            }

            // copy the important part into the message
            Array.Copy(buffer, start, raw, 0, end - start);

            int first = 1;
            controlGroup = Utils.arrayToStringUntil(raw, first, end - start, ',');
            first = first + controlGroup.Length + 1;
            control = Utils.arrayToStringUntil(raw, first, end - start, ',');
            first = first + control.Length + 1;
            type = Utils.arrayToStringUntil(raw, first, end - start, ',');
            first = first + type.Length + 1;
            value = Utils.arrayToStringUntil(raw, first, end - start, (char)13);
            return true;
        }

        public SerialMessage()
        {
            raw = new char[64];
        }

		public SerialMessage(char[] buffer)
		{
			raw = new char[64];
			Decode(buffer);
		}

		public SerialMessage(Message msg)
		{
			raw = new char[64];
			controlGroup = msg.controlGroup;
			control = msg.control;
			type = msg.type;
			value = msg.value;
			Encode();
		}
    }

    public class MessageReadyEventArgs : EventArgs
    {
        public Message message { get; set; }

		public MessageReadyEventArgs(Message msg)
		{
			message = msg;
		}
    }
}
