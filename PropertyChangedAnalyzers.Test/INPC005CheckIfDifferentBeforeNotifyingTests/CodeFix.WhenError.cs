namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class WhenError
        {
            private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
            {
                new TestCaseData("string", "Equals(value, this.bar)"),
                new TestCaseData("string", "Equals(this.bar, value)"),
                new TestCaseData("string", "Equals(value, bar)"),
                new TestCaseData("string", "Equals(value, Bar)"),
                new TestCaseData("string", "Equals(Bar, value)"),
                new TestCaseData("string", "Nullable.Equals(value, this.bar)"),
                new TestCaseData("int?",   "Nullable.Equals(value, this.bar)"),
                new TestCaseData("string", "value.Equals(this.bar)"),
                new TestCaseData("string", "value.Equals(bar)"),
                new TestCaseData("string", "this.bar.Equals(value)"),
                new TestCaseData("string", "bar.Equals(value)"),
                new TestCaseData("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
                new TestCaseData("string", "ReferenceEquals(value, this.bar)"),
            };

            [TestCaseSource(nameof(TestCases))]
            public static void Check(string type, string expression)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("int", type);

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
            }

            [TestCaseSource(nameof(TestCases))]
            public static void NegatedCheckReturn(string type, string expression)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("int", type);

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
            }

            [TestCaseSource(nameof(TestCases))]
            public static void NegatedCheckAssignReturn(string type, string expression)
            {
                var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
                {
                    return;
                    this.bar = value;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("int", type);

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
            }

            [Test]
            public static void IfOperatorNotEqualsReturn()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value != this.bar)
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Ignore("#87")]
            [Test]
            public static void OperatorEqualsNoAssignReturn()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                    return;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Test]
            public static void OperatorEqualsNoAssignButNotifyOutside()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Test]
            public static void IfOperatorEqualsAssignAndNotify()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Test]
            public static void OperatorEquals()
            {
                var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }
        }
    }
}
