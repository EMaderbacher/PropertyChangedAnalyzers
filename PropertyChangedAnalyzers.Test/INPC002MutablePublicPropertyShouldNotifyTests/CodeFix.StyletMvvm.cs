namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class StyletMvvm
        {
            private static readonly IReadOnlyList<MetadataReference> MetadataReferences = SpecialMetadataReferences.Stylet;

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.NotifyOfPropertyChange();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySet()
            {
                var before = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int p;

        public int P { get => this.p; set => this.SetAndNotify(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void InternalClassInternalPropertyAutoPropertyToTrySet()
            {
                var before = @"
namespace N
{
    internal class Foo : Stylet.PropertyChangedBase
    {
        internal int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace N
{
    internal class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        internal int Bar { get => this.bar; set => this.SetAndNotify(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int ↓Bar { get; set; } = 1;
    }
}";

                var after = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar = 1;

        public int Bar { get => this.bar; set => this.SetAndNotify(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public virtual int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public virtual int Bar { get => this.bar; set => this.SetAndNotify(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int ↓Bar { get; private set; }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";

                var after = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public int Bar { get => this.bar; private set => this.SetAndNotify(ref this.bar, value); }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySetUnderscoreNames()
            {
                var before = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        public int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar { get => _bar; set => SetAndNotify(ref _bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string ↓Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}";

                var after = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string ↓Name
        {
            get => this.name;
            set => this.name = value;
        }
    }
}";

                var after = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetAndNotify(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesStatementBody()
            {
                var before = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string _name;

        public string ↓Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}";

                var after = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetAndNotify(ref _name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNames()
            {
                var before = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string _name;

        public string ↓Name
        {
            get => _name;
            set => _name = value;
        }
    }
}";

                var after = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }
        }
    }
}
