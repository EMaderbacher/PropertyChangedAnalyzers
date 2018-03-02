// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC001ImplementINotifyPropertyChangedBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC001ImplementINotifyPropertyChanged());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC002MutablePublicPropertyShouldNotifyBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC002MutablePublicPropertyShouldNotify());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC003NotifyWhenPropertyChangesBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC003NotifyWhenPropertyChanges());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC004UseCallerMemberNameBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC004UseCallerMemberName());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC005CheckIfDifferentBeforeNotifyingBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC005CheckIfDifferentBeforeNotifying());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC006UseObjectEqualsForReferenceTypesBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC006UseObjectEqualsForReferenceTypes());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC006UseReferenceEqualsBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC006UseReferenceEquals());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC007MissingInvokerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC007MissingInvoker());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC008StructMustNotNotifyBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC008StructMustNotNotify());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC009DontRaiseChangeForMissingPropertyBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC009DontRaiseChangeForMissingProperty());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC011DontShadowBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC011DontShadow());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC014PreferSettingBackingFieldInCtorBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC014PreferSettingBackingFieldInCtor());

        private static readonly Gu.Roslyn.Asserts.Benchmark ArgumentAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.ArgumentAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark PropertyDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.PropertyDeclarationAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC001ImplementINotifyPropertyChanged()
        {
            INPC001ImplementINotifyPropertyChangedBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC002MutablePublicPropertyShouldNotify()
        {
            INPC002MutablePublicPropertyShouldNotifyBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC003NotifyWhenPropertyChanges()
        {
            INPC003NotifyWhenPropertyChangesBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC004UseCallerMemberName()
        {
            INPC004UseCallerMemberNameBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC005CheckIfDifferentBeforeNotifying()
        {
            INPC005CheckIfDifferentBeforeNotifyingBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC006UseObjectEqualsForReferenceTypes()
        {
            INPC006UseObjectEqualsForReferenceTypesBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC006UseReferenceEquals()
        {
            INPC006UseReferenceEqualsBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC007MissingInvoker()
        {
            INPC007MissingInvokerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC008StructMustNotNotify()
        {
            INPC008StructMustNotNotifyBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC009DontRaiseChangeForMissingProperty()
        {
            INPC009DontRaiseChangeForMissingPropertyBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC011DontShadow()
        {
            INPC011DontShadowBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC014PreferSettingBackingFieldInCtor()
        {
            INPC014PreferSettingBackingFieldInCtorBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ArgumentAnalyzer()
        {
            ArgumentAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void PropertyDeclarationAnalyzer()
        {
            PropertyDeclarationAnalyzerBenchmark.Run();
        }
    }
}
