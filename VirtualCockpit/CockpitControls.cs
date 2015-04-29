using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VirtualCockpit
{

	public class Cockpit
	{
		public Dictionary<string, Panels.Panel> panels;
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
                    OnChanged(EventArgs.Empty);
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

            public event EventHandler Changed;

            protected virtual void OnChanged(EventArgs e)
            {
                EventHandler handler = Changed;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public DiscreteInput()
            {
                name = "UnnamedInput";
                position = 0;
            }

            public DiscreteInput(string n, int p = 0)
            {
                name = n;
                position = p;
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

            public MultiPositionSwitch(int positions)
                : base()
            {
                maxPosition = positions - 1;
            }

            public MultiPositionSwitch(string n, int p, int positions)
                : base(n, p)
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
			public Panel(string n)
			{
				name = n;
			}
		}


		#region AAP
		public class AAP
        {
            // CDU Power
            public Controllable.MultiPositionSwitch CDUPWR = new Controllable.MultiPositionSwitch("AAP_CDUPWR", 0, 2);

            // EGI Power
            public Controllable.MultiPositionSwitch EGIPWR = new Controllable.MultiPositionSwitch("AAP_EGIPWR", 0, 2);

            // PAGE OTHER - POSITION - STEER - WAYPT
            public Controllable.MultiPositionSwitch PAGE = new Controllable.MultiPositionSwitch("AAP_PAGE", 0, 4);

            // Toggle Steerpoint
            public Controllable.MultiPositionSwitch STEER = new Controllable.MultiPositionSwitch("AAP_STEER", 0, 3);

            // STEERPT FLTPLAN - MARK - MISSION
            public Controllable.MultiPositionSwitch STEERPT = new Controllable.MultiPositionSwitch("AAP_STEERPT", 0, 3);
		}
		#endregion

	#endregion


	}
}
