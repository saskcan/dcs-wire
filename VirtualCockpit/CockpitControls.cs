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
                },
                {
                    "AOA", new Panel("AOA", new NumericInterfaceable[] {
                            new NumericInterfaceable("PWROFF", 0, 65535),
                            new NumericInterfaceable("UNITS", 0, 65535)})
                },
                {
                    "ACCEL", new Panel("ACCEL", new NumericInterfaceable[] {
                            new NumericInterfaceable("G", 0, 65535),
                            new NumericInterfaceable("MAX", 0, 65535),
                            new NumericInterfaceable("MIN", 0, 65535),
                            new NumericInterfaceable("PTS", 0, 1)})
                },
                {
                    "AIRSPEED", new Panel("AIRSPEED", new NumericInterfaceable[] {
                            new NumericInterfaceable("DIAL", 0, 65535),
                            new NumericInterfaceable("NEEDLE", 0, 65535)})
                },
                {
                    "ALT", new Panel("ALT", new NumericInterfaceable[] {
                            new NumericInterfaceable("10000FTCNT", 0, 65535),
                            new NumericInterfaceable("1000FTCNT", 0, 65535),
                            new NumericInterfaceable("100FT", 0, 65535),
                            new NumericInterfaceable("100FTCNT", 0, 65535),
                            new NumericInterfaceable("ELECT_PNEU", 0, 2),
                            new NumericInterfaceable("PRESSURE0", 0, 65535),
                            new NumericInterfaceable("PRESSURE1", 0, 65535),
                            new NumericInterfaceable("PRESSURE2", 0, 65535),
                            new NumericInterfaceable("PRESSURE3", 0, 65535),
                            new NumericInterfaceable("SET_PRESSURE", 0, 65535)})
                },
                {
                    "ANT", new Panel("ANT", new NumericInterfaceable[] {
                            new NumericInterfaceable("EGIHQTOD", 0, 1),
                            new NumericInterfaceable("IFF", 0, 2),
                            new NumericInterfaceable("UHF", 0, 2)})
                },
                {
                    "ALCP", new Panel("ALCP", new NumericInterfaceable[] {
                            new NumericInterfaceable("FDBA_TEST", 0, 1),
                            new NumericInterfaceable("HARSSAS", 0, 1),
                            new NumericInterfaceable("NVIS_LTS", 0, 2),
                            new NumericInterfaceable("RCVR_LTS", 0, 65535),
                            new NumericInterfaceable("RSIL", 0, 65535),
                            new NumericInterfaceable("WPNSTA", 0, 65535),
                            new NumericInterfaceable("LAMP_TEST_BTN", 0, 1, false)})
                },
                {
                    "CDU", new Panel("CDU", new NumericInterfaceable[] {
                            new NumericInterfaceable("0", 0, 1),
                            new NumericInterfaceable("1", 0, 1),
                            new NumericInterfaceable("2", 0, 1),
                            new NumericInterfaceable("3", 0, 1),
                            new NumericInterfaceable("4", 0, 1),
                            new NumericInterfaceable("5", 0, 1),
                            new NumericInterfaceable("6", 0, 1),
                            new NumericInterfaceable("7", 0, 1),
                            new NumericInterfaceable("8", 0, 1),
                            new NumericInterfaceable("9", 0, 1),
                            new NumericInterfaceable("A", 0, 1),
                            new NumericInterfaceable("B", 0, 1),
                            new NumericInterfaceable("BCK", 0, 1),
                            new NumericInterfaceable("BRT", 0, 2),
                            new NumericInterfaceable("C", 0, 1),
                            new NumericInterfaceable("CLR", 0, 1),
                            new NumericInterfaceable("D", 0, 1),
                            new NumericInterfaceable("DATA", 0, 2),
                            new NumericInterfaceable("E", 0, 1),
                            new NumericInterfaceable("F", 0, 1),
                            new NumericInterfaceable("FA", 0, 1),
                            new NumericInterfaceable("FPM", 0, 1),
                            new NumericInterfaceable("G", 0, 1),
                            new NumericInterfaceable("H", 0, 1),
                            new NumericInterfaceable("I", 0, 1),
                            new NumericInterfaceable("J", 0, 1),
                            new NumericInterfaceable("K", 0, 1),
                            new NumericInterfaceable("L", 0, 1),
                            new NumericInterfaceable("LSK_3L", 0, 1),
                            new NumericInterfaceable("LSK_3R", 0, 1),
                            new NumericInterfaceable("LSK_5L", 0, 1),
                            new NumericInterfaceable("LSK_5R", 0, 1),
                            new NumericInterfaceable("LSK_7L", 0, 1),
                            new NumericInterfaceable("LSK_7R", 0, 1),
                            new NumericInterfaceable("LSK_9L", 0, 1),
                            new NumericInterfaceable("LSK_9R", 0, 1),
                            new NumericInterfaceable("M", 0, 1),
                            new NumericInterfaceable("MK", 0, 1),
                            new NumericInterfaceable("N", 0, 1),
                            new NumericInterfaceable("NA1", 0, 1),
                            new NumericInterfaceable("NA2", 0, 1),
                            new NumericInterfaceable("NAV", 0, 1),
                            new NumericInterfaceable("O", 0, 1),
                            new NumericInterfaceable("OSET", 0, 1),
                            new NumericInterfaceable("P", 0, 1),
                            new NumericInterfaceable("PG", 0, 2),
                            new NumericInterfaceable("POINT", 0, 1),
                            new NumericInterfaceable("PREV", 0, 1),
                            new NumericInterfaceable("Q", 0, 1),
                            new NumericInterfaceable("R", 0, 1),
                            new NumericInterfaceable("S", 0, 1),
                            new NumericInterfaceable("SCROLL", 0, 2),
                            new NumericInterfaceable("SLASH", 0, 1),
                            new NumericInterfaceable("SPC", 0, 1),
                            new NumericInterfaceable("SYS", 0, 1),
                            new NumericInterfaceable("T", 0, 1),
                            new NumericInterfaceable("U", 0, 1),
                            new NumericInterfaceable("V", 0, 1),
                            new NumericInterfaceable("W", 0, 1),
                            new NumericInterfaceable("WP", 0, 1),
                            new NumericInterfaceable("X", 0, 1),
                            new NumericInterfaceable("Y", 0, 1),
                            new NumericInterfaceable("Z", 0, 1)},
                        new TextInterfaceable[] {
                            new TextInterfaceable("LINE0BUFFER", ""),
                            new TextInterfaceable("LINE1BUFFER", ""),
                            new TextInterfaceable("LINE2BUFFER", ""),
                            new TextInterfaceable("LINE3BUFFER", ""),
                            new TextInterfaceable("LINE4BUFFER", ""),
                            new TextInterfaceable("LINE5BUFFER", ""),
                            new TextInterfaceable("LINE6BUFFER", ""),
                            new TextInterfaceable("LINE7BUFFER", ""),
                            new TextInterfaceable("LINE8BUFFER", ""),
                            new TextInterfaceable("LINE9BUFFER", "")})
                },
                {
                    "CMSC", new Panel("CMSC", new NumericInterfaceable[] {
                            new NumericInterfaceable("BRT", 0, 65535),
                            new NumericInterfaceable("JMR", 0, 1),
                            new NumericInterfaceable("LAUNCH", 0, 1),
                            new NumericInterfaceable("MWS", 0, 1),
                            new NumericInterfaceable("PRI", 0, 1),
                            new NumericInterfaceable("PRIO", 0, 1),
                            new NumericInterfaceable("RWR_VOL", 0, 65535),
                            new NumericInterfaceable("SEP", 0, 1),
                            new NumericInterfaceable("UNKNVALUE", 0, 1)},
                        new TextInterfaceable[] {
                            new TextInterfaceable("TXTCHAFFFLAREBUFFER", ""),
                            new TextInterfaceable("TXTJMRBUFFER", ""),
                            new TextInterfaceable("TXTMWSBUFFER", "")})
                },
                {
                    "CMSP", new Panel("CMSP", new NumericInterfaceable[] {
                            new NumericInterfaceable("ARW1", 0, 1),
                            new NumericInterfaceable("ARW2", 0, 1),
                            new NumericInterfaceable("ARW3", 0, 1),
                            new NumericInterfaceable("ARW4", 0, 1),
                            new NumericInterfaceable("BRT", 0, 65535),
                            new NumericInterfaceable("DISP", 0, 2),
                            new NumericInterfaceable("JMR", 0, 2),
                            new NumericInterfaceable("JTSN", 0, 1),
                            new NumericInterfaceable("MODE", 0, 4),
                            new NumericInterfaceable("MWS", 0, 2),
                            new NumericInterfaceable("RTN", 0, 1),
                            new NumericInterfaceable("RWR", 0, 2),
                            new NumericInterfaceable("UPDN", 0, 2)},
                        new TextInterfaceable[] {
                            new TextInterfaceable("1BUFFER", ""),
                            new TextInterfaceable("2BUFFER", "")})
                },
                {
                    "CL", new Panel("CL", new NumericInterfaceable[] {
                            new NumericInterfaceable("A1", 0, 1),
                            new NumericInterfaceable("A2", 0, 1),
                            new NumericInterfaceable("A3", 0, 1),
                            new NumericInterfaceable("A4", 0, 1),
                            new NumericInterfaceable("B1", 0, 1),
                            new NumericInterfaceable("B2", 0, 1),
                            new NumericInterfaceable("B3", 0, 1),
                            new NumericInterfaceable("B4", 0, 1),
                            new NumericInterfaceable("C1", 0, 1),
                            new NumericInterfaceable("C2", 0, 1),
                            new NumericInterfaceable("C3", 0, 1),
                            new NumericInterfaceable("C4", 0, 1),
                            new NumericInterfaceable("D1", 0, 1),
                            new NumericInterfaceable("D2", 0, 1),
                            new NumericInterfaceable("D3", 0, 1),
                            new NumericInterfaceable("D4", 0, 1),
                            new NumericInterfaceable("E1", 0, 1),
                            new NumericInterfaceable("E2", 0, 1),
                            new NumericInterfaceable("E3", 0, 1),
                            new NumericInterfaceable("E4", 0, 1),
                            new NumericInterfaceable("F1", 0, 1),
                            new NumericInterfaceable("F2", 0, 1),
                            new NumericInterfaceable("F3", 0, 1),
                            new NumericInterfaceable("F4", 0, 1),
                            new NumericInterfaceable("G1", 0, 1),
                            new NumericInterfaceable("G2", 0, 1),
                            new NumericInterfaceable("G3", 0, 1),
                            new NumericInterfaceable("G4", 0, 1),
                            new NumericInterfaceable("H1", 0, 1),
                            new NumericInterfaceable("H2", 0, 1),
                            new NumericInterfaceable("H3", 0, 1),
                            new NumericInterfaceable("H4", 0, 1),
                            new NumericInterfaceable("I1", 0, 1),
                            new NumericInterfaceable("I2", 0, 1),
                            new NumericInterfaceable("I3", 0, 1),
                            new NumericInterfaceable("I4", 0, 1),
                            new NumericInterfaceable("J1", 0, 1),
                            new NumericInterfaceable("J2", 0, 1),
                            new NumericInterfaceable("J3", 0, 1),
                            new NumericInterfaceable("J4", 0, 1),
                            new NumericInterfaceable("K1", 0, 1),
                            new NumericInterfaceable("K2", 0, 1),
                            new NumericInterfaceable("K3", 0, 1),
                            new NumericInterfaceable("K4", 0, 1),
                            new NumericInterfaceable("L1", 0, 1),
                            new NumericInterfaceable("L2", 0, 1),
                            new NumericInterfaceable("L3", 0, 1),
                            new NumericInterfaceable("L4", 0, 1)})
                },
                {
                    "CBP", new Panel("CBP", new NumericInterfaceable[] {
                            new NumericInterfaceable("AILERON_DISC_L", 0, 1),
                            new NumericInterfaceable("AILERON_DISC_R", 0, 1),
                            new NumericInterfaceable("AILERON_TAB_L", 0, 1),
                            new NumericInterfaceable("AILERON_TAB_R", 0, 1),
                            new NumericInterfaceable("APU_CONT", 0, 1),
                            new NumericInterfaceable("AUX_ESS_BUS_0A", 0, 1),
                            new NumericInterfaceable("AUX_ESS_BUS_0B", 0, 1),
                            new NumericInterfaceable("AUX_ESS_BUS_0C", 0, 1),
                            new NumericInterfaceable("AUX_ESS_BUS_TIE", 0, 1),
                            new NumericInterfaceable("BAT_BUS_TRANS", 0, 1),
                            new NumericInterfaceable("BLEED_AIR_CONT_L", 0, 1),
                            new NumericInterfaceable("BLEED_AIR_CONT_R", 0, 1),
                            new NumericInterfaceable("CONVERTER_L", 0, 1),
                            new NumericInterfaceable("DC_FUEL_PUMP", 0, 1),
                            new NumericInterfaceable("ELEVATION_DISC_L", 0, 1),
                            new NumericInterfaceable("ELEVATION_DISC_R", 0, 1),
                            new NumericInterfaceable("EMER_FLAP", 0, 1),
                            new NumericInterfaceable("EMER_TRIM", 0, 1),
                            new NumericInterfaceable("ENG_IGNITOR_1", 0, 1),
                            new NumericInterfaceable("ENG_IGNITOR_2", 0, 1),
                            new NumericInterfaceable("ENG_START_L", 0, 1),
                            new NumericInterfaceable("ENG_START_R", 0, 1),
                            new NumericInterfaceable("EXT_STORES_JETT_1", 0, 1),
                            new NumericInterfaceable("EXT_STORES_JETT_2", 0, 1),
                            new NumericInterfaceable("FUEL_SHUTOFF_L", 0, 1),
                            new NumericInterfaceable("FUEL_SHUTOFF_R", 0, 1),
                            new NumericInterfaceable("GEAR", 0, 1),
                            new NumericInterfaceable("GEN_CONT_L", 0, 1),
                            new NumericInterfaceable("GEN_CONT_R", 0, 1),
                            new NumericInterfaceable("IFF", 0, 1),
                            new NumericInterfaceable("INTERCOM", 0, 1),
                            new NumericInterfaceable("INVERTER_CONT", 0, 1),
                            new NumericInterfaceable("INVERTER_PWR", 0, 1),
                            new NumericInterfaceable("MASTER_CAUTION", 0, 1),
                            new NumericInterfaceable("PITOT_HEAT_AC", 0, 1),
                            new NumericInterfaceable("SPS_RUDDER_AUTH_LIMIT", 0, 1),
                            new NumericInterfaceable("STBY_ATT_IND", 0, 1),
                            new NumericInterfaceable("UHF", 0, 1)})
                },
                {
                    "DVADR", new Panel("DVADR", new NumericInterfaceable[] {
                            new NumericInterfaceable("EOT", 0, 1),
                            new NumericInterfaceable("FUNCTION", 0, 2),
                            new NumericInterfaceable("REC", 0, 1),
                            new NumericInterfaceable("VIDEO", 0, 2)})
                },
                {
                    "CLOCK", new Panel("CLOCK", new NumericInterfaceable[] {
                            new NumericInterfaceable("CTRL", 0, 1),
                            new NumericInterfaceable("SET", 0, 1)},
                        new TextInterfaceable[] {
                            new TextInterfaceable("ETCBUFFER", ""),
                            new TextInterfaceable("HHBUFFER", ""),
                            new TextInterfaceable("MMBUFFER", ""),
                            new TextInterfaceable("SSBUFFER", "")})
                },
                {
                    "EPP", new Panel("EPP", new NumericInterfaceable[] {
                            new NumericInterfaceable("AC_GEN_PWR_L", 0, 1),
                            new NumericInterfaceable("AC_GEN_PWR_R", 0, 1),
                            new NumericInterfaceable("APU_GEN_PWR", 0, 1),
                            new NumericInterfaceable("BATTERY_PWR", 0, 1),
                            new NumericInterfaceable("EMER_FLOOD", 0, 1),
                            new NumericInterfaceable("INVERTER", 0, 2)})
                },
                {
                    "EFCP", new Panel("EFCP", new NumericInterfaceable[] {
                            new NumericInterfaceable("AILERON_EMER_DISENGAGE", 0, 2),
                            new NumericInterfaceable("ELEVATOR_EMER_DISENGAGE", 0, 2),
                            new NumericInterfaceable("EMER_TRIM", 0, 4),
                            new NumericInterfaceable("FLAPS_EMER_RETR", 0, 1),
                            new NumericInterfaceable("MRFCS", 0, 1),
                            new NumericInterfaceable("SPDBK_EMER_RETR", 0, 1),
                            new NumericInterfaceable("TRIM_OVERRIDE", 0, 1),
                            new NumericInterfaceable("LAILERONEMERDISENGAGE", 0, 1),
                            new NumericInterfaceable("LELEVATOREMERDISENGAGE", 0, 1),
                            new NumericInterfaceable("RAILERONEMERDISENGAGE", 0, 1),
                            new NumericInterfaceable("RELEVATOREMERDISENGAGE", 0, 1)})
                },
                {
                    "ENGINE", new Panel("ENGINE", new NumericInterfaceable[] {
                            new NumericInterfaceable("APURPM", 0, 65535),
                            new NumericInterfaceable("APUTEMP", 0, 65535),
                            new NumericInterfaceable("LENGCORE", 0, 65535),
                            new NumericInterfaceable("LENGCORET", 0, 65535),
                            new NumericInterfaceable("LENGFAN", 0, 65535),
                            new NumericInterfaceable("LENGFUELFLOW", 0, 65535),
                            new NumericInterfaceable("LENGTEMP", 0, 65535),
                            new NumericInterfaceable("LENGTEMPOFF", 0, 65535),
                            new NumericInterfaceable("LENGTEMPT", 0, 65535),
                            new NumericInterfaceable("LHYDPRESS", 0, 65535),
                            new NumericInterfaceable("LOILPRESS", 0, 65535),
                            new NumericInterfaceable("RENGCORE", 0, 65535),
                            new NumericInterfaceable("RENGCORET", 0, 65535),
                            new NumericInterfaceable("RENGFAN", 0, 65535),
                            new NumericInterfaceable("RENGFUELFLOW", 0, 65535),
                            new NumericInterfaceable("RENGTEMP", 0, 65535),
                            new NumericInterfaceable("RENGTEMPOFF", 0, 65535),
                            new NumericInterfaceable("RENGTEMPT", 0, 65535),
                            new NumericInterfaceable("RHYDPRESS", 0, 65535),
                            new NumericInterfaceable("ROILPRESS", 0, 65535)})
                },
                {
                    "ENVCP", new Panel("ENVCP", new NumericInterfaceable[] {
                            new NumericInterfaceable("CABINPRESSALT", 0, 65535),
                            new NumericInterfaceable("AC_OPER", 0, 3),
                            new NumericInterfaceable("AIR_SUPPLY", 0, 1),
                            new NumericInterfaceable("BLEED_AIR", 0, 1),
                            new NumericInterfaceable("CANOPY_DEFOG", 0, 65535),
                            new NumericInterfaceable("FLOW_LEVEL", 0, 65535),
                            new NumericInterfaceable("OXY_TEST", 0, 1),
                            new NumericInterfaceable("PITOT_HEAT", 0, 1),
                            new NumericInterfaceable("TEMP_LEVEL", 0, 65535),
                            new NumericInterfaceable("TEMP_PRESS", 0, 2),
                            new NumericInterfaceable("WINDSHIELD_DEFOG", 0, 1),
                            new NumericInterfaceable("WRRW", 0, 2),
                            new NumericInterfaceable("OXYVOLUME", 0, 65535)})
                },
                {
                    "DASH", new Panel("DASH", new NumericInterfaceable[] {
                            new NumericInterfaceable("CANOPYUNLOCKED", 0, 1),
                            new NumericInterfaceable("GUNREADY", 0, 1),
                            new NumericInterfaceable("MARKERBEACON", 0, 1),
                            new NumericInterfaceable("NOSEWHEELSTEERING", 0, 1)})
                },
                {
                    "FQIS", new Panel("FQIS", new NumericInterfaceable[] {
                            new NumericInterfaceable("SELECT", 0, 4),
                            new NumericInterfaceable("TEST", 0, 1),
                            new NumericInterfaceable("FUELQTY100", 0, 65535),
                            new NumericInterfaceable("FUELQTY1000", 0, 65535),
                            new NumericInterfaceable("FUELQTY10000", 0, 65535),
                            new NumericInterfaceable("FUELQTYL", 0, 65535),
                            new NumericInterfaceable("FUELQTYR", 0, 65535)})
                },
                {
                    "FSCP", new Panel("FSCP", new NumericInterfaceable[] {
                            new NumericInterfaceable("AMPL", 0, 1),
                            new NumericInterfaceable("BOOST_MAIN_L", 0, 1),
                            new NumericInterfaceable("BOOST_MAIN_R", 0, 1),
                            new NumericInterfaceable("BOOST_WING_L", 0, 1),
                            new NumericInterfaceable("BOOST_WING_R", 0, 1),
                            new NumericInterfaceable("CROSSFEED", 0, 1),
                            new NumericInterfaceable("EXT_TANKS_FUS", 0, 1),
                            new NumericInterfaceable("EXT_TANKS_WING", 0, 1),
                            new NumericInterfaceable("FD_MAIN_L", 0, 1),
                            new NumericInterfaceable("FD_MAIN_R", 0, 1),
                            new NumericInterfaceable("FD_WING_L", 0, 1),
                            new NumericInterfaceable("FD_WING_R", 0, 1),
                            new NumericInterfaceable("LINE_CHECK", 0, 1),
                            new NumericInterfaceable("RCVR_LEVER", 0, 1),
                            new NumericInterfaceable("TK_GATE", 0, 1)})
                },
                {
                    "GLARE", new Panel("GLARE", new NumericInterfaceable[] {
                            new NumericInterfaceable("APUFIRE", 0, 1),
                            new NumericInterfaceable("EXT_STORES_JETTISON", 0, 1, false),
                            new NumericInterfaceable("FIRE_APU_PULL", 0, 1, false),
                            new NumericInterfaceable("FIRE_EXT_DISCH", 0, 2, false),
                            new NumericInterfaceable("FIRE_LENG_PULL", 0, 1, false),
                            new NumericInterfaceable("FIRE_RENG_PULL", 0, 1, false),
                            new NumericInterfaceable("LENGFIRE", 0, 1),
                            new NumericInterfaceable("RENGFIRE", 0, 1)})
                },
                {
                    "HARS", new Panel("HARS", new NumericInterfaceable[] {
                            new NumericInterfaceable("FAST_ERECT", 0, 1),
                            new NumericInterfaceable("HDG", 0, 65535),
                            new NumericInterfaceable("LATITUDE", 0, 65535),
                            new NumericInterfaceable("MAGVAR", 0, 2),
                            new NumericInterfaceable("NS", 0, 1),
                            new NumericInterfaceable("PTS", 0, 1),
                            new NumericInterfaceable("SLAVE_DG", 0, 1),
                            new NumericInterfaceable("SYNC", 0, 65535)})
                },
                {
                    "HSI", new Panel("HSI", new NumericInterfaceable[] {
                            new NumericInterfaceable("BEARING1", 0, 65535),
                            new NumericInterfaceable("BEARING2", 0, 65535),
                            new NumericInterfaceable("BEARINGFLAG", 0, 65535),
                            new NumericInterfaceable("CCA", 0, 65535),
                            new NumericInterfaceable("CCB", 0, 65535),
                            new NumericInterfaceable("CRS", 0, 65535),
                            new NumericInterfaceable("CRS_KNOB", 0, 65535),
                            new NumericInterfaceable("DEVIATION", 0, 65535),
                            new NumericInterfaceable("HDG", 0, 65535),
                            new NumericInterfaceable("HDGBUG", 0, 65535),
                            new NumericInterfaceable("HDG_KNOB", 0, 65535),
                            new NumericInterfaceable("PWROFFFLAG", 0, 65535),
                            new NumericInterfaceable("RANGEFLAG", 0, 65535),
                            new NumericInterfaceable("RCA", 0, 65535),
                            new NumericInterfaceable("RCB", 0, 65535),
                            new NumericInterfaceable("RCC", 0, 65535),
                            new NumericInterfaceable("RCD", 0, 65535),
                            new NumericInterfaceable("TOFROM1", 0, 65535),
                            new NumericInterfaceable("TOFROM2", 0, 65535)})
                },
                {
                    "HUD", new Panel("HUD", new NumericInterfaceable[] {
                            new NumericInterfaceable("AIRREFUELDISCONNECT", 0, 1),
                            new NumericInterfaceable("AIRREFUELLATCHED", 0, 1),
                            new NumericInterfaceable("AIRREFUELREADY", 0, 1),
                            new NumericInterfaceable("AOAINDEXERHIGH", 0, 1),
                            new NumericInterfaceable("AOAINDEXERLOW", 0, 1),
                            new NumericInterfaceable("AOAINDEXERNORMAL", 0, 1)})
                },
                {
                    "IFF", new Panel("IFF", new NumericInterfaceable[] {
                            new NumericInterfaceable("CODE", 0, 3),
                            new NumericInterfaceable("MASTER", 0, 4),
                            new NumericInterfaceable("MIC_IDENT", 0, 2),
                            new NumericInterfaceable("MODE1_WHEEL1", 0, 7),
                            new NumericInterfaceable("MODE1_WHEEL2", 0, 3),
                            new NumericInterfaceable("MODE3A_WHEEL1", 0, 7),
                            new NumericInterfaceable("MODE3A_WHEEL2", 0, 7),
                            new NumericInterfaceable("MODE3A_WHEEL3", 0, 7),
                            new NumericInterfaceable("MODE3A_WHEEL4", 0, 7),
                            new NumericInterfaceable("ON_OUT", 0, 1),
                            new NumericInterfaceable("OUT_AUDIO_LIGHT", 0, 2),
                            new NumericInterfaceable("RADTEST", 0, 2),
                            new NumericInterfaceable("REPLY_DIM", 0, 65535),
                            new NumericInterfaceable("REPLY_TEST", 0, 1),
                            new NumericInterfaceable("TEST_DIM", 0, 65535),
                            new NumericInterfaceable("TEST_M1", 0, 2),
                            new NumericInterfaceable("TEST_M2", 0, 2),
                            new NumericInterfaceable("TEST_M3", 0, 2),
                            new NumericInterfaceable("TEST_M4", 0, 2),
                            new NumericInterfaceable("TEST_TEST", 0, 1)})
                },
                // ILS Panel
                // NOTE: DCS-BIOS also provides a string output
                {
                    "ILS", new Panel("ILS", new NumericInterfaceable[] {
                            new NumericInterfaceable("KHZ", 0, 9),
                            new NumericInterfaceable("MHZ", 0, 3),
                            new NumericInterfaceable("PWR", 0, 1),
                            new NumericInterfaceable("VOL", 0, 65535)})
                },
                {
                    "INT", new Panel("INT", new NumericInterfaceable[] {
                            new NumericInterfaceable("AIM_UNMUTE", 0, 1),
                            new NumericInterfaceable("AIM_VOLUME", 0, 65535),
                            new NumericInterfaceable("CALL", 0, 1),
                            new NumericInterfaceable("FM_UNMUTE", 0, 1),
                            new NumericInterfaceable("FM_VOL", 0, 65535),
                            new NumericInterfaceable("HM", 0, 1),
                            new NumericInterfaceable("IFF_UNMUTE", 0, 1),
                            new NumericInterfaceable("IFF_VOL", 0, 65535),
                            new NumericInterfaceable("ILS_UNMUTE", 0, 1),
                            new NumericInterfaceable("ILS_VOL", 0, 65535),
                            new NumericInterfaceable("INT_UNMUTE", 0, 1),
                            new NumericInterfaceable("INT_VOL", 0, 65535),
                            new NumericInterfaceable("MODE", 0, 4),
                            new NumericInterfaceable("TCN_UNMUTE", 0, 1),
                            new NumericInterfaceable("TCN_VOL", 0, 65535),
                            new NumericInterfaceable("UHF_UNMUTE", 0, 1),
                            new NumericInterfaceable("UHF_VOL", 0, 65535),
                            new NumericInterfaceable("VHF_UNMUTE", 0, 1),
                            new NumericInterfaceable("VHF_VOL", 0, 65535),
                            new NumericInterfaceable("VOL", 0, 65535)})
                },
                {
                    "LASTE", new Panel("LASTE", new NumericInterfaceable[] {
                            new NumericInterfaceable("AP_MODE", 0, 2),
                            new NumericInterfaceable("AP_TOGGLE", 0, 1),
                            new NumericInterfaceable("EAC", 0, 1, mm: new Dictionary<int,string>() {
                                {0, "OFF"},        
                                {1, "PUSH"}}),
                            new NumericInterfaceable("RDR_ALTM", 0, 1)})
                },
                {
                    "LANDING", new Panel("LANDING", new NumericInterfaceable[] {
                            new NumericInterfaceable("ANTI_SKID_SWITCH", 0, 1, false, new Dictionary<int,string>() {
                                {0, "OFF"},
                                {1, "PUSH"}}),
                            new NumericInterfaceable("DOWNLOCK_OVERRIDE", 0, 1, false),
                            new NumericInterfaceable("ENGINE_TEMS_DATA", 0, 1, false),
                            new NumericInterfaceable("FLAPPOS", 0, 65535),
                            new NumericInterfaceable("GEAR_HORN_SILENCE", 0, 1, false),
                            new NumericInterfaceable("GEAR_LEVER", 0, 1, false),
                            new NumericInterfaceable("GEARLSAFE", 0, 1),
                            new NumericInterfaceable("GEARNSAFE", 0, 1),
                            new NumericInterfaceable("GEARRSAFE", 0, 1),
                            new NumericInterfaceable("HANDLEGEARWARNING", 0, 1),
                            new NumericInterfaceable("LIGHTS", 0, 2)})
                },
                {
                    "LMFD", new Panel("LMFD", new NumericInterfaceable[] {
                            new NumericInterfaceable("01", 0, 1),
                            new NumericInterfaceable("02", 0, 1),
                            new NumericInterfaceable("03", 0, 1),
                            new NumericInterfaceable("04", 0, 1),
                            new NumericInterfaceable("05", 0, 1),
                            new NumericInterfaceable("06", 0, 1),
                            new NumericInterfaceable("07", 0, 1),
                            new NumericInterfaceable("08", 0, 1),
                            new NumericInterfaceable("09", 0, 1),
                            new NumericInterfaceable("10", 0, 1),
                            new NumericInterfaceable("11", 0, 1),
                            new NumericInterfaceable("12", 0, 1),
                            new NumericInterfaceable("13", 0, 1),
                            new NumericInterfaceable("14", 0, 1),
                            new NumericInterfaceable("15", 0, 1),
                            new NumericInterfaceable("16", 0, 1),
                            new NumericInterfaceable("17", 0, 1),
                            new NumericInterfaceable("18", 0, 1),
                            new NumericInterfaceable("19", 0, 1),
                            new NumericInterfaceable("20", 0, 1),
                            new NumericInterfaceable("ADJ", 0, 2),
                            new NumericInterfaceable("BRT", 0, 2),
                            new NumericInterfaceable("CON", 0, 2),
                            new NumericInterfaceable("DSP", 0, 2),
                            new NumericInterfaceable("PWR", 0, 2),
                            new NumericInterfaceable("SYM", 0, 2)}) 
                },
                {
                    "LCP", new Panel("LCP", new NumericInterfaceable[] {
                            new NumericInterfaceable("ACCEL_COMP", 0, 1),
                            new NumericInterfaceable("ANTICOLLISION", 0, 1, mm: new Dictionary<int,string>() {
                                {0, "OFF"},
                                {1, "PUSH" }}),
                            new NumericInterfaceable("AUX_INST", 0, 65535),
                            new NumericInterfaceable("CONSOLE", 0, 65535),
                            new NumericInterfaceable("ENG_INST", 0, 65535),
                            new NumericInterfaceable("FLIGHT_INST", 0, 65535),
                            new NumericInterfaceable("FLOOD", 0, 65535),
                            new NumericInterfaceable("FORMATION", 0, 65535),
                            new NumericInterfaceable("NOSE_ILLUM", 0, 1),
                            new NumericInterfaceable("POSITION", 0, 2),
                            new NumericInterfaceable("SIGNAL_LIGHTS", 0, 1)})
                },
                {
                    "MISC", new Panel("MISC", new NumericInterfaceable[] {
                            new NumericInterfaceable("AUX_GEAR", 0, 1, false),
                            new NumericInterfaceable("AUX_GEAR_LOCK", 0, 1, false),
                            new NumericInterfaceable("CANOPY_DISENGAGE", 0, 1, false),
                            new NumericInterfaceable("CANOPY_JTSN", 0, 1, false),
                            new NumericInterfaceable("CANOPY_JTSN_UNLOCK", 0, 1, false),
                            new NumericInterfaceable("CANOPY_OPEN", 0, 2, false),
                            new NumericInterfaceable("EMER_BRAKE", 0, 1, false),
                            new NumericInterfaceable("GND_SAFE_OVERRIDE", 0, 1, false),
                            new NumericInterfaceable("GND_SAFE_OVERRIDE_COVER", 0, 1, false),
                            new NumericInterfaceable("LADDER_EXTEND", 0, 1, false),
                            new NumericInterfaceable("LADDER_EXTEND_COVER", 0, 1, false),
                            new NumericInterfaceable("SEAT_ADJUST", 0, 2, false),
                            new NumericInterfaceable("SEAT_ARM", 0, 1, false),
                            new NumericInterfaceable("SUIT_TEST", 0, 1, false)})
                },
                {
                    "NMSP", new Panel("NMSP", new NumericInterfaceable[] {
                            new NumericInterfaceable("ABLE_STOW", 0, 1),
                            new NumericInterfaceable("ANCHR_BTN", 0, 1),
                            new NumericInterfaceable("ANCHRLED", 0, 1),
                            new NumericInterfaceable("EGI_BTN", 0, 1),
                            new NumericInterfaceable("EGILED", 0, 1),
                            new NumericInterfaceable("FMLED", 0, 1),
                            new NumericInterfaceable("HARS_BTN", 0, 1),
                            new NumericInterfaceable("HARSLED", 0, 1),
                            new NumericInterfaceable("ILS_BTN", 0, 1),
                            new NumericInterfaceable("ILSLED", 0, 1),
                            new NumericInterfaceable("STEERPT_BTN", 0, 1),
                            new NumericInterfaceable("STEERPTLED", 0, 1),
                            new NumericInterfaceable("TCN_BTN", 0, 1),
                            new NumericInterfaceable("TCNLED", 0, 1),
                            new NumericInterfaceable("TISL_BTN", 0, 1),
                            new NumericInterfaceable("TISLLED", 0, 1),
                            new NumericInterfaceable("UHFLED", 0, 1)})
                },
                {
                    "OXY", new Panel("OXY", new NumericInterfaceable[] {
                            new NumericInterfaceable("DILUTER", 0, 1),
                            new NumericInterfaceable("EMERGENCY", 0, 2),
                            new NumericInterfaceable("FLOW", 0, 1),
                            new NumericInterfaceable("PRESS", 0, 65535),
                            new NumericInterfaceable("SUPPLY", 0, 1)})
                },
                {
                    "RWR", new Panel("RWR", new NumericInterfaceable[] {
                            new NumericInterfaceable("BRT", 0, 65535)})
                },
                {
                    "RMFD", new Panel("RMFD", new NumericInterfaceable[] {
                            new NumericInterfaceable("01", 0, 1),
                            new NumericInterfaceable("02", 0, 1),
                            new NumericInterfaceable("03", 0, 1),
                            new NumericInterfaceable("04", 0, 1),
                            new NumericInterfaceable("05", 0, 1),
                            new NumericInterfaceable("06", 0, 1),
                            new NumericInterfaceable("07", 0, 1),
                            new NumericInterfaceable("08", 0, 1),
                            new NumericInterfaceable("09", 0, 1),
                            new NumericInterfaceable("10", 0, 1),
                            new NumericInterfaceable("11", 0, 1),
                            new NumericInterfaceable("12", 0, 1),
                            new NumericInterfaceable("13", 0, 1),
                            new NumericInterfaceable("14", 0, 1),
                            new NumericInterfaceable("15", 0, 1),
                            new NumericInterfaceable("16", 0, 1),
                            new NumericInterfaceable("17", 0, 1),
                            new NumericInterfaceable("18", 0, 1),
                            new NumericInterfaceable("19", 0, 1),
                            new NumericInterfaceable("20", 0, 1),
                            new NumericInterfaceable("ADJ", 0, 2),
                            new NumericInterfaceable("BRT", 0, 2),
                            new NumericInterfaceable("CON", 0, 2),
                            new NumericInterfaceable("DSP", 0, 2),
                            new NumericInterfaceable("PWR", 0, 2),
                            new NumericInterfaceable("SYM", 0, 2)})
                },
                {
                    "SASP", new Panel("SASP", new NumericInterfaceable[] {
                            new NumericInterfaceable("MONITOR_TEST", 0, 2),
                            new NumericInterfaceable("PITCH_SAS_L", 0, 1, mm: new Dictionary<int,string>() {
                                {0, "OFF"},
                                {1, "PUSH" }}),
                            new NumericInterfaceable("PITCH_SAS_R", 0, 1, mm: new Dictionary<int,string>() {
                                {0, "OFF"},
                                {1, "PUSH" }}),
                            new NumericInterfaceable("TO_TRIM", 0, 1),
                            new NumericInterfaceable("YAW_SAS_L", 0, 1, mm: new Dictionary<int,string>() {
                                {0, "OFF"},
                                {1, "PUSH" }}),
                            new NumericInterfaceable("YAW_SAS_R", 0, 1, mm: new Dictionary<int,string>() {
                                {0, "OFF"},
                                {1, "PUSH" }}),
                            new NumericInterfaceable("YAW_TRIM", 0, 65535),
                            new NumericInterfaceable("TAKEOFFTRIMLED", 0, 1)})
                },
                {
                    "KY58", new Panel("KY58", new NumericInterfaceable[] {
                            new NumericInterfaceable("1TO5", 0, 5),
                            new NumericInterfaceable("DELAY", 0, 1),
                            new NumericInterfaceable("MODE", 0, 2),
                            new NumericInterfaceable("PLAIN", 0, 2),
                            new NumericInterfaceable("PWR", 0, 1),
                            new NumericInterfaceable("ZEROIZE", 0, 1),
                            new NumericInterfaceable("ZEROIZE_COVER", 0, 1)})
                },
                {
                    "STALL", new Panel("STALL", new NumericInterfaceable[] {
                            new NumericInterfaceable("PEAK_VOL", 0, 65535),
                            new NumericInterfaceable("VOL", 0, 65535)})
                },
                {
                    "SAI", new Panel("SAI", new NumericInterfaceable[] {
                            new NumericInterfaceable("BANK", 0, 65535),
                            new NumericInterfaceable("CAGE", 0, 1),
                            new NumericInterfaceable("KNOBARROW", 0, 65535),
                            new NumericInterfaceable("PITCH", 0, 65535),
                            new NumericInterfaceable("PITCHADJ", 0, 65535),
                            new NumericInterfaceable("PITCH_TRIM", 0, 65535),
                            new NumericInterfaceable("WARNINGFLAG", 0, 65535)})
                },
                {
                    "TACAN", new Panel("TACAN", new NumericInterfaceable[] {
                            new NumericInterfaceable("1", 0, 10),
                            new NumericInterfaceable("10", 0, 10),
                            new NumericInterfaceable("MODE", 0, 4),
                            new NumericInterfaceable("TEST", 0, 1),
                            new NumericInterfaceable("TEST_BTN", 0, 1),
                            new NumericInterfaceable("VOL", 0, 65535),
                            new NumericInterfaceable("XY", 0, 1)},
                        new TextInterfaceable[] {
                            new TextInterfaceable("CHANNELBUFFER", "")})
                },
                {
                    "TISL", new Panel("TISL", new NumericInterfaceable[] {
                            new NumericInterfaceable("ALT_1000", 0, 9),
                            new NumericInterfaceable("ALT_10000", 0, 9),
                            new NumericInterfaceable("AUX", 0, 2),
                            new NumericInterfaceable("BITE", 0, 1),
                            new NumericInterfaceable("CODE1", 0, 19),
                            new NumericInterfaceable("CODE2", 0, 19),
                            new NumericInterfaceable("CODE3", 0, 19),
                            new NumericInterfaceable("CODE4", 0, 19),
                            new NumericInterfaceable("ENTER", 0, 1),
                            new NumericInterfaceable("MODE", 0, 4),
                            new NumericInterfaceable("SLANT_RANGE", 0, 2)})
                },
                {
                    "THROTTLE", new Panel("THROTTLE", new NumericInterfaceable[] {
                            new NumericInterfaceable("ENGINE_APU_START", 0, 1, false),
                            new NumericInterfaceable("ENGINE_FUEL_FLOW_L", 0, 1, false),
                            new NumericInterfaceable("ENGINE_FUEL_FLOW_R", 0, 1, false),
                            new NumericInterfaceable("ENGINE_OPER_L", 0, 2, false),
                            new NumericInterfaceable("ENGINE_OPER_R", 0, 2, false),
                            new NumericInterfaceable("ENGINE_THROTTLE_FRICTION", 0, 65535, false),
                            new NumericInterfaceable("FLAPS_SWITCH", 0, 2, false)})
                },
                {
                    "UFC", new Panel("UFC", new NumericInterfaceable[] {
                            new NumericInterfaceable("MASTERCAUTIONLED", 0, 1),
                            new NumericInterfaceable("1", 0, 1),
                            new NumericInterfaceable("10", 0, 1),
                            new NumericInterfaceable("2", 0, 1),
                            new NumericInterfaceable("3", 0, 1),
                            new NumericInterfaceable("4", 0, 1),
                            new NumericInterfaceable("5", 0, 1),
                            new NumericInterfaceable("6", 0, 1),
                            new NumericInterfaceable("7", 0, 1),
                            new NumericInterfaceable("8", 0, 1),
                            new NumericInterfaceable("9", 0, 1),
                            new NumericInterfaceable("ALT_ALRT", 0, 1),
                            new NumericInterfaceable("CLR", 0, 1),
                            new NumericInterfaceable("DATA", 0, 2),
                            new NumericInterfaceable("DEPR", 0, 2),
                            new NumericInterfaceable("ENT", 0, 1),
                            new NumericInterfaceable("FUNC", 0, 1),
                            new NumericInterfaceable("HACK", 0, 1),
                            new NumericInterfaceable("INTEN", 0, 2),
                            new NumericInterfaceable("LTR", 0, 1),
                            new NumericInterfaceable("MASTER_CAUTION", 0, 1),
                            new NumericInterfaceable("MK", 0, 1),
                            new NumericInterfaceable("NA1", 0, 1),
                            new NumericInterfaceable("NA2", 0, 1),
                            new NumericInterfaceable("NA3", 0, 1),
                            new NumericInterfaceable("NA4", 0, 1),
                            new NumericInterfaceable("NA5", 0, 1),
                            new NumericInterfaceable("NA6", 0, 1),
                            new NumericInterfaceable("SEL", 0, 2),
                            new NumericInterfaceable("SPC", 0, 1),
                            new NumericInterfaceable("STEER", 0, 2)})
                },
                {
                    "UHF", new Panel("UHF", new NumericInterfaceable[] {
                            new NumericInterfaceable("100MHZ_SEL", 0, 2),
                            new NumericInterfaceable("10MHZ_SEL", 0, 9),
                            new NumericInterfaceable("1MHZ_SEL", 0, 9),
                            new NumericInterfaceable("COVER", 0, 1),
                            new NumericInterfaceable("FUNCTION", 0, 3),
                            new NumericInterfaceable("LOAD", 0, 1),
                            new NumericInterfaceable("MODE", 0, 2),
                            new NumericInterfaceable("POINT1MHZ_SEL", 0, 9),
                            new NumericInterfaceable("POINT25_SEL", 0, 3),
                            new NumericInterfaceable("PRESET_SEL", 0, 20),
                            new NumericInterfaceable("SQUELCH", 0, 1),
                            new NumericInterfaceable("STATUS", 0, 1),
                            new NumericInterfaceable("TEST", 0, 1),
                            new NumericInterfaceable("T_TONE", 0, 2),
                            new NumericInterfaceable("VOL", 0, 1)},
                        new TextInterfaceable[] {
                            new TextInterfaceable("FREQUENCYBUFFER", ""),
                            new TextInterfaceable("PRESETBUFFER", "")})
                },
                {
                    "VHFAM", new Panel("VHFAM", new NumericInterfaceable[] {
                            new NumericInterfaceable("FREQ1", 0, 12),
                            new NumericInterfaceable("FREQ1ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQ2", 0, 9),
                            new NumericInterfaceable("FREQ2ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQ3", 0, 9),
                            new NumericInterfaceable("FREQ3ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQ4", 0, 4),
                            new NumericInterfaceable("FREQ4ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQEMER", 0, 3),
                            new NumericInterfaceable("LOAD", 0, 1),
                            new NumericInterfaceable("MODE", 0, 2),
                            new NumericInterfaceable("PRESET", 0, 19),
                            new NumericInterfaceable("SQUELCH", 0, 2),
                            new NumericInterfaceable("VOL", 0, 65535)})
                },
                {
                    "VHFFM", new Panel("VHFFM", new NumericInterfaceable[] {
                            new NumericInterfaceable("FREQ1", 0, 12),
                            new NumericInterfaceable("FREQ1ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQ2", 0, 9),
                            new NumericInterfaceable("FREQ2ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQ3", 0, 9),
                            new NumericInterfaceable("FREQ3ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQ4", 0, 4),
                            new NumericInterfaceable("FREQ4ROTVALUE", 0, 255),
                            new NumericInterfaceable("FREQEMER", 0, 3),
                            new NumericInterfaceable("LOAD", 0, 1),
                            new NumericInterfaceable("MODE", 0, 2),
                            new NumericInterfaceable("PRESET", 0, 19),
                            new NumericInterfaceable("SQUELCH", 0, 2),
                            new NumericInterfaceable("VOL", 0, 65535)})
                },
                {
                    "VVI", new Panel("VVI", new NumericInterfaceable[] {
                            new NumericInterfaceable("VVI", 0, 65535)})
                },


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
        private bool _sendControlGroupViaUDP;
        private Dictionary<int, string> _messageMap;

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
                    string valueToSend = _value.ToString();

                    // check for a messageMap entry corresponding to the current _value
                    if(_messageMap != null && _messageMap.Keys.Contains(_value))
                    {
                        valueToSend = _messageMap[_value];
                    }

                    Message msg = new Message(null, Name, "INT", valueToSend, _sendControlGroupViaUDP);
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

        public NumericInterfaceable(string n, int v, int m, bool sCGVUDP = true, Dictionary<int, string> mm = null) : base(n)
        {
            _value = v;
            _maxValue = m;
            _sendControlGroupViaUDP = sCGVUDP;
            _messageMap = mm;
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
        private bool _sendControlGroupViaUDP;

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
                    Message msg = new Message(null, Name, "STR", _value, _sendControlGroupViaUDP);
                    OnChanged(new MessageReadyEventArgs(msg));
                }

            }
             
        }

        public TextInterfaceable(string n, string v, bool sCGVUDP = true) : base(n)
        {
            _value = v;
            _sendControlGroupViaUDP = sCGVUDP;
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
