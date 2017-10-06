namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC006UseObjectEqualsForReferenceTypes : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC006_b";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if value is different using object.Equals before notifying.",
            messageFormat: "Check if value is different using object.Equals before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.DisabledByDefault,
            description: "Check if value is different using object.Equals before notifying.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.IfStatement);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var ifStatement = (IfStatementSyntax)context.Node;
            if (ifStatement?.Condition == null)
            {
                return;
            }

            var setter = ifStatement.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return;
            }

            if (!Notifies(setter, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);

            if (property == null ||
                property.Type.IsValueType ||
                property.Type == KnownSymbol.String)
            {
                return;
            }

            if (!Property.TryGetBackingField(property, context.SemanticModel, context.CancellationToken, out IFieldSymbol backingField))
            {
                return;
            }

            if (Property.TryFindValue(setter, context.SemanticModel, context.CancellationToken, out IParameterSymbol value))
            {
                foreach (var member in new ISymbol[] { backingField, property })
                {
                    if (Equality.IsObjectEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member) ||
                        IsNegatedObjectEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member))
                    {
                        if (Equality.UsesObjectOrNone(ifStatement.Condition))
                        {
                            return;
                        }
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, ifStatement.GetLocation()));
        }

        private static bool Notifies(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var pooled = InvocationWalker.Create(setter))
            {
                foreach (var invocation in pooled.Item.Invocations)
                {
                    if (PropertyChanged.IsNotifyPropertyChanged(invocation, semanticModel, cancellationToken))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsNegatedObjectEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var unaryExpression = expression as PrefixUnaryExpressionSyntax;
            if (unaryExpression?.IsKind(SyntaxKind.LogicalNotExpression) == true)
            {
                return Equality.IsObjectEquals(unaryExpression.Operand, semanticModel, cancellationToken, value, member);
            }

            return false;
        }
    }
}