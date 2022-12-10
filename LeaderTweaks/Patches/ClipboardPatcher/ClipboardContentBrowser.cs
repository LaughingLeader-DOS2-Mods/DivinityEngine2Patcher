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
    [LeaderPatcher("Clipboard.ContentBrowser Tweaks", "Clipboard")]
	public class ClipboardContentBrowser : IPatcher
	{
		public void Init(Harmony patcher)
		{
			var pt = typeof(ClipboardContentBrowser);
			var t = typeof(ContentBrowser);

			patcher.Patch(AccessTools.Method(t, "CopyGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardContentBrowser.CopyGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyNamedGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardContentBrowser.CopyNameGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyTypeNameGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardContentBrowser.CopyTypeNameGUID))));

			patcher.Patch(AccessTools.Method(t, "CreateDependencyOptions"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardContentBrowser.UpdateDropdownText))));
		}

		private static void UpdateDropdownText(ContextMenuStrip ___cmsListViewRightClick)
		{
			foreach (var item in ___cmsListViewRightClick.Items)
			{
				if (item is ToolStripMenuItem menuItem && menuItem.Name == "Named GUID")
				{
					menuItem.Text = "Copy Name_GUID to clipboard";
					break;
				}
			}
		}

		private static bool GetSelectedOutput(ListView lv, ResourceClipboardOutputType outputType, out string output)
		{
			output = string.Empty;
			var count = lv.SelectedIndices.Count;
			if (count <= 0)
				return false;

			if (count > 1)
			{
				output = string.Join(Environment.NewLine, ResourceTools.GetSelectedItems<ContentBrowserListViewItem>(lv).Select(x => ResourceTools.GetOutput(x.ResourceRef, outputType)));
				return true;
			}
			else
			{
				if (lv.SelectedItems[0] is ContentBrowserListViewItem item)
				{

					output = ResourceTools.GetOutput(item.ResourceRef, outputType);
					return true;
				}
			}
			return false;
		}

		public static bool CopyGUID(ListView ___m_List)
		{
			if (GetSelectedOutput(___m_List, ResourceClipboardOutputType.GUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}

		public static bool CopyNameGUID(ListView ___m_List)
		{
			if (GetSelectedOutput(___m_List, ResourceClipboardOutputType.NameGUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}
		public static bool CopyTypeNameGUID(ListView ___m_List)
		{
			if (GetSelectedOutput(___m_List, ResourceClipboardOutputType.TypeNameGUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}
	}
}
