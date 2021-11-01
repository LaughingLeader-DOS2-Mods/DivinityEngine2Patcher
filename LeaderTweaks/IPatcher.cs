using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaderTweaks
{
	public interface IPatcher
	{
		void Init(HarmonyLib.Harmony patcher);
	}
}
