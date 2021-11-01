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

namespace LeaderTweaks.Patches
{
	[LeaderPatcher]
	public class MessagePatcher : IPatcher
	{
		private static bool loaded = false;

		public void Init(Harmony patcher)
		{
			if (loaded) return;

			var pt = typeof(MessagePatcher);

			patcher.Patch(AccessTools.PropertyGetter(typeof(NotificationDataViewModel), nameof(NotificationDataViewModel.Color)), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(MessagePatcher.Color))));
			patcher.Patch(AccessTools.Method(typeof(MessageService), "AddMessage"), 
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(MessagePatcher.AddMessage))));

			loaded = true;
		}

		public static void Color(ref System.Windows.Media.Brush __result, MessageItem ___m_Data)
		{
			__result = ___m_Data.CategoryColor;
		}

		static readonly MethodInfo m_GetLastNoSystem = AccessTools.Method(typeof(MessageService), "GetLastNoSystem");
		static readonly FastInvokeHandler GetLastNoSystem = HarmonyLib.MethodInvoker.GetHandler(m_GetLastNoSystem);
		static readonly Regex OsirisAssertPattern = new Regex("(Osiris triggered an assert:)|(OSIRIS ASSERT:\\s+\\[Osiris\\])", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public static bool AddMessage(string msg, EMessageType type, EMessageCategory cat, string extrainfo, bool persistent, 
			MessageService __instance, bool ___m_Initialized, MessageService.MessageList ___m_Messages, NotificationManager ___m_NotificationManager,
			MessagePanel ___m_Panel)
		{
			if (!___m_Initialized)
			{
				return false;
			}
			msg = msg.TrimEnd(new char[]
			{
				'\n'
			});
			
			if(OsirisAssertPattern.IsMatch(msg))
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
