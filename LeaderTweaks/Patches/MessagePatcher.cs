using HarmonyLib;

using EoCPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using MessagePlugin;
using LSToolFramework;
using MessagePlugin.Notifications;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Globalization;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Message Log")]
	public class MessagePatcher : IPatcher
	{
		public static Dictionary<Regex, Brush> PatternColors = new Dictionary<Regex, Brush>();
		private static readonly Brush DesignWarningColor = Brushes.Orange;
		private static readonly Brush DesignErrorColor = Brushes.Red;
		//private static readonly Brush DesignWarningColor = Brushes.Orange;
		//private static readonly Brush DesignErrorColor = Brushes.Red;

		public void Init(Harmony patcher)
		{
			var rf = RegexOptions.IgnoreCase | RegexOptions.Multiline;
			PatternColors.Add(new Regex(@"^\s*\[Osiris\]", rf), Brushes.SpringGreen);
			PatternColors.Add(new Regex(@"^\s*\[LeaderTweaks\]", rf), Brushes.DeepSkyBlue);
			PatternColors.Add(new Regex(@"^\s*Failed"), Brushes.PaleVioletRed);

			var pt = typeof(MessagePatcher);

			//patcher.Patch(AccessTools.PropertyGetter(typeof(NotificationDataViewModel), nameof(NotificationDataViewModel.Color)),
			//	transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(MessagePatcher.t_GetTextColor2))));
			patcher.Patch(AccessTools.PropertyGetter(typeof(MessageItem), nameof(MessageItem.TextColor)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(MessagePatcher.t_GetTextColor))));
			patcher.Patch(AccessTools.Method(typeof(MessageService), "AddMessage"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(MessagePatcher.AddMessage))));
		}

		static Brush GetCategoryColor(EMessageCategory category)
		{
			switch (category)
			{
				case EMessageCategory.Code:
					return MessageItem.CodeColor;
				case EMessageCategory.Art:
					return MessageItem.ArtColor;
				case EMessageCategory.Design:
					return MessageItem.DesignColor;
				case EMessageCategory.Animation:
					return MessageItem.AnimColor;
				case EMessageCategory.Script:
					return MessageItem.ScriptColor;
				case EMessageCategory.Sound:
					return MessageItem.SoundColor;
				case EMessageCategory.Genome:
					return MessageItem.GenomeColor;
				default:
					return MessageItem.InfoColor;
			}
		}

		static Brush GetMessageColor(MessageItem item)
		{
			switch (item.Category)
			{
				case EMessageCategory.Design:
					switch (item.Type)
					{
						case EMessageType.Warning:
							return DesignWarningColor;
						case EMessageType.Error:
							return DesignErrorColor;
						default:
							return GetCategoryColor(item.Category);
					}
				default:
					switch (item.Type)
					{
						case EMessageType.Warning:
							return MessageItem.WarningColor;
						case EMessageType.Error:
							return MessageItem.ErrorColor;
						default:
							return GetCategoryColor(item.Category);
					}
			}
		}

		static readonly MethodInfo m_GetTextColor = AccessTools.Method(typeof(MessagePatcher), nameof(MessagePatcher.GetTextColor));

		public static IEnumerable<CodeInstruction> t_GetTextColor(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Call, m_GetTextColor);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		public static Brush GetTextColor(MessageItem messageItem)
		{
			if (messageItem == null)
			{
				return Brushes.White;
			}

			var specialColor = PatternColors.Where(r => r.Key.IsMatch(messageItem.Message)).Select(kvp => kvp.Value).FirstOrDefault();
			if (specialColor != null)
			{
				return specialColor;
			}
			else
			{
				return GetMessageColor(messageItem);
			}
		}

		static readonly MethodInfo m_GetLastNoSystem = AccessTools.Method(typeof(MessageService), "GetLastNoSystem");
		static readonly FastInvokeHandler GetLastNoSystem = HarmonyLib.MethodInvoker.GetHandler(m_GetLastNoSystem);
		static readonly RegexOptions defaultRO = RegexOptions.IgnoreCase | RegexOptions.Multiline;
		static readonly Regex OsirisAssertPattern = new Regex("(Osiris triggered an assert:)|(OSIRIS ASSERT:\\s+\\[Osiris\\])", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		static readonly List<Regex> IgnoreMessages = new List<Regex>()
		{
			new Regex("filename \".*\" does not exist, can't load mod!", defaultRO),
			new Regex("Failed to load (add-on|pak file|the pack file)", defaultRO),
			new Regex("Packed file corrupt", defaultRO),
			new Regex("Trying to open.*OsiToolsConfig", defaultRO),
			new Regex("No valid displayname for status", defaultRO),
			new Regex(@"\[eoc::GetStatusTranslatedName\]", defaultRO),
			new Regex("does not have a race", defaultRO),
			new Regex(".*networkfixedstrings.*", defaultRO),
			new Regex("Effect '.*?' cache overflow", defaultRO),
		};

		public static bool AddMessage(string msg, EMessageType type, EMessageCategory cat, string extrainfo, bool persistent,
			MessageService __instance, bool ___m_Initialized, MessageService.MessageList ___m_Messages, NotificationManager ___m_NotificationManager,
			MessagePanel ___m_Panel)
		{
			if (!___m_Initialized || IgnoreMessages.Any(r => r.IsMatch(msg)))
			{
				return false;
			}
			msg = msg.TrimEnd(new char[]
			{
				'\n'
			});

			if (OsirisAssertPattern.IsMatch(msg))
			{
				msg = OsirisAssertPattern.Replace(msg, "[Osiris]");
				cat = EMessageCategory.Script;
				type = EMessageType.Info;
			}

			___m_Messages.Lock();
			MessageItem messageItem = GetLastNoSystem.Invoke(__instance, null) as MessageItem;
			if (messageItem != null && messageItem.Message == msg)
			{
				MessageItem messageItem2 = messageItem;
				int count = messageItem2.Count + 1;
				messageItem2.Count = count;
			}
			else
			{
				messageItem = new MessageItem
				{
					Message = msg,
					Type = type,
					ExtraInfo = extrainfo,
					Category = cat
				};
				if (persistent)
				{
					___m_Messages.Add(messageItem);
				}
			}
			if (persistent)
			{
				___m_Panel.AddMessage(messageItem);
			}
			if (persistent && messageItem.Type != EMessageType.Error && messageItem.Type != EMessageType.Warning)
			{
				___m_Messages.Unlock();
				return false;
			}
			NotificationManager notificationManager = ___m_NotificationManager;
			if (notificationManager != null)
			{
				notificationManager.AddMessage(messageItem);
			}
			___m_Messages.Unlock();

			return false;
		}
	}
}
