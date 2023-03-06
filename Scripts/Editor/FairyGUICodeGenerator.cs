using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FairyGUI;

namespace GameFramework.FairyGUI.Editor
{
    /// <summary>
    /// FairyGUI代码生成器
    /// </summary>
    public static class FairyGUICodeGenerator
    {
        /// <summary>
        /// 生成代码
        /// </summary>
        public static void Generate(FairyGUIEditorSettings.FairyGUIExportSettings settings)
        {
            var components = FairyGUIComponentCollector.Collect(settings.runtimeSettings.uiAssetsRoot, settings.runtimeSettings.uiByteSuffix);

            var uiForms = new List<FairyGUIComponentCollector.UIComponent>();
            var exportComponents = new List<FairyGUIComponentCollector.UIComponent>();

            var regex = new Regex(settings.uiComponentNameRegex);
            foreach (var component in components)
            {
                if (string.Equals(component.Name, settings.runtimeSettings.uiFormComponentName))
                {
                    // UIForm
                    uiForms.Add(component);
                }
                else if (regex.IsMatch(component.Name))
                {
                    // Export Component
                    exportComponents.Add(component);
                }
            }

            var dictExportComponents = exportComponents.ToDictionary(item => item.Id);

            foreach (var form in uiForms)
                GenerateUIFormCode(settings, form, dictExportComponents);

            foreach (var component in exportComponents)
                GenerateUIComponentCode(settings, component, dictExportComponents);
        }

        private static void GenerateUIFormCode(
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            FairyGUIComponentCollector.UIComponent form,
            Dictionary<string, FairyGUIComponentCollector.UIComponent> dictComponents)
        {
            var declaration = new CodeTypeDeclaration
            {
                Name = form.Name.TitleCase(),
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                IsPartial = true,
                IsClass = true,
                BaseTypes = { new CodeTypeReference(settings.uiFormBaseTypeName) }
            };
            
            var bindingMethod = new CodeMemberMethod
            {
                Name = settings.uiBidingMethodName,
                Attributes = MemberAttributes.Private,
            };

            var expressionContentPane = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), settings.uiFormContentPanePropertyName);
            AddNodeBindings(declaration, bindingMethod, settings, form.Nodes, dictComponents, MemberAttributes.Private, expressionContentPane);
            
            declaration.Members.Add(bindingMethod);
            
