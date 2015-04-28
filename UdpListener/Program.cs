using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UdpListener
{

    /// <summary>
    /// Event arguments included when the simulation state is updated
    /// </summary>
    public class StateUpdatedEventArgs : EventArgs
    {
        public string dimension { get; set; }
        public UInt16 value { get; set; }
        public string message { get; set; }
    }


    public class UdpListener
    {
        private byte[] startCode = { 0x55, 0x55, 0x55, 0x55 };
        private readonly IPAddress GroupAddress = IPAddress.Parse("239.255.50.10");
        private const int GroupPort = 5010;
        private byte[] state = new byte[65536];

        UdpClient listener;
        IPEndPoint groupEP;

        /// <summary>
        /// Receives data using client on ep until the start code is received.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ep"></param>
        /// <returns>Returns any data received after the start code</returns>
        byte[] waitForStart(UdpClient client, IPEndPoint ep)
        {
            byte[] buffer = new byte[]{0x00, 0x00, 0x00, 0x00}; // initialize a buffer
            int nextPosition = 0;
            while (true)
            {
                byte[] bytes = client.Receive(ref ep);
                while (bytes.Length > 0)
                {
                    // if the current byte matches the start code, add it to the buffer
                    if (bytes[0] == startCode[nextPosition]) 
                    {
                        buffer[nextPosition] = bytes[0];
                        nextPosition = nextPosition + 1;
                        // if the buffer is full, we have found a start code
                        if (nextPosition == buffer.Length)
                        {
                            // return remaining bytes to the calling function
                            byte[] remaining = new byte[bytes.Length - 1];
                            Array.Copy(bytes, 1, remaining, 0, remaining.Length);
                            return remaining;
                        }
                    }
                    else // otherwise reset the buffer
                    {
                        nextPosition = 0;
                    }
                    byte[] newBytes = new byte[bytes.Length - 1];
                    Array.Copy(bytes, 1, newBytes, 0, newBytes.Length);
                    bytes = newBytes;
                }
            }
        }

        /// <summary>
        /// Returns length bytes from start of buffer. If the buffer doesn't
        /// yet have length bytes, reads data using client on ep.
        /// Removes length bytes from start of buffer before returning.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ep"></param>
        /// <param name="buffer">A buffer to hold data being read by client</param>
        /// <param name="length">The number of bytes to read from buffer</param>
        /// <returns></returns>
        byte[] readBytes(UdpClient client, IPEndPoint ep, ref byte[] buffer, int length)
        {
            byte[] tmp;
            while(buffer.Length < length)
            {
                byte[] bytes = client.Receive(ref ep);
                tmp = new byte[buffer.Length + bytes.Length];
                Array.Copy(buffer, 0, tmp, 0, buffer.Length);
                Array.Copy(bytes, 0, tmp, buffer.Length, bytes.Length);
                buffer = tmp;
            }

            byte[] val = new byte[length];
            Array.Copy(buffer, val, length);

            tmp = new byte[buffer.Length - length];
            Array.Copy(buffer, length, tmp, 0, tmp.Length);
            buffer = tmp;

            return val;
        }


        // used to send an event when the state is updated
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        protected virtual void OnStateUpdated(StateUpdatedEventArgs e)
        {
            EventHandler<StateUpdatedEventArgs> handler = StateUpdated;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        void updateControls(int address)
        {
            UInt16 value;
            StateUpdatedEventArgs args = new StateUpdatedEventArgs();
            UInt16 partial = (UInt16)((state[address + 1] << 8) + state[address]);

            if(address == 0x10fa)
            {
                // CDU Power
                value = decode(partial, 0x4000, 14);
                args.dimension = "AAP_CDUPWR";
                args.value = value;
                OnStateUpdated(args);

                // EGI Power
                value = decode(partial, 0x8000, 15);
                args.dimension = "AAP_EGIPWR";
                args.value = value;
                OnStateUpdated(args);

                //PAGE OTHER - POSITION - STEER - WAYPT
                value = decode(partial, 0x3000, 12);
                args.dimension = "AAP_PAGE";
                args.value = value;
                OnStateUpdated(args);

                // Toggle Steerpoint
                value = decode(partial, 0x0c00, 10);
                args.dimension = "AAP_STEER";
                args.value = value;
                OnStateUpdated(args);

                // STEERPT FLTPLAN - MARK - MISSION
                value = decode(partial, 0x0300, 8);
                args.dimension = "AAP_STEERPT";
                args.value = value;
                OnStateUpdated(args);
            }
        }

        public UInt16 decode(UInt16 partial, UInt16 mask, int shift)
        {
            return (UInt16)((partial & mask) >> shift);
        }
       
        public void StartListener()
        {
           
            byte[] address = new byte[2];
            byte[] datalength = new byte[2];
            byte[] buffer;
            byte[] tmp;

            listener = new UdpClient(GroupPort);
            groupEP = new IPEndPoint(GroupAddress, GroupPort);

            try
            {
                listener.JoinMulticastGroup(GroupAddress);
                while (true)
                {
                    buffer = waitForStart(listener, groupEP);
                    
                    while (true)
                    {
                        // get the address of new write command
                        address = readBytes(listener, groupEP, ref buffer, 2);

                        // get the datalength of new write command
                        datalength = readBytes(listener, groupEP, ref buffer, 2);

                        int addressValue = BitConverter.ToUInt16(address, 0);

                        int datalengthValue = BitConverter.ToUInt16(datalength, 0);
                        tmp = readBytes(listener, groupEP, ref buffer, datalengthValue);

                        // update cockpit raw state
                        Array.Copy(tmp, 0, state, addressValue, tmp.Length);
                        // update controls
                        updateControls(addressValue);
                     
                        if (buffer.Length < 1)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Closes the listener
        /// </summary>
        public void StopListener()
        {
            listener.Close();
        }
    }
}
