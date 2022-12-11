using EoCPlugin;

using HarmonyLib;
using LeaderTweaks.Util;
using LSFrameworkPlugin;

using LSToolFramework;

using Mono.Cecil.Cil;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;

namespace LeaderTweaks.Patches.ClipboardPatcher
{
	//Right click options for objects in the level

    [LeaderPatcher("Clipboard.LevelPlugin Tweaks", "Clipboard")]
	public class ClipboardLevelPlugin : IPatcher
	{
		public void Init(Harmony patcher)
		{
			var pt = typeof(ClipboardLevelPlugin);
			var t = typeof(LevelPlugin);

			patcher.Patch(AccessTools.Method(t, "CopyGUIDClicked"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardLevelPlugin.CopyGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyNameGUIDClicked"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardLevelPlugin.CopyNameGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyTypeNameGUIDClicked"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardLevelPlugin.CopyTypeNameGUID))));

			patcher.Patch(AccessTools.Method(t, "AddMenuItems"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardLevelPlugin.EditMenuItems))));
		}

		static readonly FieldInfo f_m_ContextMenu = AccessTools.Field(typeof(RightClickEventArgs), "m_ContextMenu");

		public static void EditMenuItems(object A_0, RightClickEventArgs A_1)
		{
			if (f_m_ContextMenu.GetValue(A_1) is ContextMenuStrip cm)
			{
				foreach (var item in cm.Items.OfType<ToolStripMenuItem>())
				{
					switch(item.Name)
					{
						case "copyNameGUIDItem":
							item.Text = "Copy Name_GUID to clipboard";
							break;
						case "copyTypeNameGUIDItem":
							item.Text = "Copy Type_Name_GUID to clipboard";
							break;
					}
				}
			}
		}

		private static bool GetSelectedOutput(ResourceClipboardOutputType outputType, out string output)
		{
			output = string.Empty;
			var selectedItems = SelectionManager.Instance?.CurrentSelection.OfType<EditableObject>().ToList();
			var count = selectedItems.Count;
			if (count <= 0)
				return false;

			if (count > 1)
			{
				output = string.Join(Environment.NewLine, selectedItems.Select(x => ResourceTools.GetOutput(x, outputType)));
			}
			else
			{
				output = ResourceTools.GetOutput(selectedItems.First(), outputType);
			}
			return true;
		}

		public static bool CopyGUID()
		{
			if (GetSelectedOutput(ResourceClipboardOutputType.GUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}

		public static bool CopyNameGUID()
		{
			if (GetSelectedOutput(ResourceClipboardOutputType.NameGUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}
		public static bool CopyTypeNameGUID()
		{
			if (GetSelectedOutput(ResourceClipboardOutputType.TypeNameGUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}
	}
}
