﻿namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpression
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class MvvmLight
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmLight;

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var before = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(↓() => this.P1);
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionInternalClassInternalProperty()
            {
                var before = @"
namespace N
{
    internal class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(↓() => this.P1);
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    internal class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }
        }
    }
}