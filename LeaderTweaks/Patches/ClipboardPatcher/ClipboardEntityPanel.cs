using EoCPlugin;

using HarmonyLib;
using LeaderTweaks.Util;
using LSFrameworkPlugin;

using LSToolFramework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeaderTweaks.Patches.ClipboardPatcher
{
    /* 
	 * Removes the link break that gets incorrectly appended to the end.
	*/
    [LeaderPatcher("Clipboard.EntityPanel Tweaks", "Clipboard")]
	public class ClipboardEntityPanel : IPatcher
	{
		public void Init(Harmony patcher)
		{
			var pt = typeof(ClipboardEntityPanel);
			var t = typeof(EntityPanel);

			patcher.Patch(AccessTools.Method(t, "CopyGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardEntityPanel.CopyGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyNameGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardEntityPanel.CopyNameGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyTypeNameGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardEntityPanel.CopyTypeNameGUID))));

			patcher.Patch(AccessTools.Method(t, "AddRightClickMenuItem"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardEntityPanel.UpdateDropdownText))));
		}

		private static void UpdateDropdownText(ToolStripMenuItem item)
		{
			if (item.Name == "NameGUID")
			{
				item.Text = "Copy Name_GUID to clipboard";
			}
		}

		private static Entity EntityFromItem(EntityController controller, ListViewItem item)
		{
			return controller.Objects[(Guid)item.Tag];
		}

		private static bool GetSelectedOutput(EntityController controller, ListView lv, ResourceClipboardOutputType outputType, out string output)
		{
			output = string.Empty;
			var count = lv.SelectedIndices.Count;
			if (count <= 0)
				return false;

			if (count > 1)
			{
				output = string.Join(Environment.NewLine, ResourceTools.GetSelectedItems<ListViewItem>(lv).Select(x => ResourceTools.GetOutput(EntityFromItem(controller, x), outputType)));
				return true;
			}
			else
			{
				if (lv.SelectedItems[0] is ListViewItem item)
				{

					output = ResourceTools.GetOutput(EntityFromItem(controller, item), outputType);
					return true;
				}
			}
			return false;
		}

		public static bool CopyGUID(ListView ___m_lstPalettes, EntityController ___m_Controller)
		{
			if (GetSelectedOutput(___m_Controller, ___m_lstPalettes, ResourceClipboardOutputType.GUID, out var output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}

		public static bool CopyNameGUID(ListView ___m_lstPalettes, EntityController ___m_Controller)
		{
			if (GetSelectedOutput(___m_Controller, ___m_lstPalettes, ResourceClipboardOutputType.NameGUID, out var output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}

		public static bool CopyTypeNameGUID(ListView ___m_lstPalettes, EntityController ___m_Controller)
		{
			if (GetSelectedOutput(___m_Controller, ___m_lstPalettes, ResourceClipboardOutputType.TypeNameGUID, out var output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}
	}
}
