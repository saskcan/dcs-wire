using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComListener
{
    class Message
    {
        public string controlGroup;
        public string control;
        public string type;
        public char[] message;
        public char[] raw;

        public bool decode()
        {
            // check to make sure the message is well-formed
            // 13 is ascii code for CR character
            if(raw[0] != '$')
            {
                return false;
            }
            if(raw[raw.Length - 1] != 13)
            {
                return false;
            }
            // strip off first and last characters and split up the reset by commas
            string buffer;
            char[] trimmed = new char[raw.Length - 2];
            buffer = new String(trimmed);
            // parse the controlGroup
            var pos = buffer.IndexOf(',');
            controlGroup = buffer.Substring(0,pos);
            buffer = buffer.Substring(pos + 1, buffer.Length - pos + 1);
            // parse the control
            pos = buffer.IndexOf(',');
            control = buffer.Substring(0, pos);
            buffer = buffer.Substring(pos + 1, buffer.Length - pos + 1);
            // parse the type
            pos = buffer.IndexOf(',');
            type = buffer.Substring(0, pos);
            buffer = buffer.Substring(pos + 1, buffer.Length - pos + 1);
            // the message is what's left
            message = buffer.ToCharArray();

            return true;
        }
    }
}
