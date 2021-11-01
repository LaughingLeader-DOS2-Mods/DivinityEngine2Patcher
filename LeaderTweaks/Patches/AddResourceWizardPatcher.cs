
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HarmonyLib;

using LSFrameworkPlugin;

using LSToolFramework;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher]
	public class AddResourceWizardPatcher : IPatcher
	{
		static readonly MethodInfo m_ImportResource = AccessTools.Method(typeof(AddResourceWizard), "ImportResource");
		static readonly FastInvokeHandler ImportResource = HarmonyLib.MethodInvoker.GetHandler(m_ImportResource);
		static readonly MethodInfo m_AutoImportResource = AccessTools.Method(typeof(AddResourceWizard), "AutoImportResource");
		static readonly FastInvokeHandler AutoImportResource = HarmonyLib.MethodInvoker.GetHandler(m_AutoImportResource);
		static readonly FieldInfo f_m_CancelAll = AccessTools.Field(typeof(AddResourceWizard), "m_CancelAll");
		static readonly FieldInfo f_m_OkForAll = AccessTools.Field(typeof(AddResourceWizard), "m_OkForAll");

		static readonly MethodInfo m_BtnAddModelClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddModelClick");
		static readonly MethodInfo m_BtnAddAnimationClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddAnimationClick");
		static readonly MethodInfo m_BtnAddPhysicsClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddPhysicsClick");
		static readonly MethodInfo m_BtnAddTextureClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddTextureClick");
		static readonly MethodInfo m_BtnAddScriptClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddScriptClick");
		static readonly MethodInfo m_BtnAddMaterialClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddMaterialClick");
		static readonly MethodInfo m_BtnAddEffectClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddEffectClick");
		static readonly MethodInfo m_BtnAddSoundClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddSoundClick");
		static readonly MethodInfo m_BtnAddAnimationBlueprintClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddAnimationBlueprintClick");
		static readonly MethodInfo m_BtnAddMeshProxyClick = AccessTools.Method(typeof(AddResourceWizardPatcher), "BtnAddMeshProxyClick");

		public void Init(Harmony harmony)
		{
			var t1 = typeof(AddResourceWizard);
			var t2 = typeof(AddResourceWizardPatcher);

			string[] funcNames = new string[]
			{
				"BtnAddModelClick",
				"BtnAddAnimationClick",
				"BtnAddPhysicsClick",
				"BtnAddTextureClick",
				"BtnAddScriptClick",
				"BtnAddMaterialClick",
				"BtnAddEffectClick",
				"BtnAddSoundClick",
				"BtnAddAnimationBlueprintClick",
				"BtnAddMeshProxyClick",
			};

			foreach(var name in funcNames)
			{
				harmony.Patch(AccessTools.Method(t1, name), transpiler: new HarmonyMethod(AccessTools.Method(t2, $"t_{name}")));
			}
		}

		public static void BtnAddModelClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Model files (*.gr2;*.lsm)|*.gr2;*.lsm|Granny files (*.gr2)|*.gr2|Larian 3rd Party exporter (*.lsm)|*.lsm";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Models"))
			{
				text += "\\Models";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							//me.ImportResource((EResourceType)0U, fileName, new ImportResourceExtraData());
							ImportResource(me, 0U, fileName, new ImportResourceExtraData());
						}
						else
						{
							//me.AutoImportResource((EResourceType)0U, fileName, new ImportResourceExtraData());
							AutoImportResource(me, 0U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddAnimationClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Animation files (*.gr2;*.lsm)|*.gr2;*.lsm|Granny files (*.gr2)|*.gr2|Larian 3rd Party exporter (*.lsm)|*.lsm";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Animations"))
			{
				text += "\\Animations";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 2U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 2U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddPhysicsClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Physics files (*.bin;*.bullet;*.raw)|*.bin;*.bullet;*.raw";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Physics"))
			{
				text += "\\Physics";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 6U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 6U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddTextureClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Texture files (*.dds;*.tga)|*.dds;*.tga|DirectDraw Surface (*.dds)|*.dds|Uncompressed texture files (*.tga)|*.tga";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Textures"))
			{
				text += "\\Textures";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 4U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 4U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddScriptClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Script files|*.charScript;*.itemScript;*.gameScript|Character Script Files|*.charScript|Item Script Files|*.itemScript|Game Script Files|*.gameScript";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder;
			if (Directory.Exists(text + "\\Scripts"))
			{
				text += "\\Scripts";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 8U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 8U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddMaterialClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Larian Studios Material files (*.lsb)|*.lsb";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Materials"))
			{
				text += "\\Materials";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 5U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 5U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddEffectClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Compiled effect files (*.lsfx)|*.lsfx";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Effects\\Effects_Banks"))
			{
				text += "\\Effects\\Effects_Banks";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 7U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 7U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddSoundClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = "Sound files (*.bnk;*.wav)|*.bnk;*.wav|WWise Sound Bank (*.bnk)|*.bnk|Waveform (*.wav)|*.wav|Waveform PCM (*.wav)|*.wav|All files (*.*)|*.*";
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Sound"))
			{
				text += "\\Sound";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 9U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 9U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddAnimationBlueprintClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Animation blueprint files (*.lsabp)|*.lsabp";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Blueprints"))
			{
				text += "\\Blueprints";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						ImportResourceExtraData extraData = new ImportResourceExtraData();
						if (!m_OkForAll)
						{
							ImportResource(me, 11U, fileName, extraData);
						}
						else
						{
							AutoImportResource(me, 11U, fileName, extraData);
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		public static void BtnAddMeshProxyClick(AddResourceWizard me, object sender, EventArgs e)
		{
			var m_CancelAll = (bool)f_m_CancelAll.GetValue(me);
			var m_OkForAll = (bool)f_m_OkForAll.GetValue(me);

			me.Close();
			LSFrameworkPlugin.ResourceManager instance = LSFrameworkPlugin.ResourceManager.Instance;
			LSCSharpCore.IO.OpenFileDialog openFileDialog = new LSCSharpCore.IO.OpenFileDialog();
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "MeshProxy files (*.gr2;*.lsm)|*.gr2;*.lsm|Granny files (*.gr2)|*.gr2|Larian 3rd Party exporter (*.lsm)|*.lsm";
			openFileDialog.Multiselect = true;
			string text = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Assets";
			if (Directory.Exists(text + "\\Proxy"))
			{
				text += "\\Proxy";
			}
			else if (Directory.Exists(text + "\\MeshProxy"))
			{
				text += "\\MeshProxy";
			}
			LSCSharpCore.Path initialDirectory = text;
			openFileDialog.InitialDirectory = initialDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = openFileDialog.FileNames;
				int num = 0;
				if (fileNames.Length != 0)
				{
					do
					{
						string fileName = fileNames[num];
						if (m_CancelAll)
						{
							break;
						}
						if (!m_OkForAll)
						{
							ImportResource(me, 12U, fileName, new ImportResourceExtraData());
						}
						else
						{
							AutoImportResource(me, 12U, fileName, new ImportResourceExtraData());
						}
						num++;
					}
					while (num < fileNames.Length);
				}
				instance.Save();
			}
		}

		/*
		 "BtnAddModelClick",
		"BtnAddAnimationClick",
		"BtnAddPhysicsClick",
		"BtnAddTextureClick",
		"BtnAddScriptClick",
		"BtnAddMaterialClick",
		"BtnAddEffectClick",
		"BtnAddSoundClick",
		"BtnAddAnimationBlueprintClick",
		"BtnAddMeshProxyClick",
		 */

		public static IEnumerable<CodeInstruction> t_BtnAddModelClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddModelClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddAnimationClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddAnimationClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddPhysicsClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddPhysicsClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddTextureClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddTextureClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddScriptClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddScriptClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddMaterialClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddMaterialClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddEffectClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddEffectClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddSoundClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddSoundClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddAnimationBlueprintClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddAnimationBlueprintClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static IEnumerable<CodeInstruction> t_BtnAddMeshProxyClick(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldarg_1);
			yield return new CodeInstruction(OpCodes.Ldarg_2);
			yield return new CodeInstruction(OpCodes.Call, m_BtnAddMeshProxyClick);
			yield return new CodeInstruction(OpCodes.Ret);
		}
	}
}
