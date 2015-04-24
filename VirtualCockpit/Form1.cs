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
            if (label1.Text == currentState)
            {
                bump(Cockpit.components.AAP.CDUPWR);
            }
            label1.Text = Cockpit.components.AAP.CDUPWR.Position.ToString();
        }
        
        // TODO: there are problems with the following event handlers
        private void button2_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.EGIPWR);
            label2.Text = Cockpit.components.AAP.EGIPWR.Position.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.PAGE);
            label3.Text = Cockpit.components.AAP.PAGE.Position.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.STEER);
            label4.Text = Cockpit.components.AAP.STEER.Position.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bump(Cockpit.components.AAP.STEERPT);
            label5.Text = Cockpit.components.AAP.STEERPT.Position.ToString();
        }

        delegate void SetTextCallback(string text);

        private void ValueChanged(object sender, EventArgs e)
        {
            var input = (VirtualCockpit.Controllable.MultiPositionSwitch)sender;
            string name = input.Name;
            int value = input.Position;
            Program.SendUDP(name + " " + value.ToString());

            if (name == "AAP_CDUPWR")
            {
                SetText(value.ToString());
            }
        }

        private void SetText(string text)
        {
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }
        }
    }
}
