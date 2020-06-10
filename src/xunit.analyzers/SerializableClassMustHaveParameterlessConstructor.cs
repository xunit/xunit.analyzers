﻿using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SerializableClassMustHaveParameterlessConstructor : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
		{
			context.RegisterSyntaxNodeAction(context =>
			{
				var classDeclaration = (ClassDeclarationSyntax)context.Node;
				if (classDeclaration.BaseList == null)
					return;

				var semanticModel = context.SemanticModel;

				foreach (var baseType in classDeclaration.BaseList.Types)
				{
					var type = semanticModel.GetTypeInfo(baseType.Type, context.CancellationToken).Type;
					if (xunitContext.Abstractions.IXunitSerializableType?.IsAssignableFrom(type) == true)
					{
						if (!classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().Any())
							return;

						var parameterlessCtor = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);
						if (parameterlessCtor == null || !parameterlessCtor.Modifiers.Any(m => m.Text == "public"))
							context.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor,
									classDeclaration.Identifier.GetLocation(),
									classDeclaration.Identifier.ValueText));

						return;
					}
				}
			}, SyntaxKind.ClassDeclaration);
		}

		protected override bool ShouldAnalyze(XunitContext xunitContext)
			=> xunitContext.HasAbstractionsReference;
	}
}
