using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class InlineDataMustMatchTheoryParametersFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues.Id,
            Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType.Id,
            Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue.Id,
            Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter.Id
            );

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var diagnostic = context.Diagnostics.Single();
            var diagnosticId = diagnostic.Id;
            var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (diagnosticId == Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues.Id)
            {
                Enum.TryParse(diagnostic.Properties[InlineDataMustMatchTheoryParameters.ParameterArrayStyle], out InlineDataMustMatchTheoryParameters.ParameterArrayStyleType arrayStyle);
                context.RegisterCodeFix(CodeAction.Create("Add Default Values", ct => AddDefaultValuesAsync(context.Document, (AttributeSyntax)node, method, arrayStyle, ct), "AddDefaultValues"), context.Diagnostics);
            }
            else if (diagnosticId == Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType.Id)
            {
                // TODO
            }
            else if (diagnosticId == Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue.Id)
            {
                context.RegisterCodeFix(CodeAction.Create("Remove Value", ct => Actions.RemoveNodeAsync(context.Document, node, ct), "Remove Value"), context.Diagnostics);

                var parameterIndex = int.Parse(diagnostic.Properties[InlineDataMustMatchTheoryParameters.ParameterIndex]);
                if (method.ParameterList.Parameters.Count == parameterIndex)
                    context.RegisterCodeFix(CodeAction.Create("Add Theory Parameter", ct => AddTheoryParameterAsync(context.Document, method, ct), "Add Theory Parameter"), context.Diagnostics);
            }
            else if (diagnosticId == Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter.Id)
            {
                var parameterIndex = int.Parse(diagnostic.Properties[InlineDataMustMatchTheoryParameters.ParameterIndex]);
                context.RegisterCodeFix(CodeAction.Create("Make Parameter Nullable", ct => MakeParameterNullableAsync(context.Document, method, parameterIndex, ct), "Make Parameter Nullable"), context.Diagnostics);
            }
        }

        private async Task<Document> AddDefaultValuesAsync(Document document, AttributeSyntax attribute, MethodDeclarationSyntax method, InlineDataMustMatchTheoryParameters.ParameterArrayStyleType arrayStyle, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            InitializerExpressionSyntax arrayInitializer = null;
            if (arrayStyle == InlineDataMustMatchTheoryParameters.ParameterArrayStyleType.Initializer)
                arrayInitializer = (InitializerExpressionSyntax)attribute.DescendantNodes().First(n => n.IsKind(SyntaxKind.ArrayInitializerExpression));

            var originalInitializer = arrayInitializer;
            int i = originalInitializer?.Expressions.Count ?? attribute.ArgumentList?.Arguments.Count ?? 0;
            for (; i < method.ParameterList.Parameters.Count; i++)
            {
                var defaultExpression = (ExpressionSyntax)CreateDefaultValueSyntax(editor, method.ParameterList.Parameters[i].Type);
                if (arrayInitializer != null)
                {
                    arrayInitializer = arrayInitializer.AddExpressions(defaultExpression);
                }
                else
                    editor.AddAttributeArgument(attribute, defaultExpression);
            }
            if (arrayInitializer != null)
                editor.ReplaceNode(originalInitializer, arrayInitializer);

            return editor.GetChangedDocument();
        }

        private SyntaxNode CreateDefaultValueSyntax(DocumentEditor editor, TypeSyntax type)
        {
            var t = editor.SemanticModel.GetTypeInfo(type).Type;
            switch (t.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return editor.Generator.FalseLiteralExpression();
                case SpecialType.System_Char:
                    return editor.Generator.LiteralExpression((char)0);
                case SpecialType.System_Double:
                    return editor.Generator.LiteralExpression((double)0);
                case SpecialType.System_Single:
                    return editor.Generator.LiteralExpression((float)0);
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                    return editor.Generator.LiteralExpression((uint)0);
                case SpecialType.System_Byte:
                case SpecialType.System_Decimal:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_SByte:
                case SpecialType.System_UInt16:
                    return editor.Generator.LiteralExpression(0);
                case SpecialType.System_String:
                    return editor.Generator.LiteralExpression("");
            }

            if (t.TypeKind == TypeKind.Enum)
                return editor.Generator.DefaultExpression(t);

            return editor.Generator.NullLiteralExpression();
        }

        private async Task<Document> MakeParameterNullableAsync(Document document, MethodDeclarationSyntax method, int parameterIndex, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var param = method.ParameterList.Parameters[parameterIndex];
            var semanticModel = editor.SemanticModel;
            var nullableT = semanticModel.Compilation.GetSpecialType(SpecialType.System_Nullable_T);
            var nullable = nullableT.Construct(semanticModel.GetTypeInfo(param.Type, cancellationToken).Type);
            editor.SetType(param, editor.Generator.TypeExpression(nullable));
            return editor.GetChangedDocument();
        }

        private async Task<Document> AddTheoryParameterAsync(Document document, MethodDeclarationSyntax method, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            // TODO be better at guessing parameter type
            editor.AddParameter(method, editor.Generator.ParameterDeclaration("value", editor.Generator.TypeExpression(SpecialType.System_Object)));
            return editor.GetChangedDocument();
        }
    }
}
