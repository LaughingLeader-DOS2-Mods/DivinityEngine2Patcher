using HarmonyLib;

using LSFrameworkPlugin;

using LSToolFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Resource Fixes", "Resources")]
	public class ResourcePatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(ResourcePatcher);
			var rt = typeof(Resource);
			
			harmony.Patch(AccessTools.PropertyGetter(typeof(PhysicsResource), nameof(PhysicsResource.ImageIndex)), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.ImageIndex))));

			harmony.Patch(AccessTools.PropertyGetter(rt, nameof(Resource.Valid)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.Valid))));

			harmony.Patch(AccessTools.Method(rt, nameof(Resource.GetSingleFileName), new Type[] { }),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.GetSingleFileName))));

			harmony.Patch(AccessTools.PropertyGetter(typeof(EditableObject), nameof(EditableObject.Inherited)), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.GetInherited))));

			harmony.Patch(AccessTools.Method(typeof(AnimationsDialog), "OnFindingResourceReferences"), 
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.OnFindingResourceReferences))));
		}

		//Fix for physics resources crashing upon previewing, if no level is loaded
		public static void Valid(ref bool __result, Resource __instance, bool ___m_Valid)
		{
			if (___m_Valid && String.IsNullOrEmpty(ToolFramework.Instance?.LevelDataPath) &&  __instance is PhysicsResource)
			{
				__result = false;
			} 
		}

		//Making sure the above result doesn't change the resource icon
		public static void ImageIndex(ref int __result, PhysicsResource __instance, bool ___m_Valid)
		{
			if (!___m_Valid)
			{
				__result = 0;
			}
			__result = 9;
		}

		//Fix local resources always being "inherited" (cannot delete), such as AnimationResource
		public static void GetInherited(ref bool __result, EditableObject __instance, bool ___m_Inherited)
		{
			if (___m_Inherited && !String.IsNullOrEmpty(__instance.FileName))
			{
				var contentPath = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Content";
				if (__instance.FileName.IsSubPathOf(contentPath))
				{
					__result = false;
				}
			}
		}

		/*
		 * Fixes null reference exception when deleting visual/animation resources
		 * Issue: Pane incorrectly assumes that it has a bound visual resource 
		 * Code @ `AnimationsDialog.OnFindingResourceReferences` (replace method body)
		 * Source: Norbyte
		 */
		public static bool OnFindingResourceReferences(IResource resource, ref List<IReference> out_Feedback, 
			AnimationsDialog __instance, VisualResource ___m_VisualResource, Dictionary<string, List<MAnimationDesc>> ___m_Animations)
		{
			AnimationResource animationResource = resource as AnimationResource;
			if (animationResource != null)
			{
				string b = animationResource.GUID.ToString();
				foreach (KeyValuePair<string, List<MAnimationDesc>> keyValuePair in ___m_Animations)
				{
					List<MAnimationDesc>.Enumerator enumerator2 = keyValuePair.Value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.ResourceID == b)
						{
							out_Feedback.Add(new FormReference(__instance));
							break;
						}
					}
				}
				//Disables the original method
				return false;
			}
			VisualResource visualResource = resource as VisualResource;
			if (visualResource != null && ___m_VisualResource != null && visualResource.GUID == ___m_VisualResource.GUID)
			{
				out_Feedback.Add(new FormReference(__instance));
			}
			//Disables the original method
			return false;
		}

		//Fixes resources always being saved as GUID.lsf, ignoring their current filename.
		public static void GetSingleFileName(ref string __result, Resource __instance)
		{
			if (!String.IsNullOrWhiteSpace(__instance.FileName))
			{
				if (!Path.GetExtension(__instance.FileName).Equals(".lsf", StringComparison.OrdinalIgnoreCase))
				{
					__instance.FileName = Path.ChangeExtension(__instance.FileName, ".lsf");
				}
				__result = __instance.FileName;
			}
		}
	}

	[LeaderPatcher("Resource Name Tweaks", "ResourceNaming")]
	public class ResourceNamePatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(ResourceNamePatcher);

			harmony.Patch(AccessTools.Method(typeof(ResourceBank), nameof(ResourceBank.Save)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourceNamePatcher.OnResourceBankSave))));
		}

		//Making new resources be named Name_UUID.lsf by default
		public static void OnResourceBankSave(ResourceBank __instance, Dictionary<System.Guid, Resource> ___m_Resources)
		{
			try
			{
				foreach (var resource in ___m_Resources.Values)
				{
					//New files only
					if (resource.Dirty && !File.Exists(resource.FileName) && Path.GetExtension(resource.FileName).Equals(".lsf", StringComparison.OrdinalIgnoreCase) && resource.Name.IsUUID4())
					{
						var resourceDir = Directory.GetParent(resource.FileName).FullName;
						resource.FileName = Path.Combine(resourceDir, $"{resource.Name}_{resource.GUID}.lsf");
						Helper.Log($"Renaming new resource to '{resource.FileName}' (Name_GUID), for sanity.");
					}
				}
			}
			catch(Exception ex)
			{
				Helper.Log($"Error renaming new resources:\n'{ex}");
			}
		}
	}
}
