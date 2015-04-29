using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualCockpit
{
    static public class Components
    {
        static public Panels.AAP AAP = new Panels.AAP();

		static public Cockpit initialize()
		{
			var cockpit = new Cockpit();
			#region AAP
			var AAP = new Panels.Panel("AAP");
			AAP.multiPositionSwitches.Add("AAP_CDUPWR", new Controllable.MultiPositionSwitch("AAP_CDUPWR", 0, 2));
			AAP.multiPositionSwitches.Add("AAP_EGIPWR", new Controllable.MultiPositionSwitch("AAP_EGIPWR", 0, 2));
			AAP.multiPositionSwitches.Add("AAP_PAGE", new Controllable.MultiPositionSwitch("AAP_PAGE", 0, 4));
			AAP.multiPositionSwitches.Add("AAP_STEER", new Controllable.MultiPositionSwitch("AAP_STEER", 0, 3));
			AAP.multiPositionSwitches.Add("AAP_STEERPT", new Controllable.MultiPositionSwitch("AAP_STEERPT", 0, 3));
			cockpit.panels.Add("AAP", AAP);
			#endregion

			#region ADI
			var ADI = new Panels.Panel("ADI");
			// TODO: Add inputs
			#endregion

			#region AHCP
			var AHCP = new Panels.Panel("AHCP");
			AHCP.multiPositionSwitches.Add("AHCP_ALT_SCE", new Controllable.MultiPositionSwitch("AHCP_ALT_SCE", 0, 3));
			AHCP.multiPositionSwitches.Add("AHCP_CICU", new Controllable.MultiPositionSwitch("AHCP_CICU", 0, 2));
			AHCP.multiPositionSwitches.Add("AHCP_GUNPAC", new Controllable.MultiPositionSwitch("AHCP_GUNPAC", 0, 3));
			AHCP.multiPositionSwitches.Add("AHCP_HUD_DAYNIGHT", new Controllable.MultiPositionSwitch("AHCP_HUD_DAYNIGHT", 0, 2));
			AHCP.multiPositionSwitches.Add("AHCP_HUD_MODE", new Controllable.MultiPositionSwitch("AHCP_H33UD_MODE", 0, 2));
			AHCP.multiPositionSwitches.Add("AHCP_IFFCC", new Controllable.MultiPositionSwitch("AHCP_IFFCC", 0, 3));
			AHCP.multiPositionSwitches.Add("AHCP_JTRS", new Controllable.MultiPositionSwitch("AHCP_JTRS", 0, 2));
			AHCP.multiPositionSwitches.Add("AHCP_LASER_ARM", new Controllable.MultiPositionSwitch("AHCP_LASER_ARM", 0, 3));
			AHCP.multiPositionSwitches.Add("AHCP_MASTER_ARM", new Controllable.MultiPositionSwitch("AHCP_MASTER_ARM", 0, 3));
			AHCP.multiPositionSwitches.Add("AHCP_TGP", new Controllable.MultiPositionSwitch("AHCP_TGP", 0, 2));
			#endregion

			return cockpit;
		}

    }

}
