using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCSWireUtils;
using DCSWire;

namespace Cockpit
{
	public class Cockpit
	{
		public Dictionary<string, Panel> panels;

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

        public virtual void HandleChildMessage(object sender, MessageReadyEventArgs e)
        {
            OnStateUpdated(e);
        }

		public Cockpit()
        {
            panels = new Dictionary<string, Panel>()
            {
                {
                    "AAP", new Panel("AAP", new MultiPositionSwitch[] {
                        new MultiPositionSwitch("CDUPWR", 0, 2),
                        new MultiPositionSwitch("EGIPWR", 0, 2),
                        new MultiPositionSwitch("PAGE", 0, 4),
                        new MultiPositionSwitch("STEER", 0, 3),
                        new MultiPositionSwitch("STEERPT", 0, 3)})
                },
                {
                    "AHCP", new Panel("AHCP", new MultiPositionSwitch[] {
                        new MultiPositionSwitch("AHCP_ALT_SCE", 0, 3),
                        new MultiPositionSwitch("AHCP_CICU", 0, 2),
                        new MultiPositionSwitch("AHCP_GUNPAC", 0, 3),
                        new MultiPositionSwitch("AHCP_HUD_DAYNIGHT", 0, 2),
                        new MultiPositionSwitch("AHCP_H33UD_MODE", 0, 2),
                        new MultiPositionSwitch("AHCP_IFFCC", 0, 3),
                        new MultiPositionSwitch("AHCP_JTRS", 0, 2),
                        new MultiPositionSwitch("AHCP_LASER_ARM", 0, 3),
                        new MultiPositionSwitch("AHCP_MASTER_ARM", 0, 3),
                        new MultiPositionSwitch("AHCP_TGP", 0, 2)})
                }
            };

            foreach (var key in panels.Keys)
            {
                panels[key].StateUpdated += HandleChildMessage;
            }
		}
	}

    public class DiscreteInput
    {
        private string name;
        private int position;
        
        public string Name
        {
            get
            {
                return name;
            }
        }
        
        public int Position
        {
            get
            {
                return position;
            }
        }

        public void SetState (int pos)
        {
            if (position != pos)
            {
                position = pos;
                Message msg = new Message(null, Name, "INT", position.ToString());
                OnChanged(new MessageReadyEventArgs(msg));
            }
        }

        public void SetState(string pos)
        {
            int position;
            if(int.TryParse(pos, out position))
            {
                SetState(position);
            }

        }

        public event MessageReadyEventHandler Changed;

        protected virtual void OnChanged(MessageReadyEventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }

        public DiscreteInput(string n, int pos = 0)
        {
            name = n;
            position = pos;
        }
    }

    public class MultiPositionSwitch : DiscreteInput
    {
        private int maxPosition;

        public int MaxPosition
        {
            get
            {
                return maxPosition;
            }
        }

        public MultiPositionSwitch(string n, int pos, int positions)
            : base(n, pos)
        {
            maxPosition = positions - 1;
        }
    }

    public class Panel
    {
        public string name;
        public Dictionary<string, MultiPositionSwitch> multiPositionSwitches;

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

        public Panel(string n)
        {
            multiPositionSwitches = new Dictionary<string, MultiPositionSwitch>();
            name = n;
        }

        public Panel(string n, MultiPositionSwitch[] switches)
        {
            multiPositionSwitches = new Dictionary<string, MultiPositionSwitch>();
            name = n;
            foreach(var s in switches)
            {
                multiPositionSwitches.Add(s.Name, s);
                s.Changed += HandleChildMessage;
            }
        }

        public virtual void HandleChildMessage(object sender, MessageReadyEventArgs e)
        {
            e.message.controlGroup = name;
            OnStateUpdated(e);
        }
	}
}
