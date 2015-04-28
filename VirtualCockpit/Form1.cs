using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualCockpit
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            Cockpit.components.AAP.CDUPWR.Changed += ValueChanged;
            Cockpit.components.AAP.EGIPWR.Changed += ValueChanged;
            Cockpit.components.AAP.PAGE.Changed += ValueChanged;
            Cockpit.components.AAP.STEER.Changed += ValueChanged;
            Cockpit.components.AAP.STEERPT.Changed += ValueChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void bump(Controllable.MultiPositionSwitch input)
        {
            int currentPosition = input.Position;
            int newPosition;
            if (currentPosition < input.MaxPosition)
            {
                newPosition = currentPosition + 1;
            }
            else
            {
                newPosition = 0;
            }
            input.SetState(newPosition);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var currentState = Cockpit.components.AAP.CDUPWR.Position.ToString();
            if (AAP_CDUPWRlabel.Text == currentState)
            {
                bump(Cockpit.components.AAP.CDUPWR);
            }
            AAP_CDUPWRlabel.Text = Cockpit.components.AAP.CDUPWR.Position.ToString();
        }
        
        // TODO: there are problems with the following event handlers
        private void button2_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.EGIPWR);
            AAP_EGIPWRlabel.Text = Cockpit.components.AAP.EGIPWR.Position.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.PAGE);
            AAP_PAGElabel.Text = Cockpit.components.AAP.PAGE.Position.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.STEER);
            AAP_STEERlabel.Text = Cockpit.components.AAP.STEER.Position.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.STEERPT);
            AAP_STEERPTlabel.Text = Cockpit.components.AAP.STEERPT.Position.ToString();
        }

        delegate void SetTextCallback(string text, Label label);

        private void ValueChanged(object sender, EventArgs e)
        {
            var input = (VirtualCockpit.Controllable.MultiPositionSwitch)sender;
            string name = input.Name;
            int value = input.Position;
            Program.SendUDP(name + " " + value.ToString());
            string[] desc = name.Split('_');

            DCSWireUtils.Message msg = new DCSWireUtils.Message();
            msg.controlGroup = desc[0];
            msg.control = desc[1];
            msg.type = "INT";
            msg.value = value.ToString();
            msg.Encode();
            Program.SendSerial(msg, Program.port);

            if (name == "AAP_CDUPWR")
            {
                SetText(value.ToString(), this.AAP_CDUPWRlabel);
            }
            else if (name == "AAP_EGIPWR")
            {
                SetText(value.ToString(), this.AAP_EGIPWRlabel);
            }
            else if (name == "AAP_PAGE")
            {
                SetText(value.ToString(), this.AAP_PAGElabel);
            }
            else if (name == "AAP_STEER")
            {
                SetText(value.ToString(), this.AAP_STEERlabel);
            }
            else if (name == "AAP_STEERPT")
            {
                SetText(value.ToString(), this.AAP_STEERPTlabel);
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
