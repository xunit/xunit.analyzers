using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2014_UseAlternateAssert";

	public AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer() :
		base(Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (method is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.MethodName, out var methodName))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Use Assert.{0}", replacement),
				cancellationToken => UseAsyncAssertion(context.Document, invocation, replacement, cancellationToken),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseAsyncAssertion(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess && semanticModel is not null)
		{
			var asyncAssertionInvocation = GetAsyncAssertionInvocation(invocation, memberAccess, replacement);
			editor.ReplaceNode(invocation, asyncAssertionInvocation);

			var parentFunctionFixer = GetFunctionFixer(GetParentFunction(invocation), semanticModel, editor);
			IFunctionFixer? childFunctionFixer = null;

			while (ShouldFixParentFunction(parentFunctionFixer, childFunctionFixer, cancellationToken))
			{
				await parentFunctionFixer.Fix(cancellationToken).ConfigureAwait(false);

				childFunctionFixer = parentFunctionFixer;
				parentFunctionFixer = GetFunctionFixer(childFunctionFixer.GetParentFunction(), semanticModel, editor);
			}
		}

		return editor.GetChangedDocument();
	}

	static ExpressionSyntax GetAsyncAssertionInvocation(
		InvocationExpressionSyntax invocation,
		MemberAccessExpressionSyntax memberAccess,
		string replacement)
	{
		var asyncAssertionInvocation = invocation
			.WithExpression(memberAccess.WithName(GetAsyncAssertionMethodName(memberAccess, replacement)))
			.WithArgumentList(invocation.ArgumentList);

		if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
			return asyncAssertionInvocation;

		return AwaitExpression(asyncAssertionInvocation.WithoutLeadingTrivia())
			.WithLeadingTrivia(invocation.GetLeadingTrivia());
	}

	static SimpleNameSyntax GetAsyncAssertionMethodName(
		MemberAccessExpressionSyntax memberAccess,
		string replacement)
	{
		if (memberAccess.Name is not GenericNameSyntax genericNameSyntax)
			return IdentifierName(replacement);

		return GenericName(IdentifierName(replacement).Identifier, genericNameSyntax.TypeArgumentList);
	}

	static SyntaxNode? GetParentFunction(InvocationExpressionSyntax invocation)
	{
		return invocation.Parent?.FirstAncestorOrSelf<SyntaxNode>(IsFunction);
	}

	static bool IsFunction(SyntaxNode node)
	{
		return node switch
		{
			AnonymousFunctionExpressionSyntax => true,
			LocalFunctionStatementSyntax => true,
			MethodDeclarationSyntax => true,
			_ => false
		};
	}

	static IFunctionFixer? GetFunctionFixer(
		SyntaxNode? node,
		SemanticModel semanticModel,
		DocumentEditor editor)
	{
		return node switch
		{
			AnonymousFunctionExpressionSyntax anonymousFunction => new AnonymousFunctionFixer(anonymousFunction, semanticModel, editor),
			LocalFunctionStatementSyntax localFunction => new LocalFunctionFixer(localFunction, semanticModel, editor),
			MethodDeclarationSyntax method => new MethodFixer(method, editor),
			_ => null
		};
	}

	static bool ShouldFixParentFunction(
		[NotNullWhen(true)] IFunctionFixer? parentFunctionFixer,
		IFunctionFixer? childFunctionFixer,
		CancellationToken cancellationToken)
	{
		// 3. Parent function is null, and child function is outer method.
		// Outer method was just fixed, so should stop fixing now.
		if (parentFunctionFixer is null)
			return false;

		// 1. Parent function is innermost function, and child function is null.
		// Should fix innermost function unconditionally.
		if (childFunctionFixer is null)
			return true;

		// 2. Parent function and child function are not null.
		// Should fix parent function if and only if child function is invoked inside parent function, and otherwise stop fixing.
		return childFunctionFixer.ShouldFixParentFunction(parentFunctionFixer.Function, cancellationToken);
	}

	interface IFunctionFixer
	{
		SyntaxNode Function { get; }
		Task Fix(CancellationToken cancellationToken);
		SyntaxNode? GetParentFunction();
		bool ShouldFixParentFunction(SyntaxNode parentFunction, CancellationToken cancellationToken);
	}

	sealed class AnonymousFunctionFixer : IFunctionFixer
	{
		public SyntaxNode Function => anonymousFunction;

		readonly AnonymousFunctionExpressionSyntax anonymousFunction;
		readonly SemanticModel semanticModel;
		readonly DocumentEditor editor;

		public AnonymousFunctionFixer(
			AnonymousFunctionExpressionSyntax anonymousFunction,
			SemanticModel semanticModel,
			DocumentEditor editor)
		{
			this.anonymousFunction = anonymousFunction;
			this.semanticModel = semanticModel;
			this.editor = editor;
		}

		public async Task Fix(CancellationToken cancellationToken)
		{
			var modifiers = AsyncHelper.GetModifiersWithAsyncKeywordAdded(anonymousFunction.Modifiers);

			editor.ReplaceNode(anonymousFunction, (node, generator) =>
			{
				if (node is AnonymousFunctionExpressionSyntax anonymousFunction)
					return anonymousFunction.WithModifiers(modifiers);

				return node;
			});

			var declaration = await GetLocalDeclaration(cancellationToken).ConfigureAwait(false);
			if (declaration is null)
				return;

			var delegateType = await AsyncHelper.GetAsyncSystemDelegateType(declaration, anonymousFunction, editor, cancellationToken).ConfigureAwait(false);
			if (delegateType is null)
				return;

			editor.ReplaceNode(declaration, (node, generator) =>
			{
				if (node is VariableDeclarationSyntax declaration)
					return declaration
						.WithType(delegateType)
						.WithLeadingTrivia(declaration.GetLeadingTrivia());

				return node;
			});
		}

		async Task<VariableDeclarationSyntax?> GetLocalDeclaration(CancellationToken cancellationToken)
		{
			if (anonymousFunction.Parent is EqualsValueClauseSyntax clause
				&& clause.Parent is VariableDeclaratorSyntax declarator
				&& declarator.Parent is VariableDeclarationSyntax declaration)
			{
				return declaration;
			}

			var operation = semanticModel.GetOperation(anonymousFunction, cancellationToken) as IAnonymousFunctionOperation;
			return await GetLocalDeclaration(operation, cancellationToken).ConfigureAwait(false);
		}

		static async Task<VariableDeclarationSyntax?> GetLocalDeclaration(
			IAnonymousFunctionOperation? operation,
			CancellationToken cancellationToken)
		{
			if (operation?.Parent is not IDelegateCreationOperation delegateCreation)
				return null;

			if (delegateCreation.Parent is IVariableInitializerOperation initializer
				&& initializer.Parent is IVariableDeclaratorOperation declarator
				&& declarator.Parent is IVariableDeclarationOperation declaration)
			{
				return declaration.Syntax as VariableDeclarationSyntax;
			}

			if (delegateCreation.Parent is IAssignmentOperation assignment
				&& assignment.Target is ILocalReferenceOperation localReference)
			{
				var declaratorReference = localReference.Local.DeclaringSyntaxReferences.SingleOrDefault();
				if (declaratorReference is null)
					return null;

				var node = await declaratorReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
				if (node is VariableDeclaratorSyntax declaratorSyntax)
					return declaratorSyntax.Parent as VariableDeclarationSyntax;
			}

			return null;
		}

		public SyntaxNode? GetParentFunction()
		{
			return anonymousFunction.Parent?.FirstAncestorOrSelf<SyntaxNode>(IsFunction);
		}

		public bool ShouldFixParentFunction(SyntaxNode parentFunction, CancellationToken cancellationToken)
		{
			var symbol = GetLocalDeclarationSymbol(cancellationToken);
			if (symbol is null)
				return false;

			var invocations = parentFunction
				.DescendantNodes()
				.Where(node => node is InvocationExpressionSyntax)
				.Select(node => semanticModel.GetOperation((InvocationExpressionSyntax)node, cancellationToken) as IInvocationOperation)
				.Where(invocation => invocation is not null
					&& invocation.TargetMethod.MethodKind == MethodKind.DelegateInvoke
					&& invocation.Instance is ILocalReferenceOperation localReference
					&& SymbolEqualityComparer.Default.Equals(localReference.Local, symbol));

			return invocations.Any();
		}

		ILocalSymbol? GetLocalDeclarationSymbol(CancellationToken cancellationToken)
		{
			if (semanticModel.GetOperation(anonymousFunction, cancellationToken) is IAnonymousFunctionOperation operation
				&& operation.Parent is IDelegateCreationOperation delegateCreation)
			{
				if (delegateCreation.Parent is IVariableInitializerOperation initializer
					&& initializer.Parent is IVariableDeclaratorOperation declarator)
				{
					return declarator.Symbol;
				}

				if (delegateCreation.Parent is IAssignmentOperation assignment
					&& assignment.Target is ILocalReferenceOperation localReference)
				{
					return localReference.Local;
				}
			}

			return null;
		}
	}

	sealed class LocalFunctionFixer : IFunctionFixer
	{
		public SyntaxNode Function => localFunction;

		readonly LocalFunctionStatementSyntax localFunction;
		readonly SemanticModel semanticModel;
		readonly DocumentEditor editor;

		public LocalFunctionFixer(
			LocalFunctionStatementSyntax localFunction,
			SemanticModel semanticModel,
			DocumentEditor editor)
		{
			this.localFunction = localFunction;
			this.semanticModel = semanticModel;
			this.editor = editor;
		}

		public async Task Fix(CancellationToken cancellationToken)
		{
			var returnType = await AsyncHelper.GetAsyncReturnType(localFunction.ReturnType, editor, cancellationToken).ConfigureAwait(false);
			if (returnType is null)
				return;

			var modifiers = AsyncHelper.GetModifiersWithAsyncKeywordAdded(localFunction.Modifiers);

			editor.ReplaceNode(localFunction, (node, generator) =>
			{
				if (node is LocalFunctionStatementSyntax localFunction)
					return localFunction
						.WithModifiers(modifiers)
						.WithReturnType(returnType);

				return node;
			});
		}

		public SyntaxNode? GetParentFunction()
		{
			return localFunction.Parent?.FirstAncestorOrSelf<SyntaxNode>(IsFunction);
		}

		public bool ShouldFixParentFunction(SyntaxNode parentFunction, CancellationToken cancellationToken)
		{
			if (semanticModel.GetOperation(localFunction, cancellationToken) is not ILocalFunctionOperation operation)
				return false;

			var symbol = operation.Symbol;
			if (symbol is null)
				return false;

			return parentFunction
				.DescendantNodes()
				.Where(node => node is InvocationExpressionSyntax)
				.Select(node => semanticModel.GetOperation((InvocationExpressionSyntax)node, cancellationToken) as IInvocationOperation)
				.Where(invocation => SymbolEqualityComparer.Default.Equals(invocation?.TargetMethod, symbol))
				.Any();
		}
	}

	sealed class MethodFixer : IFunctionFixer
	{
		public SyntaxNode Function => method;

		readonly MethodDeclarationSyntax method;
		readonly DocumentEditor editor;

		public MethodFixer(MethodDeclarationSyntax method, DocumentEditor editor)
		{
			this.method = method;
			this.editor = editor;
		}

		public async Task Fix(CancellationToken cancellationToken)
		{
			var returnType = await AsyncHelper.GetAsyncReturnType(method.ReturnType, editor, cancellationToken).ConfigureAwait(false);
			if (returnType is null)
				return;

			var modifiers = AsyncHelper.GetModifiersWithAsyncKeywordAdded(method.Modifiers);

			editor.ReplaceNode(method, (node, generator) =>
			{
				if (node is MethodDeclarationSyntax method)
					return method
						.WithModifiers(modifiers)
						.WithReturnType(returnType);

				return node;
			});
		}

		public SyntaxNode? GetParentFunction()
		{
			return null;
		}

		public bool ShouldFixParentFunction(SyntaxNode parentFunction, CancellationToken cancellationToken)
		{
			return false;
		}
	}
}
