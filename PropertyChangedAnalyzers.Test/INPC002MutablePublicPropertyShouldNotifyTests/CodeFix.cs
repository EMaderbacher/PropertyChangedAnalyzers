namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new MakePropertyNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC002");

        [Test]
        public static void CallsOnPropertyChangedCopyLocalNullCheckInvoke()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.value;
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
        }

        [Test]
        public static void AutoPropertyExplicitNameHandlesRecursionInInvoker()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void InternalClass()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class Foo : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
        }

        [Test]
        public static void InternalProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓internal int Bar { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
        }

        [Test]
        [Explicit("Not sure how we want this.")]
        public static void NoFixWhenBaseHasInternalOnPropertyChanged()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        ↓public int Foo { get; set; }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }

        [TestCase("this.Value = 1;")]
        [TestCase("this.Value++")]
        [TestCase("this.Value--")]
        public static void WhenPrivateSetAssignedInLambdaInCtor(string assignCode)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public Foo()
        {
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value { get; private set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.Value = 1", assignCode);

            var after = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int value;

        public Foo()
        {
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.value;
            private set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.Value = 1", assignCode);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after);
        }

        [Test]
        public static void WhenBackingFieldNotifyWhenValueChanged()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.value;
            set
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
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify when value changes.");
        }

        [Test]
        public static void WhenBackingFieldNotifyCallerMemberName()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.value;
            set => this.value = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
            set
            {
                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify.");
        }

        [Test]
        public static void WhenBackingFieldExpressionBodyAccessors()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.value;
            set => this.value = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify when value changes.");
        }

        [Test]
        public static void WhenBackingFieldNotify()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.value;
            set
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
            set
            {
                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify.");
        }

        [Test]
        public static void WhenBackingFieldExpressionBodyAccessorsNotify()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.value;
            set => this.value = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
            set
            {
                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, after, fixTitle: "Notify.");
        }

        [Test]
        public static void WhenSettingNestedField()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.bar.BarValue;
            set => this.bar.BarValue = value;
        }

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
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.bar.BarValue;
            set
            {
                if (value == this.bar.BarValue)
                {
                    return;
                }

                this.bar.BarValue = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, testCode }, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, testCode }, after, fixTitle: "Notify when value changes.");
        }

        [Test]
        public static void WhenSettingNestedFieldNotify()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.bar.BarValue;
            set => this.bar.BarValue = value;
        }

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
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.bar.BarValue;
            set
            {
                this.bar.BarValue = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, testCode }, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, testCode }, after, fixTitle: "Notify.");
        }
    }
}