            GenerateCodeFile(declaration, settings.uiFormCodeNamespace, settings.uiFormCodeExportRoot, form.PackageName, form.PackageName, settings.uiBindingCodeFileSuffix);
        }

        private static void GenerateUIComponentCode(
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            FairyGUIComponentCollector.UIComponent component,
            Dictionary<string, FairyGUIComponentCollector.UIComponent> dictComponents)
        {
            var declaration = new CodeTypeDeclaration
            {
                Name = component.Name.TitleCase(),
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                IsPartial = true,
                IsClass = true,
                BaseTypes = { new CodeTypeReference(component.ExtensionType) }
            };
            
            var bindingMethod = new CodeMemberMethod
            {
                Name = settings.uiBidingMethodName,
                Attributes = MemberAttributes.Private,
            };

            AddNodeBindings(declaration, bindingMethod, settings, component.Nodes, dictComponents, MemberAttributes.Public, new CodeThisReferenceExpression());

            declaration.Members.Add(bindingMethod);
            
            GenerateCodeFile(declaration, settings.uiComponentCodeNamespace, settings.uiComponentCodeExportRoot, component.PackageName, component.Name, settings.uiBindingCodeFileSuffix);
        }

        private static void AddNodeBindings(CodeTypeDeclaration declaration,
            CodeMemberMethod bindingMethod,
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            List<FairyGUIComponentCollector.UIComponentNode> nodes,
            Dictionary<string, FairyGUIComponentCollector.UIComponent> dictComponents,
            MemberAttributes memberAttributes,
            CodeExpression getChildTargetExpression)
        {
            var defaultNameRegex = new Regex(@"^n\d+$");

            for (var index = 0; index < nodes.Count; index++)
            {
                var node = nodes[index];
                if (node.ObjectType == ObjectType.Group)
                    continue;
                
                if (settings.ignoreDefaultNameChildren && defaultNameRegex.IsMatch(node.Name))
                    continue;

                CodeTypeReference typeReference;
                var fieldName = memberAttributes.HasFlag(MemberAttributes.Public) ? node.Name.TitleCase() : $"m_{node.Name.TitleCase()}";

                if (!string.IsNullOrEmpty(node.Ref) && dictComponents.TryGetValue(node.Ref, out var refComponent))
                {
                    // Export Component node
                    typeReference = new CodeTypeReference(FormatTypeName(settings.uiComponentCodeNamespace, refComponent.PackageName, refComponent.Name));
                }
                else
                {
                    // Base node
                    typeReference = node.ObjectType switch
                    {
                        ObjectType.Image => new CodeTypeReference(typeof(GImage)),
                        ObjectType.Graph => new CodeTypeReference(typeof(GGraph)),
                        ObjectType.Loader => new CodeTypeReference(typeof(GLoader)),
                        ObjectType.Text => new CodeTypeReference(typeof(GTextField)),
                        ObjectType.RichText => new CodeTypeReference(typeof(GRichTextField)),
                        ObjectType.InputText => new CodeTypeReference(typeof(GTextInput)),
                        ObjectType.Component => new CodeTypeReference(typeof(GComponent)),
                        ObjectType.List => new CodeTypeReference(typeof(GList)),
                        ObjectType.Label => new CodeTypeReference(typeof(GLabel)),
                        ObjectType.Button => new CodeTypeReference(typeof(GButton)),
                        ObjectType.ComboBox => new CodeTypeReference(typeof(GComboBox)),
                        ObjectType.ProgressBar => new CodeTypeReference(typeof(GProgressBar)),
                        ObjectType.Slider => new CodeTypeReference(typeof(GSlider)),
                        ObjectType.ScrollBar => new CodeTypeReference(typeof(GScrollBar)),
                        ObjectType.Tree => new CodeTypeReference(typeof(GTree)),
                        ObjectType.Loader3D => new CodeTypeReference(typeof(GLoader3D)),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                // field
                declaration.Members.Add(new CodeMemberField(typeReference, fieldName)
                {
                    Attributes = memberAttributes
                });

                #region [binding method]

                // GetChild(name)
                var expressionGetChild = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(getChildTargetExpression, nameof(GComponent.GetChild)), new CodePrimitiveExpression(node.Name));
                // (T)GetChild(name)
                var expressionCast = new CodeCastExpression(typeReference, expressionGetChild);
                // field = (T)GetChild(name)
                var expressionAssign = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), expressionCast);

                bindingMethod.Statements.Add(expressionAssign);

                #endregion
            }
        }

        private static string GenerateCodeText(CodeCompileUnit codeCompileUnit)
        {
            var stringBuilder = new StringBuilder();
            using TextWriter textWriter = new StringWriter(stringBuilder);
            var provider = CodeDomProvider.CreateProvider("csharp");
            var options = new CodeGeneratorOptions { BracingStyle = "C" };
            provider.GenerateCodeFromCompileUnit(codeCompileUnit, textWriter, options);
            
            return stringBuilder.ToString();
        }

        private static void GenerateCodeFile(CodeCompileUnit codeCompileUnit, string root, string packageName, string name, string fileNameSuffix)
        {
            var codeText = GenerateCodeText(codeCompileUnit);
            var path = FormatFilePath(root, packageName, name, fileNameSuffix);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            File.WriteAllText(path, codeText, Encoding.UTF8);
        }

        private static void GenerateCodeFile(CodeTypeDeclaration declaration, string namespaceStr, string root, string packageName, string name, string fileNameSuffix)
        {
            var codeNamespace = new CodeNamespace(FormatNamespace(namespaceStr, packageName));
            codeNamespace.Types.Add(declaration);

            var codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(codeNamespace);
            
            GenerateCodeFile(codeCompileUnit, root, packageName, name, fileNameSuffix);
        }

        private static string FormatNamespace(string namespaceStr, string packageName)
        {
            return !string.IsNullOrEmpty(namespaceStr) ? namespaceStr.Replace(FairyGUIEditorSettings.FairyGUIExportSettings.StrReplaceKeyPackageName, packageName) : string.Empty;
        }

        private static string FormatTypeName(string namespaceStr, string packageName, string name)
        {
            name = name.TitleCase();
            if (string.IsNullOrEmpty(namespaceStr))
                return name;

            namespaceStr = FormatNamespace(namespaceStr, packageName);
            return $"{namespaceStr}.{name}";
        }

        private static string FormatFilePath(string root, string packageName, string name, string fileNameSuffix)
        {
            name = name.TitleCase();
            if (!string.IsNullOrEmpty(fileNameSuffix))
                name += fileNameSuffix;
            name += ".cs";

            if (string.IsNullOrEmpty(root))
                return name;
            
            root = root.Replace(FairyGUIEditorSettings.FairyGUIExportSettings.StrReplaceKeyPackageName, packageName);
            return $"{root}/{name}";
        }
    }
}