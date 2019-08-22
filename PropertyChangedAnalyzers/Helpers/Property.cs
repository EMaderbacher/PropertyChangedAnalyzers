namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Property
    {
        internal static bool TryGetContainingProperty(ISymbol symbol, out IPropertySymbol property)
        {
            if (symbol is IMethodSymbol method &&
                method.AssociatedSymbol is ISymbol associated)
            {
                property = associated as IPropertySymbol;
            }
            else
            {
                property = symbol.ContainingSymbol as IPropertySymbol;
            }

            return property != null;
        }

        internal static bool IsLazy(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (propertyDeclaration.TryGetSetter(out _))
            {
                return false;
            }

            if (propertyDeclaration.TryGetGetter(out var getter))
            {
                if (getter.Body == null &&
                    getter.ExpressionBody == null)
                {
                    return false;
                }

                using (var walker = ReturnExpressionsWalker.Borrow(getter))
                {
                    foreach (var returnValue in walker.ReturnValues)
                    {
                        if (IsCoalesceAssign(returnValue))
                        {
                            return true;
                        }

                        if (semanticModel.TryGetSymbol(returnValue, cancellationToken, out IFieldSymbol returnedField) &&
                            AssignmentExecutionWalker.FirstFor(returnedField, getter, SearchScope.Instance, semanticModel, cancellationToken, out _))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return propertyDeclaration.ExpressionBody is ArrowExpressionClauseSyntax arrow &&
                   IsCoalesceAssign(arrow.Expression);

            bool IsCoalesceAssign(ExpressionSyntax expression)
            {
                return expression is BinaryExpressionSyntax binary &&
                       binary.IsKind(SyntaxKind.CoalesceExpression) &&
                       semanticModel.TryGetSymbol(binary.Left, cancellationToken, out IFieldSymbol coalesceField) &&
                       AssignmentExecutionWalker.FirstFor(coalesceField, binary.Right, SearchScope.Instance, semanticModel, cancellationToken, out _);
            }
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (declaration == null ||
                declaration.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.ProtectedKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.AbstractKeyword) ||
                !declaration.TryGetSetter(out _) ||
                declaration.ExpressionBody != null ||
                IsAutoPropertyNeverAssignedOutsideConstructor(declaration))
            {
                return false;
            }

            return semanticModel.TryGetSymbol(declaration, cancellationToken, out var property) &&
                   ShouldNotify(declaration, property, semanticModel, cancellationToken);
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property.IsIndexer ||
                property.DeclaredAccessibility == Accessibility.Private ||
                property.DeclaredAccessibility == Accessibility.Protected ||
                property.IsStatic ||
                property.IsReadOnly ||
                property.GetMethod == null ||
                property.IsAbstract ||
                property.ContainingType == null ||
                property.ContainingType.IsValueType ||
                property.ContainingType.DeclaredAccessibility == Accessibility.Private ||
                property.ContainingType.DeclaredAccessibility == Accessibility.Protected ||
                IsAutoPropertyNeverAssignedOutsideConstructor(declaration))
            {
                return false;
            }

            if (IsMutableAutoProperty(declaration))
            {
                return true;
            }

            if (declaration.TryGetSetter(out var setter))
            {
                if (!AssignsValueToBackingField(setter, out var assignment))
                {
                    return false;
                }

                if (PropertyChanged.InvokesPropertyChangedFor(assignment, property, semanticModel, cancellationToken) != AnalysisResult.No)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property)
        {
            return IsMutableAutoProperty(property, out _, out _);
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property, out AccessorDeclarationSyntax getter, out AccessorDeclarationSyntax setter)
        {
            if (property.TryGetGetter(out getter) &&
                getter.Body == null &&
                getter.ExpressionBody == null &&
                property.TryGetSetter(out setter) &&
                setter.Body == null &&
                setter.ExpressionBody == null)
            {
                return true;
            }

            getter = null;
            setter = null;
            return false;
        }

        internal static bool TrySingleReturnedInGetter(PropertyDeclarationSyntax property, out ExpressionSyntax result)
        {
            result = null;
            if (property == null)
            {
                return false;
            }

            var expressionBody = property.ExpressionBody;
            if (expressionBody != null)
            {
                result = expressionBody.Expression;
                return result != null;
            }

            if (property.TryGetGetter(out var getter))
            {
                expressionBody = getter.ExpressionBody;
                if (expressionBody != null)
                {
                    result = expressionBody.Expression;
                    return result != null;
                }

                var body = getter.Body;
                if (body == null ||
                    body.Statements.Count == 0)
                {
                    return false;
                }

                if (body.Statements.TrySingle(out var statement) &&
                    statement is ReturnStatementSyntax returnStatement)
                {
                    result = returnStatement.Expression;
                    return result != null;
                }
            }

            return false;
        }

        internal static bool TryGetBackingFieldFromSetter(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (property == null)
            {
                return false;
            }

            return property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax propertyDeclaration) &&
                   TryGetBackingFieldFromSetter(propertyDeclaration, semanticModel, cancellationToken, out field);
        }

        internal static bool TryGetBackingFieldFromSetter(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (property == null)
            {
                return false;
            }

            if (property.TryGetSetter(out var setter))
            {
                if (TryFindSingleTrySet(setter, semanticModel, cancellationToken, out var invocation))
                {
                    return TryGetBackingField(invocation.ArgumentList.Arguments[0].Expression, semanticModel, cancellationToken, out field);
                }

                if (TrySingleAssignmentInSetter(setter, out var assignment))
                {
                    return TryGetBackingField(assignment.Left, semanticModel, cancellationToken, out field);
                }
            }

            return false;
        }

        internal static bool TryFindSingleTrySet(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax invocation)
        {
            invocation = null;
            if (setter == null)
            {
                return false;
            }

            using (var walker = InvocationWalker.Borrow(setter))
            {
                return walker.Invocations.TrySingle(x => TrySet.IsMatch(x, semanticModel, cancellationToken) != AnalysisResult.No, out invocation);
            }
        }

        internal static bool TrySingleAssignmentInSetter(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
        {
            assignment = null;
            if (setter == null)
            {
                return false;
            }

            using (var walker = AssignmentWalker.Borrow(setter))
            {
                if (walker.Assignments.TrySingle(out assignment) &&
                    assignment.Right is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.ValueText == "value")
                {
                    return true;
                }
            }

            assignment = null;
            return false;
        }

        internal static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
        {
            using (var walker = AssignmentWalker.Borrow(setter))
            {
                foreach (var a in walker.Assignments)
                {
                    if ((a.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
                    {
                        continue;
                    }

                    if (a.Left is IdentifierNameSyntax)
                    {
                        assignment = a;
                        return true;
                    }

                    if (a.Left is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is IdentifierNameSyntax)
                    {
                        if (memberAccess.Expression is ThisExpressionSyntax ||
                            memberAccess.Expression is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }

                        if (memberAccess.Expression is MemberAccessExpressionSyntax nested &&
                            nested.Expression is ThisExpressionSyntax &&
                            nested.Name is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }
                    }
                }
            }

            assignment = null;
            return false;
        }

        internal static bool TryFindValue(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out IParameterSymbol value)
        {
            using (var walker = IdentifierNameWalker.Borrow(setter))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (identifierName.Identifier.ValueText == "value" &&
                        semanticModel.TryGetSymbol(identifierName, cancellationToken, out value))
                    {
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        internal static bool TryGetAssignedProperty(AssignmentExpressionSyntax assignment, out PropertyDeclarationSyntax propertyDeclaration)
        {
            propertyDeclaration = null;
            var typeDeclaration = assignment?.FirstAncestor<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
            {
                return false;
            }

            if (assignment.Left is IdentifierNameSyntax identifierName)
            {
                return typeDeclaration.TryFindProperty(identifierName.Identifier.ValueText, out propertyDeclaration);
            }

            if (assignment.Left is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is ThisExpressionSyntax)
            {
                return typeDeclaration.TryFindProperty(memberAccess.Name.Identifier.ValueText, out propertyDeclaration);
            }

            return false;
        }

        internal static bool TryGetSingleAssignedWithParameter(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax fieldAccess)
        {
            using (var mutations = MutationWalker.Borrow(setter, SearchScope.Member, semanticModel, cancellationToken))
            {
                if (mutations.TrySingle(out var mutation))
                {
                    switch (mutation)
                    {
                        case AssignmentExpressionSyntax assignment:
                            fieldAccess = assignment.Left;
                            return assignment.Right is IdentifierNameSyntax identifierName &&
                                   identifierName.Identifier.ValueText == "value";
                        case ArgumentSyntax argument:
                            fieldAccess = null;
                            return argument.Parent is ArgumentListSyntax argumentList &&
                                   argumentList.Parent is InvocationExpressionSyntax invocation &&
                                   argumentList.Arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out var refArgument) &&
                                   (fieldAccess = refArgument.Expression) != null &&
                                   TrySet.IsMatch(invocation, semanticModel, cancellationToken) == AnalysisResult.Yes;
                        default:
                            fieldAccess = null;
                            return false;
                    }
                }
            }

            fieldAccess = null;
            return false;
        }

        internal static bool IsAutoPropertyNeverAssignedOutsideConstructor(this PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.ExpressionBody is null &&
                propertyDeclaration.TryGetGetter(out var getter) &&
                getter.ExpressionBody is null &&
                getter.Body is null &&
                propertyDeclaration.Parent is ClassDeclarationSyntax classDeclaration)
            {
                if (propertyDeclaration.TryGetSetter(out var setter))
                {
                    if (propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword) &&
                        !setter.Modifiers.Any(SyntaxKind.PrivateKeyword))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }

                var name = propertyDeclaration.Identifier.ValueText;
                using (var walker = IdentifierNameWalker.Borrow(classDeclaration))
                {
                    foreach (var identifierName in walker.IdentifierNames)
                    {
                        if (identifierName.Identifier.ValueText == name &&
                            IsAssigned(identifierName) &&
                            !IsInConstructor(identifierName))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;

            bool IsAssigned(IdentifierNameSyntax identifierName)
            {
                var parent = identifierName.Parent;
                if (parent is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is ThisExpressionSyntax)
                    {
                        parent = memberAccess.Parent;
                    }
                    else
                    {
                        return false;
                    }
                }

                switch (parent)
                {
                    case AssignmentExpressionSyntax a:
                        return a.Left.Contains(identifierName);
                    case PostfixUnaryExpressionSyntax _:
                        return true;
                    case PrefixUnaryExpressionSyntax p:
                        return !p.IsKind(SyntaxKind.LogicalNotExpression);
                    default:
                        return false;
                }
            }

            bool IsInConstructor(SyntaxNode node)
            {
                if (node.TryFirstAncestor(out ConstructorDeclarationSyntax ctor) &&
                    propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) == ctor.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    // Could be in an event handler in ctor.
                    return !node.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _);
                }

                return false;
            }
        }

        private static bool TryGetBackingField(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (candidate is IdentifierNameSyntax)
            {
                field = semanticModel.GetSymbolSafe(candidate, cancellationToken) as IFieldSymbol;
            }
            else if (candidate is MemberAccessExpressionSyntax memberAccess &&
                     memberAccess.Expression is ThisExpressionSyntax)
            {
                field = semanticModel.GetSymbolSafe(candidate, cancellationToken) as IFieldSymbol;
            }

            return field != null;
        }
    }
}
