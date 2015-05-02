using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCSWire;
using DCSWireUtils;

namespace VirtualCockpit
{
    public partial class Form1 : Form
    {
		// An event that clients can use to be notified whenever a message is ready
		public event MessageReadyEventHandler MessageReady;

		// Invoke the Ready event; called whenever a message is ready
		protected virtual void OnMessageReady(MessageReadyEventArgs e)
		{
			if (MessageReady != null)
				MessageReady(this, e);
		}

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Program.cockpit.StateUpdated += ValueChanged;

        }

        private string nextValueString(int pos, int max)
        {
            if(pos < max)
            {
                return (pos + 1).ToString();
            }
            else
            {
                return "0";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // find the current value of the label
            var currentValue = Convert.ToInt16(AAP_CDUPWRlabel.Text);

            // send the next value in the message
            var msg = new DCSWireUtils.Message("AAP", "CDUPWR", "INT", nextValueString(currentValue, 1));
            OnMessageReady(new MessageReadyEventArgs(msg));
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            // find the current value of the label
            var currentValue = Convert.ToInt16(AAP_EGIPWRlabel.Text);

            // send the next value in the message
            var msg = new DCSWireUtils.Message("AAP", "EGIPWR", "INT", nextValueString(currentValue, 1));
            OnMessageReady(new MessageReadyEventArgs(msg));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // find the current value of the label
            var currentValue = Convert.ToInt16(AAP_PAGElabel.Text);

            // send the next value in the message
            var msg = new DCSWireUtils.Message("AAP", "PAGE", "INT", nextValueString(currentValue, 3));
            OnMessageReady(new MessageReadyEventArgs(msg));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // find the current value of the label
            var currentValue = Convert.ToInt16(AAP_STEERlabel.Text);

            // send the next value in the message
            var msg = new DCSWireUtils.Message("AAP", "STEER", "INT", nextValueString(currentValue, 2));
            OnMessageReady(new MessageReadyEventArgs(msg));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // find the current value of the label
            var currentValue = Convert.ToInt16(AAP_STEERPTlabel.Text);

            // send the next value in the message
            var msg = new DCSWireUtils.Message("AAP", "STEERPT", "INT", nextValueString(currentValue, 2));
            OnMessageReady(new MessageReadyEventArgs(msg));
        }
        
        delegate void SetTextCallback(string text, Label label);

        private void ValueChanged(object sender, MessageReadyEventArgs e)
        {
            // AAP
            if (e.message.controlGroup == "AAP")
            {
                // CDUPWR
                if(e.message.control == "CDUPWR")
                {
                    SetText(e.message.value, this.AAP_CDUPWRlabel);
                }
                else if(e.message.control == "EGIPWR")
                {
                    SetText(e.message.value, this.AAP_EGIPWRlabel);
                }
                else if(e.message.control == "PAGE")
                {
                    SetText(e.message.value, this.AAP_PAGElabel);
                }
                else if(e.message.control == "STEER")
                {
                    SetText(e.message.value, this.AAP_STEERlabel);
                }
                else if(e.message.control == "STEERPT")
                {
                    SetText(e.message.value, this.AAP_STEERPTlabel);
                }
            }
        }

        private void SetText(string text, Label label)
        {
            if (label.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text, label });
            }
            else
            {
                label.Text = text;
            }
        }

    }
}
