using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers.Utilities
{
    internal struct RequireTypesContext
    {
        private readonly List<Action<CompilationStartAnalysisContext>> _registrations;

        internal RequireTypesContext(AnalysisContext context, string[] types)
        {
            _registrations = new List<Action<CompilationStartAnalysisContext>>();

            // Since we're a struct, capture _registrations in a local variable to use it in the closure.
            var registrations = _registrations;

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                foreach (var type in types)
                {
                    if (compilation.GetTypeByMetadataName(type) == null)
                        return;
                }

                foreach (var registration in registrations)
                {
                    registration(compilationStartContext);
                }
            });
        }

        public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
        {
            _registrations.Add(context => context.RegisterSymbolAction(action, symbolKinds));
        }

        public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds)
            where TLanguageKindEnum : struct
        {
            _registrations.Add(context => context.RegisterSyntaxNodeAction(action, syntaxKinds));
        }
    }
}
