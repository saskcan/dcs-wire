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
                    "AAP", new Panel("AAP", new NumericInterfaceable[] {
                        new NumericInterfaceable("CDUPWR", 0, 1),
                        new NumericInterfaceable("EGIPWR", 0, 1),
                        new NumericInterfaceable("PAGE", 0, 3),
                        new NumericInterfaceable("STEER", 0, 2),
                        new NumericInterfaceable("STEERPT", 0, 2)})
                },
                {
                    "ADI", new Panel("ADI", new NumericInterfaceable[] {
                        new NumericInterfaceable("ATTWARNFLAG", 0, 65535),
                        new NumericInterfaceable("BANK", 0, 65535),
                        new NumericInterfaceable("CRSWARNFLAG", 0, 65535),
                        new NumericInterfaceable("GS", 0, 65535),
                        new NumericInterfaceable("GSWARNFLAG", 0, 65535),
                        new NumericInterfaceable("PITCH", 0, 65535),
                        new NumericInterfaceable("PITCH_TRIM", 0, 65535),
                        new NumericInterfaceable("SLIP", 0, 65535),
                        new NumericInterfaceable("STEERBANK", 0, 65535),
                        new NumericInterfaceable("STEERPITCH", 0, 65535),
                        new NumericInterfaceable("TURN", 0, 65535)})
                },
                {
                    "AHCP", new Panel("AHCP", new NumericInterfaceable[] {
                        new NumericInterfaceable("ALT_SCE", 0, 2),
                        new NumericInterfaceable("CICU", 0, 1),
                        new NumericInterfaceable("GUNPAC", 0, 2),
                        new NumericInterfaceable("HUD_DAYNIGHT", 0, 1),
                        new NumericInterfaceable("HUD_MODE", 0, 1),
                        new NumericInterfaceable("IFFCC", 0, 2),
                        new NumericInterfaceable("JTRS", 0, 1),
                        new NumericInterfaceable("LASER_ARM", 0, 2),
                        new NumericInterfaceable("MASTER_ARM", 0, 2),
                        new NumericInterfaceable("TGP", 0, 1)})
                }
            };

            foreach (var key in panels.Keys)
            {
                panels[key].StateUpdated += HandleChildMessage;
            }
		}
	}

    public class Interfaceable
    {
        private string name;

        public string Name
        {
            get
            {
                return name;
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

        public Interfaceable(string n)
        {
            name = n;
        }
    }

    public class NumericInterfaceable : Interfaceable
    {
        private int _value;

        public int Value 
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    Message msg = new Message(null, Name, "INT", _value.ToString());
                    OnChanged(new MessageReadyEventArgs(msg));
                }

            }
             
        }

        private int _maxValue;

        public int MaxValue
        {
            get
            {
                return _maxValue;
            }
        }

        public NumericInterfaceable(string n, int v, int m) : base(n)
        {
            _value = v;
            _maxValue = m;
        }

        public void SetValue(string v)
        {
            int value;
            if(int.TryParse(v, out value))
            {
                Value = value;
            }
        }
    }

    public class TextInterfaceable : Interfaceable
    {
        private string _value;

        public string Value 
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    Message msg = new Message(null, Name, "STR", _value);
                    OnChanged(new MessageReadyEventArgs(msg));
                }

            }
             
        }

        public TextInterfaceable(string n, string v) : base(n)
        {
            _value = v;
        }
    }

    public class Panel
    {
        private string _name;

        public string Name
        { 
            get
            {
                return _name;
            }
         }

        public Dictionary<string, NumericInterfaceable> numericInterfaceables;
        public Dictionary<string, TextInterfaceable> textInterfaceables;

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

        public Panel(string n, NumericInterfaceable[] numeric = null, TextInterfaceable[] text = null)
        {
            numericInterfaceables = new Dictionary<string, NumericInterfaceable>();
            textInterfaceables = new Dictionary<string, TextInterfaceable>();
            _name = n;

            if (numeric != null)
            {
                foreach (var nu in numeric)
                {
                    numericInterfaceables.Add(nu.Name, nu);
                    nu.Changed += HandleChildMessage;
                }
            }

            if (text != null)
            {
                foreach (var t in text)
                {
                    textInterfaceables.Add(t.Name, t);
                    t.Changed += HandleChildMessage;
                }
            }
        }

        public virtual void HandleChildMessage(object sender, MessageReadyEventArgs e)
        {
            e.message.controlGroup = _name;
            OnStateUpdated(e);
        }
	}
}
