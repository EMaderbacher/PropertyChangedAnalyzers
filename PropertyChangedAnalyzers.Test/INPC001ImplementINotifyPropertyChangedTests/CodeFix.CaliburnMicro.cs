namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class CaliburnMicro
        {
            private static readonly IReadOnlyList<MetadataReference> MetadataReferences = Gu.Roslyn.Asserts.MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase)).ToArray();

            [Test]
            public static void SubclassPropertyChangedBaseAddUsing()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using Caliburn.Micro;

    public class Foo : PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass Caliburn.Micro.PropertyChangedBase and add using.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SubclassPropertyChangedBaseFullyQualified()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass Caliburn.Micro.PropertyChangedBase fully qualified.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void ImplementINotifyPropertyChangedAddUsings()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void ImplementINotifyPropertyChangedFullyQualified()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.", metadataReferences: MetadataReferences);
            }
        }
    }
}
