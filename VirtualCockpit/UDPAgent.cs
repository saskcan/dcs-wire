using DCSWireUtils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DCSWire 
{
    public class DCSBIOSAgent
    {
        private byte[] startCode;
        private IPAddress GroupAddress;
        private int GroupPort;
        private byte[] state;
        UdpClient listener;
        IPEndPoint groupEP;

        public DCSBIOSAgent(IPAddress address, int portNum, byte[] sc, int stateSize)
        {
            GroupAddress = address;
            GroupPort = portNum;
            startCode = sc;
            state = new byte[stateSize];
        }

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
        public event EventHandler<MessageReadyEventArgs> StateUpdated;

        protected virtual void OnStateUpdated(MessageReadyEventArgs e)
        {
            EventHandler<MessageReadyEventArgs> handler = StateUpdated;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        void decodeAndRaise(int address)
        {
            UInt16 partial = (UInt16)((state[address + 1] << 8) + state[address]);
			string controlGroup;
            Message msg;
            switch (address)
            {
                #region AAP, CDU(1)
                case 0x10fa:
                    controlGroup = "AAP";
                    // AAP CDU Power
                    msg = new Message(controlGroup, "CDUPWR", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AAP EGI Power
                    msg = new Message(controlGroup, "EGIPWR", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AAP PAGE OTHER - POSITION - STEER - WAYPT
                    msg = new Message(controlGroup, "PAGE", "INT");
                    msg.value = (decode(partial, 0x3000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AAP Toggle Steerpoint
                    msg = new Message(controlGroup, "STEER", "INT");
                    msg.value = (decode(partial, 0x0c00, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AAP STEERPT FLTPLAN - MARK - MISSION
                    msg = new Message(controlGroup, "STEERPT", "INT");
                    msg.value = (decode(partial, 0x0300, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CDU DIMBRT Rocker (No Function) 
                    msg = new Message("CDU", "BRT", "INT");
                    msg.value = (decode(partial, 0x0003, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CDU +/- Rocker 
                    msg = new Message("CDU", "DATA", "INT");
                    msg.value = (decode(partial, 0x00c0, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CDU PG Rocker 
                    msg = new Message("CDU", "PG", "INT");
                    msg.value = (decode(partial, 0x000c, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CDU Scroll Waypoint Names (Blank Rocker) 
                    msg = new Message("CDU", "SCROLL", "INT");
                    msg.value = (decode(partial, 0x0030, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region ADI
                // ADI Attitude Warning Flag
                case 0x103a:
                    msg = new Message("ADI", "ATTWARNFLAG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Bank
                case 0x1034:
                    msg = new Message("ADI", "BANK", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Course Warning Flag
                case 0x103c:
                    msg = new Message("ADI", "CRSWARNFLAG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Glide Slope Indicator
                case 0x1044:
                    msg = new Message("ADI", "GS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Glide Slope Warning Flag
                case 0x103e:
                    msg = new Message("ADI", "GSWARNFLAG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Pitch
                case 0x1032:
                    msg = new Message("ADI", "PITCH", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Pitch Trim
                case 0x115e:
                    msg = new Message("ADI", "PITCH_TRIM", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Slipball Position
                case 0x1036:
                    msg = new Message("ADI", "SLIP", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Bank Steering Bar
                case 0x1040:
                    msg = new Message("ADI", "STEERBANK", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Pitch Steering Bar
                case 0x1042:
                    msg = new Message("ADI", "STEERPITCH", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                // ADI Turn Needle
                case 0x1038:
                    msg = new Message("ADI", "TURN", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region AHCP(1), CMSC(1)
                case 0x10e8:
                    controlGroup = "AHCP";
                    // AHCP Altimeter Source RADAR - DELTA - BARO
                    msg = new Message(controlGroup, "ALT_SCE", "INT");
                    msg.value = (decode(partial, 0x0600, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP CICU OFF - ON 
                    msg = new Message(controlGroup, "CICU", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP GUN/PAC GUNARM - SAFE - ARM 
                    msg = new Message(controlGroup, "GUNPAC", "INT");
                    msg.value = (decode(partial, 0x0030, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP Hud Mode NIGHT - DAY 
                    msg = new Message(controlGroup, "HUD_DAYNIGHT", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP  Hud Mode STBY - NORM
                    msg = new Message(controlGroup, "HUD_MODE", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP JTRS OFF - ON 
                    msg = new Message(controlGroup, "JTRS", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // AHCP Laser Arm TRAIN - SAFE - ARM
                    msg = new Message(controlGroup, "LASER_ARM", "INT");
                    msg.value = (decode(partial, 0x00c0, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP Master Arm TRAIN - SAFE - ARM 
                    msg = new Message(controlGroup, "MASTER_ARM", "INT");
                    msg.value = (decode(partial, 0x000c, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AHCP TGP OFF - ON 
                    msg = new Message(controlGroup, "TGP", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSC Toggle between 5 and 16 Priority Threats
                    msg = new Message("CMSC", "PRI", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSC Separate RWR Symbols
                    msg = new Message("CMSC", "SEP", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region AHCP(2)
                case 0x10ee:
                    // IFFCC OFF - TEST - ON 
                    msg = new Message("AHCP", "IFFCC", "INT");
                    msg.value = (decode(partial, 0x0003, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region AOA Indicator
                case 0x1076:
                    // AOA Poweroff Flag 
                    msg = new Message("AOA", "PWROFF", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1078:
                    // AOA Units 
                    msg = new Message("AOA", "UNITS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Accelerometer, Antenna Panel(1)
                case 0x1070:
                    // G Load
                    msg = new Message("ACCEL", "G", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                    
                case 0x1074:
                    // Accelerometer Max Pointer 
                    msg = new Message("ACCEL", "MAX", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1072:
                    // Accelerometer Min Pointer 
                    msg = new Message("ACCEL", "MIN", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x11ba:
                    // Accelerometer Push to Set 
                    msg = new Message("ACCEL", "PTS", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Antenna Panel IFF Antenna Switch
                    msg = new Message("ANT", "IFF", "INT");
                    msg.value = (decode(partial, 0x3000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Antenna Panel UHF Antenna Switch 
                    msg = new Message("ANT", "UHF", "INT");
                    msg.value = (decode(partial, 0xc000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region DVADR
                    // DVADR End of Tape Indicator Light
                    msg = new Message("DVADR", "EOT", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Function Control Toggle Switch
                    msg = new Message("DVADR", "FUNCTION", "INT");
                    msg.value = (decode(partial, 0x000c, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // DVADR Record (On) Indicator Light
                    msg = new Message("DVADR", "REC", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Video Selector Toggle Switch
                    msg = new Message("DVADR", "VIDEO", "INT");
                    msg.value = (decode(partial, 0x0030, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Airspeed Indicator
                case 0x107c:
                    // Airspeed Dial 
                    msg = new Message("AIRSPEED", "DIAL", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x107a:
                    // Airspeed Needle 
                    msg = new Message("AIRSPEED", "NEEDLE", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Altimeter(1)
                case 0x1080:
                    // 10000 FT Counter 
                    msg = new Message("ALT", "10000FTCNT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1082:
                    // 1000 FT Counter 
                    msg = new Message("ALT", "1000FTCNT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x107e:
                    // 100 ft Pointer 
                    msg = new Message("ALT", "100FT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1084:
                    // 100 FT Counter 
                    msg = new Message("ALT", "100FTCNT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1086:
                    // Barometric Pressure Digit 0 
                    msg = new Message("ALT", "PRESSURE0", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1088:
                    // Barometric Pressure Digit 1 
                    msg = new Message("ALT", "PRESSURE1", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x108a:
                    // Barometric Pressure Digit 2 
                    msg = new Message("ALT", "PRESSURE2", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x108c:
                    // Barometric Pressure Digit 3 
                    msg = new Message("ALT", "PRESSURE3", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1142:
                    // Set Pressure 
                    msg = new Message("ALT", "SET_PRESSURE", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Altimeter(2), Auxiliary Light Control Panel(1)  
                case 0x1144:
                    // Altimeter ELECT / PNEU 
                    msg = new Message("ALT", "ELECT_PNEU", "INT");
                    msg.value = (decode(partial, 0x0003, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // Auxiliary Light Control Panel HARS-SAS Override / Norm 
                    msg = new Message("ALCP", "HARSSAS", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                    // Auxiliary Light Control Panel Nightvision Lights 
                    msg = new Message("ALCP", "NVIS_LTS", "INT");
                    msg.value = (decode(partial, 0x6000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                #endregion
                #region Antenna Panel(2)
                case 0x11bc:
                    // EGI HQ TOD Switch 
                    msg = new Message("ANT", "EGIHQTOD", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Glare Shield
                case 0x1158:
                    // APU Fire T-Handle
                    msg = new Message("GLARE", "FIRE_APU_PULL", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Fire Extinguisher Discharge Left/Off/Right
                    msg = new Message("GLARE", "FIRE_EXT_DISCH", "INT");
                    msg.value = (decode(partial, 0x0030, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Left Engine Fire T-Handle
                    msg = new Message("GLARE", "FIRE_LENG_PULL", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Right Engine Fire T-Handle
                    msg = new Message("GLARE", "FIRE_RENG_PULL", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Auxiliary Light Control Panel(2)
                    // Fire Detect Bleed Air Test 
                    msg = new Message("ALCP", "FDBA_TEST", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1156:
                    // Refueling Lighting Dial 
                    msg = new Message("ALCP", "RCVR_LTS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1152:
                    // Refuel Status Indexer Lights 
                    msg = new Message("ALCP", "RSIL", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1154:
                    // Weapon Station Lights (No Function) 
                    msg = new Message("ALCP", "WPNSTA", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1116:
                    // Signal Lights Test 
                    msg = new Message("ALCP", "LAMP_TEST_BTN", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Glare Shield
                    // External Stores Jettison Button
                    msg = new Message("GLARE", "EXT_STORES_JETTISON", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region CDU(2)
                case 0x10f4:
                    controlGroup = "CDU";
                    // 0 
                    msg = new Message(controlGroup, "0", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 1
                    msg = new Message(controlGroup, "1", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 2
                    msg = new Message(controlGroup, "2", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 3
                    msg = new Message(controlGroup, "3", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 4
                    msg = new Message(controlGroup, "4", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 5
                    msg = new Message(controlGroup, "5", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 6
                    msg = new Message(controlGroup, "6", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 7
                    msg = new Message(controlGroup, "7", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 8
                    msg = new Message(controlGroup, "8", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // 9
                    msg = new Message(controlGroup, "9", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // A 
                    msg = new Message(controlGroup, "A", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // FPM 
                    msg = new Message(controlGroup, "FPM", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // OSET
                    msg = new Message(controlGroup, "OSET", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Decimal Point
                    msg = new Message(controlGroup, "POINT", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // PREV 
                    msg = new Message(controlGroup, "PREV", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Slash 
                    msg = new Message(controlGroup, "SLASH", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x10f6:
                    // B 
                    msg = new Message("CDU", "B", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // C
                    msg = new Message("CDU", "C", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // D 
                    msg = new Message("CDU", "D", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // E
                    msg = new Message("CDU", "E", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // F
                    msg = new Message("CDU", "F", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // G
                    msg = new Message("CDU", "G", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // H
                    msg = new Message("CDU", "H", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // I
                    msg = new Message("CDU", "I", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // J
                    msg = new Message("CDU", "J", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // K
                    msg = new Message("CDU", "K", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // L
                    msg = new Message("CDU", "L", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // M
                    msg = new Message("CDU", "M", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // N
                    msg = new Message("CDU", "N", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // O
                    msg = new Message("CDU", "O", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // P
                    msg = new Message("CDU", "P", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Q
                    msg = new Message("CDU", "Q", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x10f8:
                    // BCK 
                    msg = new Message("CDU", "BCK", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CLR 
                    msg = new Message("CDU", "CLR", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // FA 
                    msg = new Message("CDU", "FA", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // MK
                    msg = new Message("CDU", "MK", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // No Function 1
                    msg = new Message("CDU", "NA1", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // No Function 2
                    msg = new Message("CDU", "NA2", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R 
                    msg = new Message("CDU", "R", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // S
                    msg = new Message("CDU", "S", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SPC
                    msg = new Message("CDU", "SPC", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // T
                    msg = new Message("CDU", "T", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // U
                    msg = new Message("CDU", "U", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // V
                    msg = new Message("CDU", "V", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // W
                    msg = new Message("CDU", "W", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // X
                    msg = new Message("CDU", "X", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // Y
                    msg = new Message("CDU", "Y", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // Z
                    msg = new Message("CDU", "Z", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x10f2:
                    // LSK 3L 
                    msg = new Message("CDU", "LSK_3L", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 3R
                    msg = new Message("CDU", "LSK_3R", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 5L
                    msg = new Message("CDU", "LSK_5L", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 5R
                    msg = new Message("CDU", "LSK_5R", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 7L
                    msg = new Message("CDU", "LSK_7L", "INT");
                    msg.value = (decode(partial, 0x080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 7R
                    msg = new Message("CDU", "LSK_7R", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 9L
                    msg = new Message("CDU", "LSK_9L", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LSK 9R
                    msg = new Message("CDU", "LSK_9R", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // NAV
                    msg = new Message("CDU", "NAV", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SYS
                    msg = new Message("CDU", "SYS", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // WP
                    msg = new Message("CDU", "WP", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region CMSC(1)
                case 0x10ea:
                    // Adjust Display Brightness
                    msg = new Message("CMSC", "BRT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10ec:
                    // Adjust RWR Volume
                    msg = new Message("CMSC", "RWR_VOL", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1012:
                    // Missile Launch Indicator
                    msg = new Message("CMSC", "LAUNCH", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Priority Status Indicator
                    msg = new Message("CMSC", "PRIO", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Unknown Status Indicator
                    msg = new Message("CMSC", "UNKNVALUE", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region HUD
                    // Air Refuel READY
                    msg = new Message("HUD", "AIRREFUELREADY", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AOA Indexer High
                    msg = new Message("HUD", "AOAINDEXERHIGH", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AOA Indexer Low
                    msg = new Message("HUD", "AOAINDEXERLOW", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AOA Indexer Normal
                    msg = new Message("HUD", "AOAINDEXERNORMAL", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region CMSC(2), CMSP(1)
                case 0x10e4:
                    // CMSC Select Jammer Program 
                    msg = new Message("CMSC", "JMR", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // CMSC Select MWS Programs (No Function)
                    msg = new Message("CMSC", "MWS", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP Countermeasure Dispenser OFF - ON - (MENU)
                    msg = new Message("CMSP", "DISP", "INT");
                    msg.value = (decode(partial, 0x0600, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP Jammer OFF - ON - (MENU)
                    msg = new Message("CMSP", "JMR", "INT");
                    msg.value = (decode(partial, 0x0060, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP JTSN / OFF
                    msg = new Message("CMSP", "JTSN", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP Mode Select
                    msg = new Message("CMSP", "MODE", "INT");
                    msg.value = (decode(partial, 0x3800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP Missile Warning System OFF - ON - (MENU)
                    msg = new Message("CMSP", "MWS", "INT");
                    msg.value = (decode(partial, 0x0018, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP Radar Warning Receiver OFF - ON - (MENU)
                    msg = new Message("CMSP", "RWR", "INT");
                    msg.value = (decode(partial, 0x0180, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP Cycle Program or Value Up/Down
                    msg = new Message("CMSP", "UPDN", "INT");
                    msg.value = (decode(partial, 0x0003, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region CMSP(2)
                case 0x10e0:
                    // SET Button 1
                    msg = new Message("CMSP", "ARW1", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10e2:
                    // SET Button 2
                    msg = new Message("CMSP", "ARW2", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SET Button 3
                    msg = new Message("CMSP", "ARW3", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SET Button 4
                    msg = new Message("CMSP", "ARW4", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CMSP RTN
                    msg = new Message("CMSP", "RTN", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x10e6:
                    // Brightness 
                    msg = new Message("CMSP", "BRT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Caution Lights Panel
                case 0x10d4:
                    // Engine Start Cycle 
                    msg = new Message("CL", "A1", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-HYD PRESS
                    msg = new Message("CL", "A2", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-HYD PRESS
                    msg = new Message("CL", "A3", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // GUN UNSAFE
                    msg = new Message("CL", "A4", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ANTI-SKID
                    msg = new Message("CL", "B1", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-HYD RES
                    msg = new Message("CL", "B2", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-HYD RES
                    msg = new Message("CL", "B3", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // OXY LOW
                    msg = new Message("CL", "B4", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ELEV DISENG
                    msg = new Message("CL", "C1", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // VOID1
                    msg = new Message("CL", "C2", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SEAT NOT ARMED
                    msg = new Message("CL", "C3", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // BLEED AIR LEAK
                    msg = new Message("CL", "C4", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AIL DISENG
                    msg = new Message("CL", "D1", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-AIL TAB
                    msg = new Message("CL", "D2", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-AIL TAB 
                    msg = new Message("CL", "D3", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SERVICE AIR HOT
                    msg = new Message("CL", "D4", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10d6:
                    // PITCH SAS
                    msg = new Message("CL", "E1", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-ENG HOT
                    msg = new Message("CL", "E2", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // R-ENG HOT
                    msg = new Message("CL", "E3", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // WINDSHIELD HOT
                    msg = new Message("CL", "E4", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // YAW SAS 
                    msg = new Message("CL", "F1", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-ENG OIL PRESS 
                    msg = new Message("CL", "F2", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-ENG OIL PRESS 
                    msg = new Message("CL", "F3", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CICU
                    msg = new Message("CL", "F4", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // GCAS
                    msg = new Message("CL", "G1", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-MAIN PUMP
                    msg = new Message("CL", "G2", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-MAIN PUMP
                    msg = new Message("CL", "G3", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // VOID2
                    msg = new Message("CL", "G4", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // LASTE
                    msg = new Message("CL", "H1", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-WING PUMP
                    msg = new Message("CL", "H2", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-WING PUMP
                    msg = new Message("CL", "H3", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // HARS
                    msg = new Message("CL", "H4", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10d8:
                    // IFF MODE-4
                    msg = new Message("CL", "I1", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-MAIN FUEL LOW
                    msg = new Message("CL", "I2", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-MAIN FUEL LOW
                    msg = new Message("CL", "I3", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-R TKS UNEQUAL
                    msg = new Message("CL", "I4", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // EAC
                    msg = new Message("CL", "J1", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-FUEL PRESS
                    msg = new Message("CL", "J2", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-FUEL PRESS 
                    msg = new Message("CL", "J3", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // NAV
                    msg = new Message("CL", "J4", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // STALL SYS
                    msg = new Message("CL", "K1", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-CONV
                    msg = new Message("CL", "K2", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-CONV
                    msg = new Message("CL", "K3", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CADC
                    msg = new Message("CL", "K4", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // APU GEN
                    msg = new Message("CL", "L1", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // L-GEN
                    msg = new Message("CL", "L2", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // R-GEN
                    msg = new Message("CL", "L3", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // INST INV
                    msg = new Message("CL", "L4", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Environment Control Panel(1)
                case 0x1138:
                    // ENVCP Air Conditioner MAN/AUTO/COLD/HOT
                    msg = new Message("ENVCP", "AC_OPER", "INT");
                    msg.value = (decode(partial, 0x0300, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENVCP Main Air Supply
                    msg = new Message("ENVCP", "AIR_SUPPLY", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENVCP Bleed Air
                    msg = new Message("ENVCP", "BLEED_AIR", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENVCP Pitot Heat 
                    msg = new Message("ENVCP", "PITOT_HEAT", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENVCP Temperature/Pressure Control
                    msg = new Message("ENVCP", "TEMP_PRESS", "INT");
                    msg.value = (decode(partial, 0x000c, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Circuit Breaker Panel
                    // AILERON DISC L Circuit Breaker
                    msg = new Message("CBP", "AILERON_DISC_L", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AILERON DISC R Circuit Breaker
                    msg = new Message("CBP", "AILERON_DISC_R", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // AILERON TAB L Circuit Breaker
                    msg = new Message("CBP", "AILERON_TAB_L", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ELEVATION DISC L Circuit Breaker
                    msg = new Message("CBP", "ELEVATION_DISC_L", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ELEVATION DISC R Circuit Breaker
                    msg = new Message("CBP", "ELEVATION_DISC_R", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x113e:
                    // AILERON TAB R Circuit Breaker
                    msg = new Message("CBP", "AILERON_TAB_R", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // APU CONT Circuit Breaker
                    msg = new Message("CBP", "APU_CONT", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // BLEED AIR CONT L Circuit Breaker
                    msg = new Message("CBP", "BLEED_AIR_CONT_L", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // BLEED AIR CONT R Circuit Breaker
                    msg = new Message("CBP", "BLEED_AIR_CONT_R", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // DC FUEL PUMP Circuit Breaker
                    msg = new Message("CBP", "DC_FUEL_PUMP", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // EMER FLAP Circuit Breaker
                    msg = new Message("CBP", "EMER_FLAP", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // EMER TRIM Circuit Breaker
                    msg = new Message("CBP", "EMER_TRIM", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENG IGNITOR 1 Circuit Breaker
                    msg = new Message("CBP", "ENG_IGNITOR_1", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENG IGNITOR 2 Circuit Breaker
                    msg = new Message("CBP", "ENG_IGNITOR_2", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENGINE START L Circuit Breaker
                    msg = new Message("CBP", "ENG_START_L", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // ENGINE START R Circuit Breaker
                    msg = new Message("CBP", "ENG_START_R", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // EXT STORES JETT 1 Circuit Breaker
                    msg = new Message("CBP", "EXT_STORES_JETT_1", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // EXT STORES JETT 2 Circuit Breaker
                    msg = new Message("CBP", "EXT_STORES_JETT_2", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // FUEL SHUTOFF L Circuit Breaker
                    msg = new Message("CBP", "FUEL_SHUTOFF_L", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // FUEL SHUTOFF R Circuit Breaker
                    msg = new Message("CBP", "FUEL_SHUTOFF_R", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // GEAR Circuit Breaker
                    msg = new Message("CBP", "GEAR", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1140:
                    // AUX ESS BUS 0A Circuit Breaker
                    msg = new Message("CBP", "AUX_ESS_BUS_0A", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AUX ESS BUS 0B Circuit Breaker
                    msg = new Message("CBP", "AUX_ESS_BUS_0B", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AUX ESS BUS 0C Circuit Breaker
                    msg = new Message("CBP", "AUX_ESS_BUS_0C", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // AUX ESS BUS TIE Circuit Breaker
                    msg = new Message("CBP", "AUX_ESS_BUS_TIE", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // BAT BUS TRANS Circuit Breaker
                    msg = new Message("CBP", "BAT_BUS_TRANS", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // CONVERTER L Circuit Breaker
                    msg = new Message("CBP", "CONVERTER_L", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // GEN CONT L Circuit Breaker
                    msg = new Message("CBP", "GEN_CONT_L", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // GEN CONT R Circuit Breaker
                    msg = new Message("CBP", "GEN_CONT_R", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // IFF Circuit Breaker
                    msg = new Message("CBP", "IFF", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // INTERCOM Circuit Breaker
                    msg = new Message("CBP", "INTERCOM", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // INVERTER CONT Circuit Breaker
                    msg = new Message("CBP", "INVERTER_CONT", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // INVERTER PWR Circuit Breaker
                    msg = new Message("CBP", "INVERTER_PWR", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // MASTER CAUTION Circuit Breaker
                    msg = new Message("CBP", "MASTER_CAUTION", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // PITOT HEAT AC Circuit Breaker
                    msg = new Message("CBP", "PITOT_HEAT_AC", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // SPS RUDDER AUTH LIM Circuit Breaker
                    msg = new Message("CBP", "SPS_RUDDER_AUTH_LIM", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // STBY ATT IND Circuit Breaker
                    msg = new Message("CBP", "STBY_ATT_IND", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // UHF Circuit Breaker
                    msg = new Message("CBP", "UHF", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Digital Clock(1)
                case 0x10fc:
                    // Clock CTRL
                    msg = new Message("CLOCK", "CTRL", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Clock SET 
                    msg = new Message("CLOCK", "SET", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Fuel System Control Panel
                    // Signal Amplifier NORM - OVERRIDE
                    msg = new Message("FSCP", "AMPL", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Boost Pumps Main Fuselage Left
                    msg = new Message("FSCP", "BOOST_MAIN_L", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // Boost Pumps Main Fuselage Right 
                    msg = new Message("FSCP", "BOOST_MAIN_R", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Boost Pumps Left Wign
                    msg = new Message("FSCP", "BOOST_WING_L", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Boost Pumps Left Wign
                    msg = new Message("FSCP", "BOOST_WING_R", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Crossfeed
                    msg = new Message("FSCP", "CROSSFEED", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // External Fuselage Tanks Boost Pumps 
                    msg = new Message("FSCP", "EXT_TANKS_FUS", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // External Wing Tanks Boost Pumps 
                    msg = new Message("FSCP", "EXT_TANKS_WING", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Fill Disable Main Left
                    msg = new Message("FSCP", "FD_MAIN_L", "INT");
                    msg.value = (decode(partial, 0x4000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Fill Disable Main Right 
                    msg = new Message("FSCP", "FD_MAIN_R", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Fill Disable Wing Left 
                    msg = new Message("FSCP", "FD_WING_L", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Fill Disable Wing Right 
                    msg = new Message("FSCP", "FD_WING_R", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // Line Check
                    msg = new Message("FSCP", "LINE_CHECK", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // TK Gate
                    msg = new Message("FSCP", "TK_GATE", "INT");
                    msg.value = (decode(partial, 0x0010, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Electrical Power Panel(1)
                case 0x110c:
                    // AC GEN PWR Left
                    msg = new Message("EPP", "AC_GEN_PWR_L", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // APU GEN PWR
                    msg = new Message("EPP", "APU_GEN_PWR", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Inverter TEST - OFF - STBY
                    msg = new Message("EPP", "INVERTER", "INT");
                    msg.value = (decode(partial, 0x6000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Emergency Flight Control Panel
                    // Aileron Emergency Disengage LEFT - OFF - RIGHT
                    msg = new Message("EFCP", "AILERON_EMER_DISENGAGE", "INT");
                    msg.value = (decode(partial, 0x00c0, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Elevator Emergency Disengage LEFT - OFF - RIGHT
                    msg = new Message("EFCP", "ELEVATOR_EMER_DISENGAGE", "INT");
                    msg.value = (decode(partial, 0x0300, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Emergency Trim CENTER - NOSE DN - RWD - NOSE UP - LWD
                    msg = new Message("EFCP", "EMER_TRIM", "INT");
                    msg.value = (decode(partial, 0x0038, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Flaps Emergency Retract
                    msg = new Message("EFCP", "FLAPS_EMER_RETR", "INT");
                    msg.value = (decode(partial, 0x0400, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Manual Revision Flight Control System MAN REVERSION - NORM
                    msg = new Message("EFCP", "MRFCS", "INT");
                    msg.value = (decode(partial, 0x0800, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Speed Brake Emergency Retract
                    msg = new Message("EFCP", "SPDBK_EMER_RETR", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Pitch/Roll Trim Override EMER - NORM
                    msg = new Message("EFCP", "TRIM_OVERRIDE", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Electrical Power Panel(2)
                case 0x1110:
                    // AC GEN PWR Right 
                    msg = new Message("EPP", "AC_GEN_PWR_R", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Battery Power
                    msg = new Message("EPP", "BATTERY_PWR", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Emergency Flood Light
                    msg = new Message("EPP", "EMER_FLOOD", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10da:
                    // Left Aileron EMER DISENG Indicator
                    msg = new Message("EFCP", "LAILERONEMERDISENGAGE", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Left Elevator EMER DISENG Indicator
                    msg = new Message("EFCP", "LELEVATOREMERDISENGAGE", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Right Aileron EMER DISENG Indicator
                    msg = new Message("EFCP", "RAILERONEMERDISENGAGE", "INT");
                    msg.value = (decode(partial, 0x0080, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Right Elevator EMER DISENG Indicator
                    msg = new Message("EFCP", "RELEVATOREMERDISENGAGE", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Glare Shield
                    // APU Fire Indicator
                    msg = new Message("GLARE", "APUFIRE", "INT");
                    msg.value = (decode(partial, 0x0010, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Left Engine Fire Indicator
                    msg = new Message("GLARE", "LENGFIRE", "INT");
                    msg.value = (decode(partial, 0x0008, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Right Engine Fire Indicator
                    msg = new Message("GLARE", "RENGFIRE", "INT");
                    msg.value = (decode(partial, 0x0020, 5).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Front Dash 
                    // CANOPY UNLOCKED Indicator
                    msg = new Message("DASH", "CANOPYUNLOCKED", "INT");
                    msg.value = (decode(partial, 0x0004, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // MARKER BEACON Indicator
                    msg = new Message("DASH", "MARKERBEACON", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Nosewheel Steering Indicator
                    msg = new Message("DASH", "NOSEWHEELSTEERING", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1026:
                    // GUN READY Indicator
                    msg = new Message("DASH", "GUNREADY", "INT");
                    msg.value = (decode(partial, 0x0002, 1).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region HUD
                    // Air Refuel DISCONNECT
                    msg = new Message("HUD", "AIRREFUELDISCONNECT", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Air Refuel LATCHED
                    msg = new Message("HUD", "AIRREFUELLATCHED", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Engine Instruments
                case 0x10be:
                    // APU RPM Gauge
                    msg = new Message("ENGINE", "APURPM", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10c0:
                    // APU Temperature Gauge
                    msg = new Message("ENGINE", "APUTEMP", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10a8:
                    // Left Engine Core Speed 
                    msg = new Message("ENGINE", "LENGCORE", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10a6:
                    // Left Engine Core Speed Tenth
                    msg = new Message("ENGINE", "LENGCORET", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10a2:
                    // Left Engine Fan Speed
                    msg = new Message("ENGINE", "LENGFAN", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10ae:
                    // Left Engine Fuel Flow
                    msg = new Message("ENGINE", "LENGFUELFLOW", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10b4:
                    // Left Engine Temperature
                    msg = new Message("ENGINE", "LENGTEMP", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10ba:
                    // Left Engine Temperature Off
                    msg = new Message("ENGINE", "LENGTEMPOFF", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10b2:
                    // Left Engine Temperature Tenth
                    msg = new Message("ENGINE", "LENGTEMPT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10c2:
                    // Left Hydraulic Pressure 
                    msg = new Message("ENGINE", "LHYDPRESS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10c6:
                    // Left Oil Pressure
                    msg = new Message("ENGINE", "LOILPRESS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10ac:
                    // Right Engine Core Speed 
                    msg = new Message("ENGINE", "RENGCORE", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10aa:
                    // Right Engine Core Speed Tenth
                    msg = new Message("ENGINE", "RENGCORET", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10a4:
                    // Right Engine Fan Speed
                    msg = new Message("ENGINE", "RENGFAN", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10b0:
                    // Right Engine Fuel Flow
                    msg = new Message("ENGINE", "RENGFUELFLOW", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10b8:
                    // Right Engine Temperature
                    msg = new Message("ENGINE", "RENGTEMP", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10bc:
                    // Right Engine Temperature Off
                    msg = new Message("ENGINE", "RENGTEMPOFF", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10b6:
                    // Right Engine Temperature Tenth
                    msg = new Message("ENGINE", "RENGTEMPT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10c4:
                    // Right Hydraulic Pressure 
                    msg = new Message("ENGINE", "RHYDPRESS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10c8:
                    // Right Oil Pressure
                    msg = new Message("ENGINE", "ROILPRESS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Environment Control Panel(2)
                case 0x1134:
                    // Cabin Pressure Altitude 
                    msg = new Message("ENVCP", "CABINPRESSALT", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1136:
                    // Canopy Defog
                    msg = new Message("ENVCP", "CANOPY_DEFOG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x113a:
                    // Flow Level
                    msg = new Message("ENVCP", "FLOW_LEVEL", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x112a:
                    // Oxygen Indicator Test
                    msg = new Message("ENVCP", "OXY_TEST", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Windshield Defog/Deice
                    msg = new Message("ENVCP", "WINDSHIELD_DEFOG", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Windshield Rain Removal/Wash
                    msg = new Message("ENVCP", "WRRW", "INT");
                    msg.value = (decode(partial, 0xc000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region IFF
                    // Mode-3A Wheel 3
                    msg = new Message("IFF", "MODE3A_WHEEL3", "INT");
                    msg.value = (decode(partial, 0x0007, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Mode-3A Wheel 4
                    msg = new Message("IFF", "MODE3A_WHEEL4", "INT");
                    msg.value = (decode(partial, 0x0038, 3).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // TEST Push to Test
                    msg = new Message("IFF", "TEST_TEST", "INT");
                    msg.value = (decode(partial, 0x0040, 6).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Environmental Control Panel(3)
                case 0x113c:
                    // Temp Level Control
                    msg = new Message("ENVCP", "TEMP_LEVEL", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1132:
                    // Oxygen Volume (0 to 5 liters) 
                    msg = new Message("ENVCP", "OXYVOLUME", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region Fuel System Control Panel
                case 0x1106:
                    // Aerial Refueling Slipway Control Lever
                    msg = new Message("FSCP", "RCVR_LEVER", "INT");
                    msg.value = (decode(partial, 0x0100, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                #endregion
                #region Fuel Panel
                    // Fuel Display Selector
                    msg = new Message("FQIS", "SELECT", "INT");
                    msg.value = (decode(partial, 0x1c00, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Fuel Gauge Test
                    msg = new Message("FQIS", "TEST", "INT");
                    msg.value = (decode(partial, 0x0200, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10d2:
                    // Fuel Quantity Counter 100
                    msg = new Message("FQIS", "FUELQTY100", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10d0:
                    // Fuel Quantity Counter 1000
                    msg = new Message("FQIS", "FUELQTY1000", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10ce:
                    // Fuel Quantity Counter 10000
                    msg = new Message("FQIS", "FUELQTY10000", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10ca:
                    // Fuel Qty Left 
                    msg = new Message("FQIS", "FUELQTYL", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x10cc:
                    // Fuel Qty Right 
                    msg = new Message("FQIS", "FUELQTYR", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region HARS
                case 0x11a6:
                    // HARS Fast Erect Button 
                    msg = new Message("HARS", "FAST_ERECT", "INT");
                    msg.value = (decode(partial, 0x0080, 11).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // HARS MAG VAR
                    msg = new Message("HARS", "MAGVAR", "INT");
                    msg.value = (decode(partial, 0xc000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // HARS N/S toggle switch
                    msg = new Message("HARS", "NS", "INT");
                    msg.value = (decode(partial, 0x2000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // HARS SLAVE-DG Mode
                    msg = new Message("HARS", "SLAVE_DG", "INT");
                    msg.value = (decode(partial, 0x1000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x11b6:
                    // HARS Heading
                    msg = new Message("HARS", "HDG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x11b4:
                    // HARS Latitude Dial
                    msg = new Message("HARS", "LATITUDE", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x11b8:
                    // HARS Push-to-Sync
                    msg = new Message("HARS", "PTS", "INT");
                    msg.value = (decode(partial, 0x0001, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x106c:
                    // HARS Sync
                    msg = new Message("HARS", "SYNC", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region HSI
                case 0x104e:
                    // HSI Bearing Pointer 1
                    msg = new Message("HSI", "BEARING1", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1050:
                    // HSI Bearing Pointer 2
                    msg = new Message("HSI", "BEARING2", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x104a:
                    // HSI Bearing Flag
                    msg = new Message("HSI", "BEARINGFLAG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1056:
                    // HSI Course Counter A
                    msg = new Message("HSI", "CCA", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1058:
                    // HSI Course Counter B
                    msg = new Message("HSI", "CCB", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;

                case 0x1054:
                    // HSI Course
                    msg = new Message("HSI", "CRS", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x115a:
                    // Course Select Knob
                    msg = new Message("HSI", "CRS_KNOB", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1062:
                    // HSI Deviation
                    msg = new Message("HSI", "DEVIATION", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x104c:
                    // HSI Heading
                    msg = new Message("HSI", "HDG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1052:
                    // HSI Heading Bug
                    msg = new Message("HSI", "HDGBUG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x115c:
                    // Heading Select Knob
                    msg = new Message("HSI", "HDGKNOB", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1046:
                    // HSI Poweroff Flag
                    msg = new Message("HSI", "PWROFFFLAG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1048:
                    // HSI Range Flag
                    msg = new Message("HSI", "RANGEFLAG", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x105a:
                    // HSI Range Counter A
                    msg = new Message("HSI", "RCA", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x105c:
                    // HSI Range Counter B
                    msg = new Message("HSI", "RCB", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x105e:
                    // HSI Range Counter C
                    msg = new Message("HSI", "RCC", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1060:
                    // HSI Range Counter D
                    msg = new Message("HSI", "RCD", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1064:
                    // HSI TO/FROM 1
                    msg = new Message("HSI", "TOFROM1", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1066:
                    // HSI TO/FROM 2
                    msg = new Message("HSI", "TOFROM2", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                #region IFF
                case 0x111a:
                    // IFF Code: ZERO - B - A - (HOLD)
                    msg = new Message("IFF", "CODE", "INT");
                    msg.value = (decode(partial, 0xc000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x111e:
                    // IFF Master: OFF - STBY - LOW - NORM - EMER
                    msg = new Message("IFF", "MASTER", "INT");
                    msg.value = (decode(partial, 0xe000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                #endregion
                case 0x1128:
                    // Mic Ident
                    msg = new Message("IFF", "MIC_IDENT", "INT");
                    msg.value = (decode(partial, 0x000c, 2).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Mode-1 Wheel 1
                    msg = new Message("IFF", "MODE1_WHEEL1", "INT");
                    msg.value = (decode(partial, 0x0070, 4).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Mode-1 Wheel 2
                    msg = new Message("IFF", "MODE1_WHEEL2", "INT");
                    msg.value = (decode(partial, 0x0180, 7).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Mode-3A Wheel 1
                    msg = new Message("IFF", "MODE3A_WHEEL1", "INT");
                    msg.value = (decode(partial, 0x0e00, 9).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Mode-3A Wheel 2
                    msg = new Message("IFF", "MODE3A_WHEEL2", "INT");
                    msg.value = (decode(partial, 0x7000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // RAD Test/Mon
                    msg = new Message("IFF", "RADTEST", "INT");
                    msg.value = (decode(partial, 0x0003, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // REPLY Push to Test
                    msg = new Message("IFF", "REPLY_TEST", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1122:
                    // IFF On/Out
                    msg = new Message("IFF", "ON_OUT", "INT");
                    msg.value = (decode(partial, 0x8000, 15).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // IFF Out: LIGHT - OFF - AUDIO
                    msg = new Message("IFF", "OUT_AUDIO_LIGHT", "INT");
                    msg.value = (decode(partial, 0x6000, 13).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x112c:
                    // IFF Reply Dim
                    msg = new Message("IFF", "REPLY_DIM", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x112e:
                    // TEST Reply Dim
                    msg = new Message("IFF", "TEST_DIM", "INT");
                    msg.value = (decode(partial, 0xffff, 0).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
                case 0x1126:
                    // Test M-1
                    msg = new Message("IFF", "TEST_M1", "INT");
                    msg.value = (decode(partial, 0x0300, 8).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Test M-2
                    msg = new Message("IFF", "TEST_M2", "INT");
                    msg.value = (decode(partial, 0x0c00, 10).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    
                    // Test M-3
                    msg = new Message("IFF", "TEST_M3", "INT");
                    msg.value = (decode(partial, 0x3000, 12).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));

                    // Test M-4
                    msg = new Message("IFF", "TEST_M4", "INT");
                    msg.value = (decode(partial, 0xc000, 14).ToString()); 
                    OnStateUpdated(new MessageReadyEventArgs(msg));
                    break;
            }
            #region Strings
            #region CDU Display
            if(0x11c0 <= address && address <= 0x11d7)
            {
                // CDU Line 1
                msg = new Message("CDU", "LINE0BUFFER", "STRING");
                msg.value = decodeString(0x11c0, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x11d8 <= address && address <= 0x11ef)
            {
                // CDU Line 2
                msg = new Message("CDU", "LINE1BUFFER", "STRING");
                msg.value = decodeString(0x11d8, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x11f0 <= address && address <= 0x1207)
            {
                // CDU Line 3
                msg = new Message("CDU", "LINE2BUFFER", "STRING");
                msg.value = decodeString(0x11f0, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1208 <= address && address <= 0x121f)
            {
                // CDU Line 4
                msg = new Message("CDU", "LINE3BUFFER", "STRING");
                msg.value = decodeString(0x1208, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1220 <= address && address <= 0x1237)
            {
                // CDU Line 5
                msg = new Message("CDU", "LINE4BUFFER", "STRING");
                msg.value = decodeString(0x1220, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1238 <= address && address <= 0x124f)
            {
                // CDU Line 6
                msg = new Message("CDU", "LINE5BUFFER", "STRING");
                msg.value = decodeString(0x1238, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1250 <= address && address <= 0x1267)
            {
                // CDU Line 7
                msg = new Message("CDU", "LINE6BUFFER", "STRING");
                msg.value = decodeString(0x1250, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1268 <= address && address <= 0x127f)
            {
                // CDU Line 8
                msg = new Message("CDU", "LINE7BUFFER", "STRING");
                msg.value = decodeString(0x1268, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1280 <= address && address <= 0x1297)
            {
                // CDU Line 9
                msg = new Message("CDU", "LINE8BUFFER", "STRING");
                msg.value = decodeString(0x1280, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1298 <= address && address <= 0x12af)
            {
                // CDU Line 10
                msg = new Message("CDU", "LINE9BUFFER", "STRING");
                msg.value = decodeString(0x1298, 24);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            #endregion
            #region CMSC
            else if(0x10e8 <= address && address <= 0x10ef)
            {
                // CMSC Chaff / Flare Amount Display
                msg = new Message("CMSC", "TXTCHAFFFLAREBUFFER", "STRING");
                msg.value = decodeString(0x10e8, 8);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1096 <= address && address <= 0x109d )
            {
                // CMSC JMR Status Display
                msg = new Message("CMSC", "TXTJMRBUFFER", "STRING");
                msg.value = decodeString(0x1096, 8);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x12b0 <= address && address <= 0x12b7 )
            {
                // CMSC MWS Status Display
                msg = new Message("CMSC", "TXTMWSBUFFER", "STRING");
                msg.value = decodeString(0x12b0, 8);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            #endregion
            #region CMSP
            else if(0x1000 <= address && address <= 0x1012 )
            {
                // CMSP Display Line 1
                msg = new Message("CMSP", "1BUFFER", "STRING");
                msg.value = decodeString(0x1000, 19);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1014 <= address && address <= 0x1026 )
            {
                // CMSP Display Line 2
                msg = new Message("CMSP", "2BUFFER", "STRING");
                msg.value = decodeString(0x1014, 19);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            #endregion
            #region Digital Clock(2)
            else if(0x1104 <= address && address <= 0x1106)
            {
                // Clock ETC display ('ET', 'C', or three spaces)
                msg = new Message("CLOCK", "ETCBUFFER", "STRING");
                msg.value = decodeString(0x1104, 3);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x10fe <= address && address <= 0x10ff)
            {
                // Clock Hours (or two spaces)
                msg = new Message("CLOCK", "HHBUFFER", "STRING");
                msg.value = decodeString(0x10fe, 2);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1100 <= address && address <= 0x1101)
            {
                // Clock Hours (or two spaces)
                msg = new Message("CLOCK", "MMBUFFER", "STRING");
                msg.value = decodeString(0x1100, 2);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            else if(0x1102 <= address && address <= 0x1103)
            {
                // Clock Hours (or two spaces)
                msg = new Message("CLOCK", "SSBUFFER", "STRING");
                msg.value = decodeString(0x1102, 2);
                OnStateUpdated(new MessageReadyEventArgs(msg));
            }
            #endregion
            #endregion
        }

        string decodeString(int start, int length)
        {
            byte[] buff = new byte[length];
            Array.Copy(state, start, buff, 0, length);
            return System.Text.Encoding.Default.GetString(buff);
        }

        private Byte[] encode(Message msg)
        {
            string toSend = msg.control + " " + msg.value + "\n";
            if(msg.sendControlGroupViaUDP)
            {
                toSend = msg.controlGroup + "_" + toSend;
            }
            return Encoding.ASCII.GetBytes(toSend);
        }
        
        public UInt16 decode(UInt16 partial, UInt16 mask, int shift)
        {
            return (UInt16)((partial & mask) >> shift);
        }
       
        public void StartAgent()
        {
           
            byte[] address = new byte[2];
            byte[] datalength = new byte[2];
            byte[] buffer;
            byte[] tmp;

            listener = new UdpClient(GroupPort);
            groupEP = new IPEndPoint(GroupAddress, GroupPort);
            int addressValue;
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

                        /*int*/ addressValue = BitConverter.ToUInt16(address, 0);

                        int datalengthValue = BitConverter.ToUInt16(datalength, 0);
                        tmp = readBytes(listener, groupEP, ref buffer, datalengthValue);

                        // update cockpit raw state
                        Array.Copy(tmp, 0, state, addressValue, tmp.Length);
                        // decode data and raise events for the updated controls 
                        decodeAndRaise(addressValue);
                     
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

        public void SendMessage(Message msg)
        {
            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try
            {
                client.Connect("localhost", 7778);
                Byte[] data = encode(msg); 
                client.Send(data, data.Length);
                client.Close();
            }
            catch (Exception ex)
            {

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
