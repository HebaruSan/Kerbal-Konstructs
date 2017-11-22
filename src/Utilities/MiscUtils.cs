using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.UI.Screens;

namespace KerbalKonstructs.Utilities
{
	public class MiscUtils
	{
		public static Boolean CareerStrategyEnabled(Game gGame)
		{
			return gGame.Mode == Game.Modes.CAREER
			 	&& !KerbalKonstructs.instance.disableCareerStrategyLayer;
		}

		public static Boolean isCareerGame()
		{
			return HighLogic.CurrentGame.Mode == Game.Modes.CAREER
			 	&& !KerbalKonstructs.instance.disableCareerStrategyLayer;
		}

		public static void PostMessage(string sTitle, string sMessage, MessageSystemButton.MessageButtonColor cColor,
			MessageSystemButton.ButtonIcons bIcon)
		{
			MessageSystem.Message m = new MessageSystem.Message(sTitle, sMessage, cColor, bIcon);
			MessageSystem.Instance.AddMessage(m);
		}

		public static void HUDMessage(string sMessage, float fDuration = 10f, int iStyle = 2)
		{
			ScreenMessageStyle smsStyle = (ScreenMessageStyle)iStyle;
			ScreenMessages.PostScreenMessage(sMessage, fDuration, smsStyle);
		}
	}
}
