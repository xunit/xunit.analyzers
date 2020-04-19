using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MemberDataShouldReferenceValidMember : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(
				Descriptors.X1014_MemberDataShouldUseNameOfOperator,
				Descriptors.X1015_MemberDataMustReferenceExistingMember,
				Descriptors.X1016_MemberDataMustReferencePublicMember,
				Descriptors.X1017_MemberDataMustReferenceStaticMember,
				Descriptors.X1018_MemberDataMustReferenceValidMemberKind,
				Descriptors.X1019_MemberDataMustReferenceMemberOfValidType,
				Descriptors.X1020_MemberDataPropertyMustHaveGetter,
				Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
		{
			var compilation = compilationStartContext.Compilation;

			var iEnumerableOfObjectArrayType = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);

			var supportsNameofOperator =
				compilation is CSharpCompilation cSharpCompilation
				&& cSharpCompilation.LanguageVersion >= LanguageVersion.CSharp6;

			compilationStartContext.RegisterSyntaxNodeAction(symbolContext =>
			{
				var attribute = (AttributeSyntax)symbolContext.Node;
				var semanticModel = symbolContext.SemanticModel;
				if (!Equals(semanticModel.GetTypeInfo(attribute, symbolContext.CancellationToken).Type, xunitContext.Core.MemberDataAttributeType))
					return;

				var memberNameArgument = attribute.ArgumentList.Arguments.FirstOrDefault();
				if (memberNameArgument == null)
					return;

				var propertyAttributeParameters = attribute.ArgumentList.Arguments
					.Count(a => !string.IsNullOrEmpty(a.NameEquals?.Name.Identifier.ValueText));

				var paramsCount = attribute.ArgumentList.Arguments.Count - 1 - propertyAttributeParameters;
				var constantValue = semanticModel.GetConstantValue(memberNameArgument.Expression, symbolContext.CancellationToken);
				if (!(constantValue.Value is string memberName))
					return;

				var memberTypeArgument = attribute.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText == "MemberType");
				ITypeSymbol memberTypeSymbol = null;
				if (memberTypeArgument?.Expression is TypeOfExpressionSyntax typeofExpression)
				{
					var typeSyntax = typeofExpression.Type;
					memberTypeSymbol = semanticModel.GetTypeInfo(typeSyntax, symbolContext.CancellationToken).Type;
				}

				var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>());
				var declaredMemberTypeSymbol = memberTypeSymbol ?? testClassTypeSymbol;
				var memberSymbol = FindMemberSymbol(memberName, declaredMemberTypeSymbol, paramsCount);

				if (memberSymbol == null)
				{
					symbolContext.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1015_MemberDataMustReferenceExistingMember,
							attribute.GetLocation(),
							memberName,
							SymbolDisplay.ToDisplayString(declaredMemberTypeSymbol)));
				}
				else
				{
					if (memberSymbol.Kind != SymbolKind.Field &&
						memberSymbol.Kind != SymbolKind.Property &&
						memberSymbol.Kind != SymbolKind.Method)
					{
						symbolContext.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.X1018_MemberDataMustReferenceValidMemberKind,
								attribute.GetLocation()));
					}
					else
					{
						if (supportsNameofOperator && memberNameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
						{
							var builder = ImmutableDictionary.CreateBuilder<string, string>();
							if (!Equals(memberSymbol.ContainingType, testClassTypeSymbol))
								builder.Add("DeclaringType", memberSymbol.ContainingType.ToDisplayString());

							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1014_MemberDataShouldUseNameOfOperator,
									memberNameArgument.Expression.GetLocation(),
									builder.ToImmutable(),
									memberName,
									memberSymbol.ContainingType.ToDisplayString()));
						}

						var memberProperties = new Dictionary<string, string>
						{
							{ "DeclaringType", declaredMemberTypeSymbol.ToDisplayString() },
							{ "MemberName", memberName }
						}.ToImmutableDictionary();

						if (memberSymbol.DeclaredAccessibility != Accessibility.Public)
						{
							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1016_MemberDataMustReferencePublicMember,
									attribute.GetLocation(),
									memberProperties));
						}
						if (!memberSymbol.IsStatic)
						{
							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1017_MemberDataMustReferenceStaticMember,
									attribute.GetLocation(),
									memberProperties));
						}
						var memberType = GetMemberType(memberSymbol);
						if (!iEnumerableOfObjectArrayType.IsAssignableFrom(memberType))
						{
							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1019_MemberDataMustReferenceMemberOfValidType,
									attribute.GetLocation(),
									memberProperties,
									SymbolDisplay.ToDisplayString(iEnumerableOfObjectArrayType),
									SymbolDisplay.ToDisplayString(memberType)));
						}
						if (memberSymbol.Kind == SymbolKind.Property && ((IPropertySymbol)memberSymbol).GetMethod == null)
						{
							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1020_MemberDataPropertyMustHaveGetter,
									attribute.GetLocation()));
						}
						var extraArguments = attribute.ArgumentList.Arguments.Skip(1).TakeWhile(a => a.NameEquals == null).ToList();
						if (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field)
						{
							if (extraArguments.Any())
							{
								var span = TextSpan.FromBounds(extraArguments.First().Span.Start, extraArguments.Last().Span.End);
								symbolContext.ReportDiagnostic(
									Diagnostic.Create(
										Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters,
										Location.Create(attribute.SyntaxTree, span)));
							}
						}

						if (memberSymbol.Kind == SymbolKind.Method)
						{
							// TODO: handle method parameter type matching, model after InlineDataMustMatchTheoryParameter
						}
					}
				}
			}, SyntaxKind.Attribute);
		}

		static ITypeSymbol GetMemberType(ISymbol memberSymbol)
		{
			return memberSymbol switch
			{
				IPropertySymbol prop => prop.Type,
				IFieldSymbol field => field.Type,
				IMethodSymbol method => method.ReturnType,
				_ => null,
			};
		}

		static ISymbol FindMemberSymbol(string memberName, ITypeSymbol type, int paramsCount)
		{
			if (paramsCount > 0 && FindMethodSymbol(memberName, type, paramsCount) is ISymbol methodSymbol)
			{
				return methodSymbol;
			}

			while (type != null)
			{
				var memberSymbol = type.GetMembers(memberName).FirstOrDefault();

				if (memberSymbol != null)
					return memberSymbol;

				type = type.BaseType;
			}

			return null;
		}

		static ISymbol FindMethodSymbol(string memberName, ITypeSymbol type, int paramsCount)
		{
			while (type != null)
			{
				var methodSymbol = type.GetMembers(memberName)
					.OfType<IMethodSymbol>()
					.FirstOrDefault(x => x.Parameters.Length == paramsCount);

				if (methodSymbol != null)
				{
					return methodSymbol;
				}

				type = type.BaseType;
			}

			return null;
		}
	}
}
