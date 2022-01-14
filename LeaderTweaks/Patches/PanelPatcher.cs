using EoCPlugin;

using HarmonyLib;

using LSFrameworkPlugin;

using LSToolFramework;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Panel Fixes")]
	public class PanelPatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(PanelPatcher);

			harmony.Patch(AccessTools.Method(typeof(PanelService), nameof(PanelService.SetTitle)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.OnPanelServiceSetTitle))));

			harmony.Patch(AccessTools.Method(typeof(MWallTileSet), nameof(MWallTileSet.TileCanBelongToTileSet)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.MWallTileSet_TileCanBelongToTileSet_Fix))));

			harmony.Patch(AccessTools.Method(typeof(PrefabPlugin), nameof(PrefabPlugin.CreatePrefabClicked)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.PrefabPlugin_CreatePrefabClicked_Fix))));
			harmony.Patch(AccessTools.Method(typeof(WallConstructionPanel), "CreateRootTemplateClick"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.WallConstructionPanel_CreateRootTemplateClick_Fix))));
			harmony.Patch(AccessTools.Method(typeof(LevelPlugin), "ExportSelectedToRootTemplate"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.LevelPlugin_ExportSelectedToRootTemplate_Fix))));

			harmony.Patch(AccessTools.Method(typeof(PrefabPlugin), "AddMenuItems"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.PrefabPlugin_AddMenuItems_Fix))));

			harmony.Patch(AccessTools.Method(typeof(TileSetEditorDialog), "InitializeComponent"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(PanelPatcher.TileSetEditorDialog_InitializeComponent_Fix))));
		}

		//Shorten the Eyes of a Child text
		public static void OnPanelServiceSetTitle(System.Windows.FrameworkElement content, ref string title)
		{
			if (title.Contains("Eyes of"))
			{
				title = "Game";
			}
		}

		/* 
		 * Fixes "Create Prefab" option in the context menu
		 * Issue: Panel not added to panel manager service before it is shown
		 * Source: Norbyte
		*/
		public static bool PrefabPlugin_CreatePrefabClicked_Fix(object sender, MouseEventArgs e)
		{
			PanelService service = ToolFramework.Instance.ServiceManagerInstance.GetService<PanelService>();
			if (service != null)
			{
				CreatePrefabWizard panel = new CreatePrefabWizard();
				service.AddPanel(panel, EDockState.Float);
				service.Show(panel);
			}
			return false;
		}

		static readonly PropertyInfo p_EntityPanel = AccessTools.Property(typeof(EntityPanel), "Controller");
		static readonly PropertyInfo p_m_lstPalettes = AccessTools.Property(typeof(EntityPanel), "m_lstPalettes");

		/* 
		 * Fixes the wall construction create panel
		 * Source: Norbyte
		*/
		public static bool WallConstructionPanel_CreateRootTemplateClick_Fix(object A_0, EventArgs A_1, WallConstructionPanel __instance)
		{
			PanelService service = ToolFramework.Instance?.ServiceManagerInstance?.GetService<PanelService>();
			if (service != null)
			{
				if (service.GetPanel<CreateWallConstructionWizard>() == null)
				{
					WallConstructionPanel wallPanel = service.GetPanel<WallConstructionPanel>();
					if (wallPanel != null)
					{
						EntityPanel.DBListView m_lstPalettes = p_m_lstPalettes.GetValue(wallPanel, null) as EntityPanel.DBListView;
						EntityController controller = p_EntityPanel.GetValue(wallPanel, null) as EntityController;
						if (controller != null && m_lstPalettes != null)
						{
							ListViewItem listViewItem = m_lstPalettes.Items[m_lstPalettes.SelectedIndices[0]];
							MWallConstruction construction = controller.Objects[(Guid)listViewItem.Tag] as MWallConstruction;
							CreateWallConstructionWizard panel = new CreateWallConstructionWizard(construction);
							service.AddPanel(panel, EDockState.Float);
							service.Show(panel);
						}
					}
				}
			}
			return false;
		}

		/* 
		 * Fixes 'Export To RootTemplate' by making sure the panel is created.
		 * Source: Norbyte
		*/
		public static bool LevelPlugin_ExportSelectedToRootTemplate_Fix(object sender, MouseEventArgs e)
		{
			if (SelectionManager.Instance.CurrentSelection.Count == 1)
			{
				MGameObjectTemplate mgameObjectTemplate = (MGameObjectTemplate)SelectionManager.Instance.CurrentSelection[0];
				if (mgameObjectTemplate != null && !mgameObjectTemplate.IsRootTemplate())
				{
					PanelService service = ToolFramework.Instance.ServiceManagerInstance.GetService<PanelService>();
					if (service != null)
					{
						CreateObjectWizard panel = new CreateObjectWizard(mgameObjectTemplate);
						service.AddPanel(panel, EDockState.Float);
						service.Show(panel);
					}
				}
			}
			return false;
		}

		/* 
		 * Fixes "Create Prefab" option in the context menu
		*/
		public static bool PrefabPlugin_AddMenuItems_Fix(object A_0, RightClickEventArgs A_1, PrefabPlugin __instance)
		//PrefabPlugin __instance, MPrefabManager ___m_PrefabManager)
		{
			List<ToolStripItem> list = new List<ToolStripItem>();
			if (SelectionManager.Instance.HasSelectedObjects)
			{
				Assembly executingAssembly = Assembly.GetAssembly(typeof(PrefabPlugin));
				System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("LSFrameworkPlugin.Icons", executingAssembly);
				ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
				Size size = new Size(152, 22);
				toolStripMenuItem.Size = size;
				toolStripMenuItem.Text = "Create Prefab";
				toolStripMenuItem.Name = "createPrefabItem";
				toolStripMenuItem.Image = (Bitmap)resourceManager.GetObject("Prefab");
				list.Add(toolStripMenuItem);
				//A_1.AddMenuItems(list);
				//list.Clear(); // Fixed here
				ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
				Size size2 = new Size(152, 22);
				toolStripMenuItem2.Size = size2;
				toolStripMenuItem2.Text = "Group";
				toolStripMenuItem2.Name = "groupItem";
				toolStripMenuItem2.Image = (Bitmap)resourceManager.GetObject("Group");
				list.Add(toolStripMenuItem2);
				ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
				Size size3 = new Size(152, 22);
				toolStripMenuItem3.Size = size3;
				toolStripMenuItem3.Text = "Ungroup";
				toolStripMenuItem3.Name = "ungroupItem";
				toolStripMenuItem3.Image = (Bitmap)resourceManager.GetObject("Ungroup");
				list.Add(toolStripMenuItem3);
				A_1.AddMenuItems(list);
				using (IEnumerator<ISelectable> enumerator = SelectionManager.Instance.CurrentSelection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						EditableObject editableObject = enumerator.Current as EditableObject;
						if (editableObject != null && editableObject.TypeId == "terrain")
						{
							toolStripMenuItem.Enabled = false;
							break;
						}
					}
				}
				toolStripMenuItem.MouseUp += __instance.CreatePrefabClicked;
				toolStripMenuItem2.MouseUp += __instance.GroupClicked;
				toolStripMenuItem3.MouseUp += __instance.UngroupClicked;
			}

			return false;
		}

		/* 
		 * Fixes the wall construction create wizard
		 * Source: Norbyte
		*/
		public static void MWallTileSet_TileCanBelongToTileSet_Fix(string name, ref bool __result, string ___m_TileSetName)
		{
			string[] array = name.Split(new char[]
			{
				'_'
			});
			if (array.Length < 3)
			{
				return;
			}
			string text = array[1];
			if (array[2] == "ex")
			{
				text += "_ex";
			}
			__result = text == ___m_TileSetName || String.IsNullOrEmpty(___m_TileSetName);
		}

		public class TileSetEditorDialogPrivateFieldsAccessor
		{
			private TileSetEditorDialog _instance;

			public MWallTileSet m_TileSet { get { return (MWallTileSet)t_m_TileSet.GetValue(_instance); } set { t_m_TileSet.SetValue(_instance, value); } }
			public System.ComponentModel.IContainer components { get { return (System.ComponentModel.IContainer)t_components.GetValue(_instance); } set { t_components.SetValue(_instance, value); } }
			public Panel pnlButtons { get { return (Panel)t_pnlButtons.GetValue(_instance); } set { t_pnlButtons.SetValue(_instance, value); } }
			public Button btnOK { get { return (Button)t_btnOK.GetValue(_instance); } set { t_btnOK.SetValue(_instance, value); } }
			public ListBox lstTileSetPieces { get { return (ListBox)t_lstTileSetPieces.GetValue(_instance); } set { t_lstTileSetPieces.SetValue(_instance, value); } }
			public TextBox txtFilter { get { return (TextBox)t_txtFilter.GetValue(_instance); } set { t_txtFilter.SetValue(_instance, value); } }
			public Button btnAdd { get { return (Button)t_btnAdd.GetValue(_instance); } set { t_btnAdd.SetValue(_instance, value); } }
			public string m_SelectedID { get { return (string)t_m_SelectedID.GetValue(_instance); } set { t_m_SelectedID.SetValue(_instance, value); } }
			public Button btnRemove { get { return (Button)t_btnRemove.GetValue(_instance); } set { t_btnRemove.SetValue(_instance, value); } }
			public Button btnClear { get { return (Button)t_btnClear.GetValue(_instance); } set { t_btnClear.SetValue(_instance, value); } }


			public void btnRemove_Click(object sender, EventArgs e)
			{
				m_btnRemove_Click.Invoke(_instance, new object[] { sender, e });
			}

			public void btnClear_Click(object sender, EventArgs e)
			{
				m_btnClear_Click.Invoke(_instance, new object[] { sender, e });
			}

			public void btnAdd_Click(object sender, EventArgs e)
			{
				m_btnAdd_Click.Invoke(_instance, new object[] { sender, e });
			}

			public void btnOk_Click(object sender, EventArgs e)
			{
				m_btnOk_Click.Invoke(_instance, new object[] { sender, e });
			}

			public void lstTileSetPieces_SelectedIndexChanged(object sender, EventArgs e)
			{
				m_lstTileSetPieces_SelectedIndexChanged.Invoke(_instance, new object[] { sender, e });
			}

			public void txtFilter_TextChanged(object sender, EventArgs e)
			{
				m_txtFilter_TextChanged.Invoke(_instance, new object[] { sender, e });
			}

			static readonly Type t = typeof(TileSetEditorDialog);

			static readonly FieldInfo t_m_TileSet = AccessTools.Field(t, "m_TileSet");
			static readonly FieldInfo t_components = AccessTools.Field(t, "components");
			static readonly FieldInfo t_pnlButtons = AccessTools.Field(t, "pnlButtons");
			static readonly FieldInfo t_btnOK = AccessTools.Field(t, "btnOK");
			static readonly FieldInfo t_lstTileSetPieces = AccessTools.Field(t, "lstTileSetPieces");
			static readonly FieldInfo t_txtFilter = AccessTools.Field(t, "txtFilter");
			static readonly FieldInfo t_btnAdd = AccessTools.Field(t, "btnAdd");
			static readonly FieldInfo t_m_SelectedID = AccessTools.Field(t, "m_SelectedID");
			static readonly FieldInfo t_btnRemove = AccessTools.Field(t, "btnRemove");
			static readonly FieldInfo t_btnClear = AccessTools.Field(t, "btnClear");
			static readonly MethodInfo m_btnRemove_Click = AccessTools.Method(t, "btnRemove_Click");
			static readonly MethodInfo m_btnClear_Click = AccessTools.Method(t, "btnClear_Click");
			static readonly MethodInfo m_btnAdd_Click = AccessTools.Method(t, "btnAdd_Click");
			static readonly MethodInfo m_btnOk_Click = AccessTools.Method(t, "btnOk_Click");
			static readonly MethodInfo m_lstTileSetPieces_SelectedIndexChanged = AccessTools.Method(t, "lstTileSetPieces_SelectedIndexChanged");
			static readonly MethodInfo m_txtFilter_TextChanged = AccessTools.Method(t, "txtFilter_TextChanged");

			public TileSetEditorDialogPrivateFieldsAccessor(TileSetEditorDialog instance)
			{
				_instance = instance;
			}
		}

		private static bool RequestTileSetName(ref string input)
		{
			Size clientSize = new Size(300, 70);
			Form form = new Form();
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.ClientSize = clientSize;
			form.Text = "Tile set name:";
			TextBox textBox = new TextBox();
			textBox.Size = new Size(clientSize.Width - 10, 23);
			textBox.Location = new Point(5, 5);
			textBox.Text = input;
			form.Controls.Add(textBox);
			Button button = new Button();
			button.DialogResult = DialogResult.OK;
			button.Name = "okButton";
			button.Size = new Size(75, 23);
			button.Text = "&OK";
			button.Location = new Point(clientSize.Width - 80 - 80, 39);
			form.Controls.Add(button);
			Button button2 = new Button();
			button2.DialogResult = DialogResult.Cancel;
			button2.Name = "cancelButton";
			button2.Size = new Size(75, 23);
			button2.Text = "&Cancel";
			button2.Location = new Point(clientSize.Width - 80, 39);
			form.Controls.Add(button2);
			form.AcceptButton = button;
			form.CancelButton = button2;
			DialogResult dialogResult = form.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				input = textBox.Text;
			}
			return dialogResult == DialogResult.OK;
		}

		static readonly FieldInfo f_m_TileSetName = AccessTools.Field(typeof(MWallTileSet), "m_TileSetName");

		/* 
		 * Added a "Tile Set" button to the Tile Set Editor dialog.
		 * Source: Norbyte
		*/
		public static bool TileSetEditorDialog_InitializeComponent_Fix(TileSetEditorDialog __instance)
		{
			var dialog = new TileSetEditorDialogPrivateFieldsAccessor(__instance);
			__instance.Shown += (o, e) =>
			{
				if (dialog.m_TileSet.GetTileSetType() == null)
				{
					string tileSetName = "";
					if (RequestTileSetName(ref tileSetName))
					{
						f_m_TileSetName.SetValue(dialog.m_TileSet, tileSetName);
					}
				}
			};
			dialog.pnlButtons = new Panel();
			dialog.btnRemove = new Button();
			dialog.btnClear = new Button();
			dialog.btnAdd = new Button();
			dialog.btnOK = new Button();
			var btnSelectTileSet = new Button();
			dialog.lstTileSetPieces = new ListBox();
			dialog.txtFilter = new TextBox();
			dialog.pnlButtons.SuspendLayout();
			__instance.SuspendLayout();
			dialog.pnlButtons.Controls.Add(dialog.btnRemove);
			dialog.pnlButtons.Controls.Add(dialog.btnClear);
			dialog.pnlButtons.Controls.Add(dialog.btnAdd);
			dialog.pnlButtons.Controls.Add(dialog.btnOK);
			dialog.pnlButtons.Controls.Add(btnSelectTileSet);
			dialog.pnlButtons.Dock = DockStyle.Bottom;
			Point location = new Point(0, 230);
			dialog.pnlButtons.Location = location;
			dialog.pnlButtons.Name = "pnlButtons";
			Size size = new Size(541, 32);
			dialog.pnlButtons.Size = size;
			dialog.pnlButtons.TabIndex = 0;
			Point location2 = new Point(84, 6);
			dialog.btnRemove.Location = location2;
			dialog.btnRemove.Name = "btnRemove";
			Size size2 = new Size(75, 23);
			dialog.btnRemove.Size = size2;
			dialog.btnRemove.TabIndex = 4;
			dialog.btnRemove.Text = "Remove";
			dialog.btnRemove.UseVisualStyleBackColor = true;
			dialog.btnRemove.Click += dialog.btnRemove_Click;
			Point location3 = new Point(165, 6);
			dialog.btnClear.Location = location3;
			dialog.btnClear.Name = "btnClear";
			Size size3 = new Size(75, 23);
			dialog.btnClear.Size = size3;
			dialog.btnClear.TabIndex = 3;
			dialog.btnClear.Text = "Clear";
			dialog.btnClear.UseVisualStyleBackColor = true;
			dialog.btnClear.Click += dialog.btnClear_Click;
			Point location4 = new Point(3, 6);
			dialog.btnAdd.Location = location4;
			dialog.btnAdd.Name = "btnAdd";
			Size size4 = new Size(75, 23);
			dialog.btnAdd.Size = size4;
			dialog.btnAdd.TabIndex = 2;
			dialog.btnAdd.Text = "Add...";
			dialog.btnAdd.UseVisualStyleBackColor = true;
			dialog.btnAdd.Click += dialog.btnAdd_Click;
			Point location5 = new Point(246, 6);
			btnSelectTileSet.Location = location5;
			btnSelectTileSet.Name = "btnSelectTileSet";
			Size size5 = new Size(75, 23);
			btnSelectTileSet.Size = size5;
			btnSelectTileSet.TabIndex = 4;
			btnSelectTileSet.Text = "Tile Set";
			btnSelectTileSet.UseVisualStyleBackColor = true;
			btnSelectTileSet.Click += (object sender, EventArgs eventArgs) =>
			{
				string tileSetName = "";
				if (dialog.m_TileSet.GetTileSetType() != null)
				{
					tileSetName = dialog.m_TileSet.GetTileSetType();
				}
				if (RequestTileSetName(ref tileSetName))
				{
					f_m_TileSetName.SetValue(dialog.m_TileSet, tileSetName);
				}
			};
			Point location6 = new Point(454, 6);
			dialog.btnOK.Location = location6;
			dialog.btnOK.Name = "btnOK";
			Size size6 = new Size(75, 23);
			dialog.btnOK.Size = size6;
			dialog.btnOK.TabIndex = 0;
			dialog.btnOK.Text = "OK";
			dialog.btnOK.UseVisualStyleBackColor = true;
			dialog.btnOK.Click += dialog.btnOk_Click;
			dialog.lstTileSetPieces.Dock = DockStyle.Fill;
			dialog.lstTileSetPieces.FormattingEnabled = true;
			Point location7 = new Point(0, 20);
			dialog.lstTileSetPieces.Location = location7;
			dialog.lstTileSetPieces.Name = "lstTileSetPieces";
			dialog.lstTileSetPieces.SelectionMode = SelectionMode.MultiExtended;
			Size size7 = new Size(541, 210);
			dialog.lstTileSetPieces.Size = size7;
			dialog.lstTileSetPieces.Sorted = true;
			dialog.lstTileSetPieces.TabIndex = 1;
			dialog.lstTileSetPieces.SelectedIndexChanged += dialog.lstTileSetPieces_SelectedIndexChanged;
			dialog.txtFilter.Dock = DockStyle.Top;
			Point location8 = new Point(0, 0);
			dialog.txtFilter.Location = location8;
			dialog.txtFilter.Name = "txtFilter";
			Size size8 = new Size(541, 20);
			dialog.txtFilter.Size = size8;
			dialog.txtFilter.TabIndex = 2;
			dialog.txtFilter.TextChanged += dialog.txtFilter_TextChanged;
			__instance.AcceptButton = dialog.btnOK;
			SizeF autoScaleDimensions = new SizeF(6f, 13f);
			__instance.AutoScaleDimensions = autoScaleDimensions;
			__instance.AutoScaleMode = AutoScaleMode.Font;
			Size clientSize = new Size(541, 262);
			__instance.ClientSize = clientSize;
			__instance.ControlBox = false;
			__instance.Controls.Add(dialog.lstTileSetPieces);
			__instance.Controls.Add(dialog.pnlButtons);
			__instance.Controls.Add(dialog.txtFilter);
			__instance.MaximizeBox = false;
			__instance.MinimizeBox = false;
			__instance.Name = "TileSetEditorDialog";
			__instance.ShowIcon = false;
			__instance.ShowInTaskbar = false;
			__instance.Text = "Choose TileSetPieces";
			dialog.pnlButtons.ResumeLayout(false);
			__instance.ResumeLayout(false);
			__instance.PerformLayout();
			return false;
		}
	}
}
