﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaderTweaks
{
	public class LeaderPatcherAttribute : Attribute
	{
		public string ID { get; set; }
		public LeaderPatcherAttribute([System.Runtime.CompilerServices.CallerMemberName] string id = "")
		{
			ID = id;
		}
	}
}
