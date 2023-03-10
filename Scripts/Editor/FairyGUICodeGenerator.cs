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
using FairyGUI.Utils;
using GameFramework.FairyGUI.Runtime;

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
            var className = form.PackageName.ToTitleCase().UppercaseFirst();

            GenerateBindingCode(settings,
                form,
                dictComponents,
                typeof(FairyGUIFormLogic),
                MemberAttributes.Private,
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ContentPane"),
                settings.uiFormCodeNamespace,
                settings.uiFormCodeExportRoot,
                form.PackageName,
                className,
                settings.uiBindingCodeFileSuffix);

            var onInitMethod = new CodeMemberMethod
            {
                Name = nameof(FairyGUIFormLogic.OnInit),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(FairyGUIForm), "uiForm"),
                    new CodeParameterDeclarationExpression(typeof(object), "userData")
                },
                Statements =
                {
                    new CodeMethodInvokeExpression(
                        new CodeBaseReferenceExpression(),
                        nameof(FairyGUIFormLogic.OnInit),
                        new CodeVariableReferenceExpression("uiForm"),
                        new CodeVariableReferenceExpression("userData")
                    )
                }
            };

            GenerateLogicCode(settings,
                onInitMethod,
                settings.uiFormCodeNamespace,
                settings.uiFormCodeExportRoot,
                form.PackageName,
                className);
        }

        private static void GenerateUIComponentCode(
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            FairyGUIComponentCollector.UIComponent component,
            Dictionary<string, FairyGUIComponentCollector.UIComponent> dictComponents)
        {
            var className = component.Name.ToTitleCase().UppercaseFirst();

            GenerateBindingCode(settings,
                component,
                dictComponents,
                component.ExtensionType,
                MemberAttributes.Public,
                new CodeThisReferenceExpression(),
                settings.uiComponentCodeNamespace,
                settings.uiComponentCodeExportRoot,
                component.PackageName,
                className,
                settings.uiBindingCodeFileSuffix);

            var onInitMethod = new CodeMemberMethod
            {
                Name = nameof(GComponent.ConstructFromXML),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(XML), "xml"),
                }
            };

            GenerateLogicCode(settings,
                onInitMethod,
                settings.uiComponentCodeNamespace,
                settings.uiComponentCodeExportRoot,
                component.PackageName,
                className);
        }

        private static void GenerateBindingCode(
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            FairyGUIComponentCollector.UIComponent component,
            Dictionary<string, FairyGUIComponentCollector.UIComponent> dictComponents,
            Type baseType, MemberAttributes memberAttributes, CodeExpression contentPaneReferenceExpression,
            string namespaceStr, string root, string packageName, string name, string fileNameSuffix)
        {
            var declaration = new CodeTypeDeclaration
            {
                Name = name,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                IsPartial = true,
                IsClass = true,
                BaseTypes = { new CodeTypeReference(baseType) }
            };

            var bindingMethod = new CodeMemberMethod
            {
                Name = settings.uiBindingMethodName,
                Attributes = MemberAttributes.Private,
            };

            AddNodeBindings(declaration, bindingMethod, settings, component.Nodes, dictComponents, memberAttributes, contentPaneReferenceExpression);
            AddTransitionBindings(declaration, bindingMethod, settings, component.Transitions, memberAttributes, contentPaneReferenceExpression);
            AddControllerBindings(declaration, bindingMethod, settings, component.Controllers, memberAttributes, contentPaneReferenceExpression);

            declaration.Members.Add(bindingMethod);

            GenerateCodeFile(declaration, namespaceStr, root, packageName, name, fileNameSuffix, true);
        }

        private static void GenerateLogicCode(
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            CodeMemberMethod method,
            string namespaceStr, string root, string packageName, string name)
        {
            var path = FormatFilePath(root, packageName, name, string.Empty);
            if (File.Exists(path))
                return;

            // logic code
            var declaration = new CodeTypeDeclaration
            {
                Name = name,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                IsPartial = true,
                IsClass = true,
            };

            method.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), settings.uiBindingMethodName)));
            declaration.Members.Add(method);
            GenerateCodeFile(declaration, namespaceStr, root, packageName, name, string.Empty, false);
        }

        private static void AddNodeBindings(CodeTypeDeclaration declaration,
            CodeMemberMethod bindingMethod,
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            List<FairyGUIComponentCollector.UIComponentNode> nodes,
            Dictionary<string, FairyGUIComponentCollector.UIComponent> dictComponents,
            MemberAttributes memberAttributes,
            CodeExpression contentPaneReferenceExpression)
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
                var fieldName = memberAttributes.HasFlag(MemberAttributes.Public) ? node.Name.ToTitleCase().UppercaseFirst() : $"m_{node.Name.ToTitleCase().UppercaseFirst()}";

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
                var expressionGetChild = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(contentPaneReferenceExpression, nameof(GComponent.GetChild)), new CodePrimitiveExpression(node.Name));
                // (T)GetChild(name)
                var expressionCast = new CodeCastExpression(typeReference, expressionGetChild);
                // field = (T)GetChild(name)
                var expressionAssign = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), expressionCast);

                bindingMethod.Statements.Add(expressionAssign);

                #endregion
            }
        }

        private static void AddTransitionBindings(CodeTypeDeclaration declaration,
            CodeMemberMethod bindingMethod,
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            List<FairyGUIComponentCollector.UITransition> transitions,
            MemberAttributes memberAttributes,
            CodeExpression contentPaneReferenceExpression)
        {
            for (var index = 0; index < transitions.Count; index++)
            {
                var transition = transitions[index];

                var typeReference = new CodeTypeReference(typeof(Transition));
                var fieldName = memberAttributes.HasFlag(MemberAttributes.Public) ? transition.Name.ToTitleCase().UppercaseFirst() : $"m_{transition.Name.ToTitleCase().UppercaseFirst()}";
                if (!string.IsNullOrEmpty(settings.uiTransitionCodeExportNameSuffix) && !fieldName.EndsWith(settings.uiTransitionCodeExportNameSuffix))
                    fieldName += settings.uiTransitionCodeExportNameSuffix;

                // field
                declaration.Members.Add(new CodeMemberField(typeReference, fieldName)
                {
                    Attributes = memberAttributes
                });

                #region [binding method]

                // GetTransition(name)
                var expressionGetChild = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(contentPaneReferenceExpression, nameof(GComponent.GetTransition)), new CodePrimitiveExpression(transition.Name));
                // field = GetTransition(name)
                var expressionAssign = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), expressionGetChild);

                bindingMethod.Statements.Add(expressionAssign);

                #endregion
            }
        }

        private static void AddControllerBindings(CodeTypeDeclaration declaration,
            CodeMemberMethod bindingMethod,
            FairyGUIEditorSettings.FairyGUIExportSettings settings,
            List<FairyGUIComponentCollector.UIController> controllers,
            MemberAttributes memberAttributes,
            CodeExpression contentPaneReferenceExpression)
        {
            for (var index = 0; index < controllers.Count; index++)
            {
                var controller = controllers[index];

                var typeReference = new CodeTypeReference(typeof(Controller));
                var fieldName = memberAttributes.HasFlag(MemberAttributes.Public) ? controller.Name.ToTitleCase().UppercaseFirst() : $"m_{controller.Name.ToTitleCase().UppercaseFirst()}";
                if (!string.IsNullOrEmpty(settings.uiControllerCodeExportNameSuffix) && !fieldName.EndsWith(settings.uiControllerCodeExportNameSuffix))
                    fieldName += settings.uiControllerCodeExportNameSuffix;

                // field
                declaration.Members.Add(new CodeMemberField(typeReference, fieldName)
                {
                    Attributes = memberAttributes
                });

                #region [binding method]

                // GetController(name)
                var expressionGetChild = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(contentPaneReferenceExpression, nameof(GComponent.GetController)), new CodePrimitiveExpression(controller.Name));
                // field = GetController(name)
                var expressionAssign = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), expressionGetChild);

                bindingMethod.Statements.Add(expressionAssign);

                #endregion

                #region [controller page enum]

                if (!string.IsNullOrEmpty(settings.uiControllerEnumNameSuffix))
                {
                    var enumName = controller.Name.ToTitleCase().UppercaseFirst() + settings.uiControllerEnumNameSuffix;
                    var enumDeclaration = new CodeTypeDeclaration
                    {
                        Name = enumName,
                        TypeAttributes = memberAttributes.HasFlag(MemberAttributes.Public) ? TypeAttributes.Public : TypeAttributes.NestedPrivate,
                        IsEnum = true,
                    };

                    enumDeclaration.BaseTypes.Add(typeof(int));
                    for (var i = 0; i < controller.Pages.Count; i++)
                    {
                        var defaultPageName = $"{controller.Name.ToTitleCase().UppercaseFirst()}_{i}";

                        var pageName = controller.Pages[i];

                        // 分页名为空的话 使用默认名称
                        if (string.IsNullOrEmpty(pageName))
                            pageName = defaultPageName;
                        // 分页名首个字符必须为字母
                        else if (!Regex.IsMatch(pageName, @"^[a-zA-Z]"))
                            pageName = defaultPageName;
                        // 分页名包含中文
                        else if (Regex.IsMatch(pageName, @"[\u4e00-\u9fa5]"))
                            pageName = defaultPageName;
                        else
                            pageName = FilterFieldName(pageName).ToTitleCase().UppercaseFirst();

                        var enumMember = new CodeMemberField(enumName, pageName)
                        {
                            InitExpression = new CodePrimitiveExpression(i),
                            Comments =
                            {
                                new CodeCommentStatement("<summary>", true),
                                new CodeCommentStatement(pageName, true),
                                new CodeCommentStatement("</summary>", true),
                            }
                        };

                        enumDeclaration.Members.Add(enumMember);
                    }

                    declaration.Members.Add(enumDeclaration);

                    // enum property
                    var property = new CodeMemberProperty
                    {
                        Name = (memberAttributes.HasFlag(MemberAttributes.Public) ? enumName.UppercaseFirst() : enumName.LowercaseFirst()) + "Page",
                        Type = new CodeTypeReference(enumName),
                        HasGet = true,
                        HasSet = true,
                        Attributes = memberAttributes | MemberAttributes.Final,
                        GetStatements =
                        {
                            // return (PageEnum)controller.selectedIndex;
                            new CodeMethodReturnStatement(
                                new CodeCastExpression(enumName,
                                    new CodePropertyReferenceExpression(
                                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName),
                                        nameof(Controller.selectedIndex))))
                        },
                        SetStatements =
                        {
                            // controller.selectedIndex = (int)value;
                            new CodeAssignStatement(
                                new CodePropertyReferenceExpression(
                                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName),
                                    nameof(Controller.selectedIndex)),
                                new CodeCastExpression(typeof(int), new CodePropertySetValueReferenceExpression()))
                        }
                    };

                    declaration.Members.Add(property);
                }

                #endregion
            }
        }

        private static string GenerateCodeText(CodeCompileUnit codeCompileUnit, bool keepAutoGenerateComment)
        {
            var stringBuilder = new StringBuilder();
            using TextWriter textWriter = new StringWriter(stringBuilder);
            var provider = CodeDomProvider.CreateProvider("csharp");
            var options = new CodeGeneratorOptions { BracingStyle = "C" };
            provider.GenerateCodeFromCompileUnit(codeCompileUnit, textWriter, options);

            var text = stringBuilder.ToString();
            if (!keepAutoGenerateComment)
                text = text.RemoveLines(10);
            text = text.Replace("\r", string.Empty);
            return text;
        }

        private static void GenerateCodeFile(CodeCompileUnit codeCompileUnit, string root, string packageName, string name, string fileNameSuffix, bool keepAutoGenerateComment)
        {
            var codeText = GenerateCodeText(codeCompileUnit, keepAutoGenerateComment);
            var path = FormatFilePath(root, packageName, name, fileNameSuffix);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, codeText, Encoding.UTF8);
        }

        private static void GenerateCodeFile(CodeTypeDeclaration declaration, string namespaceStr, string root, string packageName, string name, string fileNameSuffix, bool keepAutoGenerateComment)
        {
            var codeNamespace = new CodeNamespace(FormatNamespace(namespaceStr, packageName));
            codeNamespace.Types.Add(declaration);

            var codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(codeNamespace);

            GenerateCodeFile(codeCompileUnit, root, packageName, name, fileNameSuffix, keepAutoGenerateComment);
        }

        private static string FormatNamespace(string namespaceStr, string packageName)
        {
            return !string.IsNullOrEmpty(namespaceStr) ? namespaceStr.Replace(FairyGUIEditorSettings.FairyGUIExportSettings.StrReplaceKeyPackageName, packageName) : string.Empty;
        }

        private static string FormatTypeName(string namespaceStr, string packageName, string name)
        {
            name = name.ToTitleCase().UppercaseFirst();
            if (string.IsNullOrEmpty(namespaceStr))
                return name;

            namespaceStr = FormatNamespace(namespaceStr, packageName);
            return $"{namespaceStr}.{name}";
        }

        private static string FormatFilePath(string root, string packageName, string name, string fileNameSuffix)
        {
            name = name.ToTitleCase().UppercaseFirst();
            if (!string.IsNullOrEmpty(fileNameSuffix))
                name += fileNameSuffix;
            name += ".cs";

            if (string.IsNullOrEmpty(root))
                return name;

            root = root.Replace(FairyGUIEditorSettings.FairyGUIExportSettings.StrReplaceKeyPackageName, packageName);
            return $"{root}/{name}";
        }

        private static string FilterFieldName(string fieldName)
        {
            return Regex.Replace(fieldName, "[^\\w]", "_");
        }
    }
}