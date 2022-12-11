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
    [LeaderPatcher("Clipboard.SelectionPanel Tweaks", "Clipboard")]
	public class ClipboardSelectionPanel : IPatcher
	{
		public void Init(Harmony patcher)
		{
			var pt = typeof(ClipboardSelectionPanel);
			var t = typeof(SelectionPanel);

			patcher.Patch(AccessTools.Method(t, "CopyGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardSelectionPanel.CopyGUID))));

			patcher.Patch(AccessTools.Method(t, "CopyNameGUIDClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardSelectionPanel.CopyNameGUID))));

			/*patcher.Patch(AccessTools.Method(t, "CopyPositionClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, "CopyPosition")));*/

			patcher.Patch(AccessTools.Method(t, "AddRightClickMenuItem"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ClipboardSelectionPanel.UpdateDropdownText))));
		}
		private static void UpdateDropdownText(ToolStripMenuItem item)
		{
			if (item.Name == "GUIDName")
			{
				item.Text = "Copy Name_GUID to clipboard";
			}
		}

		private static bool TryGetEntityFromItem(Dictionary<Guid, Entity> entities, ListViewItem item, out Entity entity)
		{
			entity = null;
			if (item.Tag is Guid guid)
			{
				if(entities.TryGetValue(guid, out var e))
				{
					entity = e;
					return true;
				}
			}
			return false;
		}

		private static bool GetSelectedOutput(SelectionPanel.DBListView lv, Dictionary<Guid, Entity> entities, ResourceClipboardOutputType outputType, out string output)
		{
			output = string.Empty;
			var count = lv.SelectedIndices.Count;
			if (count <= 0)
				return false;

			var selectedItems = ResourceTools.GetSelectedItems<ListViewItem>(lv);

			if (count > 1)
			{
				var selectedEntities = new List<string>();
				foreach(var item in selectedItems)
				{
					if(TryGetEntityFromItem(entities, item, out var entity))
					{
						selectedEntities.Add(ResourceTools.GetOutput(entity, outputType));
					}
				}
				output = string.Join(Environment.NewLine, selectedEntities);
				return true;
			}
			else
			{
				if (TryGetEntityFromItem(entities, selectedItems.First(), out var entity))
				{
					output = ResourceTools.GetOutput(entity, outputType);
					return true;
				}
			}
			return false;
		}

		public static bool CopyGUID(SelectionPanel.DBListView ___ItemList, Dictionary<Guid, Entity> ___m_Items)
		{
			if (GetSelectedOutput(___ItemList, ___m_Items, ResourceClipboardOutputType.GUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}

		public static bool CopyNameGUID(SelectionPanel.DBListView ___ItemList, Dictionary<Guid, Entity> ___m_Items)
		{
			if (GetSelectedOutput(___ItemList, ___m_Items, ResourceClipboardOutputType.NameGUID, out string output))
			{
				Clipboard.SetDataObject(output, true);
			}
			return false;
		}
	}
}
