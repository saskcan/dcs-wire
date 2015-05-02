using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCSWireUtils;
using DCSWire;

namespace VirtualCockpit
{
	public class Cockpit
	{
		public Dictionary<string, Panels.Panel> panels;

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
            panels = new Dictionary<string, Panels.Panel>()
            {
                {
                    "AAP", new Panels.Panel("AAP", new Controllable.MultiPositionSwitch[] {
                        new Controllable.MultiPositionSwitch("CDUPWR", 0, 2),
                        new Controllable.MultiPositionSwitch("EGIPWR", 0, 2),
                        new Controllable.MultiPositionSwitch("PAGE", 0, 4),
                        new Controllable.MultiPositionSwitch("STEER", 0, 3),
                        new Controllable.MultiPositionSwitch("STEERPT", 0, 3)})
                },
                {
                    "AHCP", new Panels.Panel("AHCP", new Controllable.MultiPositionSwitch[] {
                        new Controllable.MultiPositionSwitch("AHCP_ALT_SCE", 0, 3),
                        new Controllable.MultiPositionSwitch("AHCP_CICU", 0, 2),
                        new Controllable.MultiPositionSwitch("AHCP_GUNPAC", 0, 3),
                        new Controllable.MultiPositionSwitch("AHCP_HUD_DAYNIGHT", 0, 2),
                        new Controllable.MultiPositionSwitch("AHCP_H33UD_MODE", 0, 2),
                        new Controllable.MultiPositionSwitch("AHCP_IFFCC", 0, 3),
                        new Controllable.MultiPositionSwitch("AHCP_JTRS", 0, 2),
                        new Controllable.MultiPositionSwitch("AHCP_LASER_ARM", 0, 3),
                        new Controllable.MultiPositionSwitch("AHCP_MASTER_ARM", 0, 3),
                        new Controllable.MultiPositionSwitch("AHCP_TGP", 0, 2)})
                }
            };

            foreach (var key in panels.Keys)
            {
                panels[key].StateUpdated += HandleChildMessage;
            }
		}
	}

    namespace Controllable
    {
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
    }


	#region Panels
	namespace Panels
	{
		public class Panel
		{
			public string name;
			public Dictionary<string, Controllable.MultiPositionSwitch> multiPositionSwitches;

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
                multiPositionSwitches = new Dictionary<string, Controllable.MultiPositionSwitch>();
				name = n;
			}

            public Panel(string n, Controllable.MultiPositionSwitch[] switches)
            {
                multiPositionSwitches = new Dictionary<string, Controllable.MultiPositionSwitch>();
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
	#endregion
	}
}
