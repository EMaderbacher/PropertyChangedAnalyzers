namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class ValidCode
    {
        internal class ThirdParty
        {
            [TearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetMetadataReferences();
            }

            [Test]
            public void MvvmLight()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.AddRange(MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly));
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void CaliburnMicro()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.AddRange(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly));
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void Stylet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void MvvmCross()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCross);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SubclassBindableBase()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        public int Bar { get; set; }
    }
}";
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}