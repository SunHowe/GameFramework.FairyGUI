using System;
using System.Collections.Generic;
using FairyGUI;

namespace GameFramework.FairyGUI.Editor
{
    /// <summary>
    /// FairyGUI组件收集器
    /// </summary>
    public static class FairyGUIComponentCollector
    {
        public static List<UIComponent> Collect(string uiAssetsRoot, string uiByteSuffix)
        {
            AddPackages(uiAssetsRoot, uiByteSuffix);

            var components = new List<UIComponent>();
            foreach (var package in UIPackage.GetPackages())
                components.AddRange(LoadFromPackage(package));

            UIPackage.RemoveAllPackages();
            return components;
        }

        private static List<UIComponent> LoadFromPackage(UIPackage package)
        {
            var components = new List<UIComponent>();

            foreach (var packageItem in package.GetItems())
            {
                if (packageItem.type != PackageItemType.Component)
                    continue;

                if (!packageItem.exported)
                    continue;

                components.Add(LoadFromPackageItem(packageItem));
            }

            return components;
        }

        private static UIComponent LoadFromPackageItem(PackageItem item)
        {
            return new UIComponent
            {
                Id = FormatId(item.owner.id, item.id),
                PackageName = item.owner.name,
                Name = item.name,
                Nodes = LoadComponentNodes(item),
                Controllers = LoadControllers(item),
                Transitions = LoadTransitions(item),
                ExtensionType = item.objectType switch
                {
                    ObjectType.Label => typeof(GLabel),
                    ObjectType.Button => typeof(GButton),
                    ObjectType.ComboBox => typeof(GComboBox),
                    ObjectType.ProgressBar => typeof(GProgressBar),
                    ObjectType.Slider => typeof(GSlider),
                    ObjectType.ScrollBar => typeof(GScrollBar),

                    ObjectType.Component => typeof(GComponent),

                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }

        private static List<UIComponentNode> LoadComponentNodes(PackageItem item)
        {
            var nodes = new List<UIComponentNode>();

            var buffer = item.rawData;
            buffer.Seek(0, 2);

            int childCount = buffer.ReadShort();
            for (var i = 0; i < childCount; i++)
            {
                int dataLen = buffer.ReadShort();
                var curPos = buffer.position;

                buffer.Seek(curPos, 0);

                var objectType = (ObjectType)buffer.ReadByte();
                var src = buffer.ReadS();
                var pkgId = buffer.ReadS() ?? item.owner.id;

                buffer.Seek(curPos, 0);
                buffer.Skip(5);

                buffer.ReadS();
                var name = buffer.ReadS();
                buffer.position = curPos + dataLen;

                nodes.Add(new UIComponentNode
                {
                    Name = name,
                    ObjectType = objectType,
                    Ref = !string.IsNullOrEmpty(src) ? FormatId(pkgId, src) : string.Empty,
                });
            }

            return nodes;
        }

        private static List<UIController> LoadControllers(PackageItem item)
        {
            var controllers = new List<UIController>();
            var buffer = item.rawData;

            buffer.Seek(0, 1);

            int controllerCount = buffer.ReadShort();
            for (var i = 0; i < controllerCount; i++)
            {
                int nextPos = buffer.ReadShort();
                nextPos += buffer.position;

                var beginPos = buffer.position;
                buffer.Seek(beginPos, 0);

                var name = buffer.ReadS();
                var pages = new List<string>();

                buffer.Seek(beginPos, 1);

                var pageCount = buffer.ReadShort();
                for (var j = 0; j < pageCount; j++)
                {
                    buffer.ReadS(); // pageId
                    var packageName = buffer.ReadS();

                    pages.Add(packageName);
                }

                buffer.position = nextPos;

                controllers.Add(new UIController
                {
                    Name = name,
                    Pages = pages,
                });
            }

            return controllers;
        }

        private static List<UITransition> LoadTransitions(PackageItem item)
        {
            var transitions = new List<UITransition>();

            var buffer = item.rawData;
            buffer.Seek(0, 5);

            int transitionCount = buffer.ReadShort();
            for (var i = 0; i < transitionCount; i++)
            {
                int nextPos = buffer.ReadShort();
                nextPos += buffer.position;

                var name = buffer.ReadS();

                buffer.position = nextPos;

                transitions.Add(new UITransition { Name = name });
            }

            return transitions;
        }

        private static void AddPackages(string uiAssetsRoot, string uiByteSuffix)
        {
            UIPackage.RemoveAllPackages();

            if (string.IsNullOrEmpty(uiAssetsRoot))
                return;

            var names = FairyGUIUtils.GetUIPackageFileNames(uiAssetsRoot, uiByteSuffix);
            foreach (var name in names)
                UIPackage.AddPackage(uiAssetsRoot + "/" + name);
        }

        private static string FormatId(string packageId, string objectId)
        {
            return $"{packageId}.{objectId}";
        }

        /// <summary>
        /// UI组件
        /// </summary>
        public class UIComponent
        {
            public string Id { get; set; }
            public string PackageName { get; set; }
            public string Name { get; set; }
            public Type ExtensionType { get; set; }
            public List<UIComponentNode> Nodes { get; set; }
            public List<UIController> Controllers { get; set; }
            public List<UITransition> Transitions { get; set; }
        }

        /// <summary>
        /// UI组件的子节点
        /// </summary>
        public class UIComponentNode
        {
            public string Name { get; set; }
            public ObjectType ObjectType { get; set; }
            public string Ref { get; set; }
        }

        /// <summary>
        /// UI控制器
        /// </summary>
        public class UIController
        {
            public string Name { get; set; }
            public List<string> Pages { get; set; }
        }

        /// <summary>
        /// UI动效
        /// </summary>
        public class UITransition
        {
            public string Name { get; set; }
        }
    }
}