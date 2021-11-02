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
	[LeaderPatcher("Resource Fixes")]
	public class ResourcePatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(ResourcePatcher);
			
			harmony.Patch(AccessTools.PropertyGetter(typeof(PhysicsResource), nameof(PhysicsResource.ImageIndex)), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.ImageIndex))));

			var t1 = typeof(Resource);
			harmony.Patch(AccessTools.PropertyGetter(t1, nameof(Resource.Valid)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.Valid))));
			harmony.Patch(AccessTools.Method(t1, nameof(Resource.GetSingleFileName), new Type[] { }), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.GetSingleFileName))));
			harmony.Patch(AccessTools.PropertyGetter(typeof(EditableObject), nameof(EditableObject.Inherited)), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.GetInherited))));

			harmony.Patch(AccessTools.Method(typeof(ResourceBank), nameof(ResourceBank.Save)), 
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.OnResourceBankSave))));

			harmony.Patch(AccessTools.Method(typeof(AnimationsDialog), "OnFindingResourceReferences"), 
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(ResourcePatcher.OnFindingResourceReferences))));
		}

		public static void GetSingleFileName(ref string __result, Resource __instance)
		{
			if (!String.IsNullOrWhiteSpace(__instance.FileName))
			{
				if(!__instance.FileName.EndsWith(".lsf", StringComparison.OrdinalIgnoreCase))
				{
					__instance.FileName = Path.ChangeExtension(__instance.FileName, ".lsf");
				}
				__result = __instance.FileName;
			}
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
			if (___m_Inherited)
			{
				var contentPath = ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Public\\" + ToolFramework.Instance.ModFolder + "\\Content";
				if (!String.IsNullOrEmpty(__instance.FileName) && __instance.FileName.IsSubPathOf(contentPath))
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

		//Making new resources be named Name_UUID.lsf by default
		public static void OnResourceBankSave(ResourceBank __instance, Dictionary<System.Guid, Resource> ___m_Resources)
		{
			foreach(var resource in ___m_Resources.Values)
			{
				//New files only
				if(resource.Dirty && !File.Exists(resource.FileName))
				{
					var resourceDir = Directory.GetParent(resource.FileName).FullName;
					resource.FileName = Path.Combine(resourceDir, $"{resource.Name}_{resource.GUID}.lsf");
					Helper.Log($"Renaming new resource to '{resource.FileName}' (Name_GUID), for sanity.");
				}
			}
		}
	}
}
