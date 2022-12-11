using LSFrameworkPlugin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeaderTweaks.Util
{
    public enum ResourceClipboardOutputType
    {
        GUID,
        NameGUID,
        TypeNameGUID
    }

    public static class ResourceTools
    {
        public static string GetOutput(Resource res, ResourceClipboardOutputType outputType)
        {
            switch (outputType)
            {
                case ResourceClipboardOutputType.TypeNameGUID:
                    return $"{res.TypeId.ToUpperInvariant().Replace(" ", "_")}_{res.Name}_{res.GUID}";
                case ResourceClipboardOutputType.NameGUID:
                    return $"{res.Name}_{res.GUID}";
                case ResourceClipboardOutputType.GUID:
                default:
                    return res.GUID.ToString();
            }
        }

        public static string GetOutput(LSToolFramework.Entity res, ResourceClipboardOutputType outputType)
        {
            switch (outputType)
            {
                case ResourceClipboardOutputType.TypeNameGUID:
                    return $"{res.TypeId.ToUpperInvariant().Replace(" ", "_")}_{res.Name}_{res.GUID}";
                case ResourceClipboardOutputType.NameGUID:
                    return $"{res.Name}_{res.GUID}";
                case ResourceClipboardOutputType.GUID:
                default:
                    return res.GUID.ToString();
            }
        }

        public static string GetOutput(LSToolFramework.EditableObject res, ResourceClipboardOutputType outputType)
        {
            switch (outputType)
            {
                case ResourceClipboardOutputType.TypeNameGUID:
                    return $"{res.TypeId.ToUpperInvariant().Replace(" ", "_")}_{res.Name}_{res.GUID}";
                case ResourceClipboardOutputType.NameGUID:
                    return $"{res.Name}_{res.GUID}";
                case ResourceClipboardOutputType.GUID:
                default:
                    return res.GUID.ToString();
            }
        }

        public static List<T> GetSelectedItems<T>(ListView lv) where T : ListViewItem
        {
            List<T> items = new List<T>();
            var count = lv.SelectedIndices.Count;
            for (int i = 0; i < count; i++)
            {
                int index = lv.SelectedIndices[i];
                if (lv.Items[index] is T item)
                {
                    items.Add(item);
                }
            }
            return items;
        }
    }
}
