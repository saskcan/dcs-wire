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

namespace VirtualCockpit
{

    public static class Cockpit
    {
       public static Components components = new Components();
    }
    

    static class Program
    {
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
            // run the forms application
            Application.Run(new Form1());
            
            //listener.StartListener();
        }

        static public void HandleUpdates(object sender, UdpListener.StateUpdatedEventArgs e)
        {
            var args = e.dimension.Split('_');
            Type t = typeof(Components);
            foreach(var t_member in t.GetMembers())
            {
                if(t_member.Name == args[0])
                {
                    FieldInfo t_fi = (FieldInfo)t_member;
                    Type u = t_member.GetType();
                    foreach(var u_member in u.GetMembers())
                    {
                        if(u_member.Name == args[1])
                        {
                            PropertyInfo u_pi = (PropertyInfo)u_member;
                            u_pi.SetValue(t_fi.GetValue(Cockpit.components), e.value);
                        }
                    }
                }
            }
            //if(e.dimension == "AAP_CDUPWR")
            //{
            //    cockpit.AAP.CDUPWR.SetState(e.value);
            //}
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
