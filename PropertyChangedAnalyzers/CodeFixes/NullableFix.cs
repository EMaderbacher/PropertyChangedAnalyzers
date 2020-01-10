﻿namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullableFix))]
    [Shared]
    internal class NullableFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("CS8618");

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == "CS8618" &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax? member))
                {
                    if (FindEventType(member) is { } type)
                    {
                        context.RegisterCodeFix(
                            "Declare as nullable.",
                            (editor, _) => editor.ReplaceNode(
                                type,
                                x => SyntaxFactory.NullableType(x)),
                            "Declare event as nullable.",
                            diagnostic);
                    }

                    TypeSyntax? FindEventType(MemberDeclarationSyntax candidate)
                    {
                        return candidate switch
                        {
                            EventDeclarationSyntax { Type: { } t } => t,
                            EventFieldDeclarationSyntax { Declaration: { Type: { } t } } => t,
                            ConstructorDeclarationSyntax { Parent: TypeDeclarationSyntax typeDeclaration }
                            when Regex.Match(diagnostic.GetMessage(CultureInfo.InvariantCulture), "Non-nullable event '(?<name>[^']+)' is uninitialized") is { Success: true } match &&
                                 typeDeclaration.TryFindEvent(match.Groups["name"].Value, out member)
                            => FindEventType(member),
                            _ => null,
                        };
                    }
                }
            }
        }
    }
}
