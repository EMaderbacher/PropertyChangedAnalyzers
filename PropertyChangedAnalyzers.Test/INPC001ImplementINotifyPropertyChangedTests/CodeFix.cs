namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC001ImplementINotifyPropertyChanged();
        private static readonly CodeFixProvider Fix = new ImplementINotifyPropertyChangedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC001");

        [Test]
        public static void Message()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        public int Bar1 { get; set; }

        public int Bar2 { get; set; }
    }
}";

            var expectedMessage = ExpectedDiagnostic.CreateFromCodeWithErrorsIndicated(
                "INPC001",
                "The class Foo should notify for:\r\nBar1\r\nBar2",
                before,
                out before);
            RoslynAssert.Diagnostics<INPC001ImplementINotifyPropertyChanged>(expectedMessage, before);
        }

        [Test]
        public static void WhenPublicClassPublicAutoProperty()
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenInternalClassInternalAutoProperty()
        {
            var before = @"
namespace RoslynSandbox
{
    internal class ↓Foo
    {
        internal int Bar { get; set; }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    internal class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal int Bar { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenNotNotifyingWithBackingField()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        private int value;

        public int Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
            }
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenNotNotifyingWithBackingFieldExpressionBodies()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        private int value;

        public int Value
        {
            get => this.value;
            private set => this.value = value;
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.value;
            private set => this.value = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenNotNotifyingWithBackingFieldUnderscoreNames()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ↓Foo
    {
        private int _value;

        public int Value
        {
            get
            {
                return _value;
            }
            private set
            {
                _value = value;
            }
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int _value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return _value;
            }
            private set
            {
                _value = value;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenEventOnly()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ↓Foo
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenEventAndInvokerOnly()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ↓Foo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        [Explicit("Not sure how we want this.")]
        public static void IgnoresWhenBaseIsMouseGesture()
        {
            var before = @"
namespace RoslynSandBox
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        ↓public int Foo { get; set; }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
        }

        [TestCase("this.Value = 1;")]
        [TestCase("this.Value++")]
        [TestCase("this.Value--")]
        public static void WhenPrivateSetAssignedInLambdaInCtor(string assignCode)
        {
            var before = @"
namespace RoslynSandbox
{
    using System;

    public class ↓Foo
    {
        public Foo()
        {
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;

        public int Value { get; private set; }
    }
}".AssertReplace("this.Value = 1", assignCode);

            var after = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public Foo()
        {
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value { get; private set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.Value = 1", assignCode);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }
    }
}
