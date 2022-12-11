using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tomlet.Models;
using Tomlet.Attributes;

namespace LeaderTweaks
{
	public class Settings
	{
		[TomlPrecedingComment("Additional messages to block/ignore in the message log. Uses regex.")]
		public List<string> IgnoreLogMessages { get; set; }

		[TomlPrecedingComment("Specific patches to disable.")]
		public PatchDisableSettings DisablePatches { get; set; }

		[TomlPrecedingComment("Force-rename the game window tab to use the provided name (ex. 'Game' instead of 'Eyes of a Child'). This requires the 'Panels' patch to be enabled.")]
		public string GameWindowName { get; set; } = "Game";

		public class PatchDisableSettings
		{
			[TomlPrecedingComment("Disable the editor's ability to load saves. This can speed up loading, due to the fact the editor will try to instantiate mods stored in saves for the active profile.")]
			public bool SaveLoading { get; set; }

			[TomlPrecedingComment("Clipboard improvements, such as removing the linebreak when copying a single GUID, and making Name <GUID> consistent with the other options (making it Name_GUID instead).")]
			public bool Clipboard { get; set; }

			[TomlPrecedingComment("Tweaks to make the resource wizard (content browser) start in the correct folder when adding resources, and displaying the correct file types when adding a new resource.")]
			public bool ResourceWizard { get; set; }

			[TomlPrecedingComment("Improvements to the message log to color specific message types, and block spammy messages.")]
			public bool Messages { get; set; }

			[TomlPrecedingComment("Makes the root templates panel load even if a level isn't loaded.")]
			public bool RootTemplatesPanel { get; set; }

			[TomlPrecedingComment("Fixes various resource issues, such as physics previewing without a level loaded crashing the editor, the animation window being broken ('read-only' despite being a resource in the project), and certain resources always being 'inherited'/undeletable despite being local to the project.")]
			public bool Resources { get; set; }

			[TomlPrecedingComment("Fixes various issues with panels, such as options being broken (create prefab, wall construction, export to root template, tile set creation).")]
			public bool Panels { get; set; }

			[TomlPrecedingComment("Attempts to fix Project Settings issues, such as the project version/meta not reloading if edited externally.")]
			public bool ProjectSettings { get; set; }
		}

		public Settings()
		{
			IgnoreLogMessages = new List<string>();
			DisablePatches = new PatchDisableSettings();
		}
	}
}
