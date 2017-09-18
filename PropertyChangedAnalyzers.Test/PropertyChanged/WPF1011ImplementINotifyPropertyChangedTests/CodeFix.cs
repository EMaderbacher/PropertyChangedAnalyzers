﻿namespace PropertyChangedAnalyzers.Test.PropertyChanged.WPF1011ImplementINotifyPropertyChangedTests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;
    using PropertyChangedAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1011ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>
    {
        [Test]
        public async Task WhenNotNotifyingAutoProperty()
        {
            var testCode = @"
public class Foo
{
    ↓public int Bar { get; set; }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Implement INotifyPropertyChanged.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotNotifyingWithBackingField()
        {
            var testCode = @"
public class Foo
{
    private int value;

    ↓public int Value
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
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Implement INotifyPropertyChanged.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

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

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotNotifyingWithBackingFieldUnderscoreNames()
        {
            var testCode = @"
public class Foo
{
    private int _value;

    ↓public int Value
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
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Implement INotifyPropertyChanged.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _value;

    public event PropertyChangedEventHandler PropertyChanged;

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

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenInterfaceOnly()
        {
            var testCode = @"
public class Foo : ↓INotifyPropertyChanged
{
}";

            var expected = this.CSharpDiagnostic("CS0246").WithLocationIndicated(ref testCode).WithMessage("The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenInterfaceOnlySealed()
        {
            var testCode = @"
public sealed class Foo : ↓INotifyPropertyChanged
{
}";

            var expected = this.CSharpDiagnostic("CS0246").WithLocationIndicated(ref testCode).WithMessage("The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public sealed class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenInterfaceOnlyAndNameSpace()
        {
            var testCode = @"
namespace TestCode
{
    public class Foo : ↓INotifyPropertyChanged
    {
    }
}";

            var expected = this.CSharpDiagnostic("CS0246").WithLocationIndicated(ref testCode).WithMessage("The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
namespace TestCode
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
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenInterfaceOnlyAndNamespaceAndUsingsInside()
        {
            var testCode = @"
namespace TestCode
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    public class Foo : ↓INotifyPropertyChanged
    {
    }
}";

            var expected = this.CSharpDiagnostic("CS0246").WithLocationIndicated(ref testCode).WithMessage("The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
namespace TestCode
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenInterfaceOnlyAndNamespaceAndUsingsOutside()
        {
            var testCode = @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace TestCode
{
    public class Foo : ↓INotifyPropertyChanged
    {
    }
}";

            var expected = this.CSharpDiagnostic("CS0246").WithLocationIndicated(ref testCode).WithMessage("The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace TestCode
{
    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenInterfaceOnlyAndUsings()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : ↓INotifyPropertyChanged
{
}";

            var expected = this.CSharpDiagnostic("CS0535").WithLocationIndicated(ref testCode).WithMessage("'Foo' does not implement interface member 'INotifyPropertyChanged.PropertyChanged'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenEventOnly()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo
{
    ↓public event PropertyChangedEventHandler PropertyChanged;
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Implement INotifyPropertyChanged.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenEventAndInvokerOnly()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo
{
    ↓public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Implement INotifyPropertyChanged.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        internal override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            return base.GetCSharpDiagnosticAnalyzers()
                       .Concat(new DiagnosticAnalyzer[] { CS0535Analyzer.Default, CS0246Analyzer.Default });
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        //// ReSharper disable once InconsistentNaming
        private class CS0535Analyzer : DiagnosticAnalyzer
        {
            public static readonly CS0535Analyzer Default = new CS0535Analyzer();

            private CS0535Analyzer()
            {
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
                ImmutableArray.Create(
                    new DiagnosticDescriptor(
                        id: "CS0535",
                        title: string.Empty,
                        messageFormat: string.Empty,
                        category: string.Empty,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: false));

            public override void Initialize(AnalysisContext context)
            {
            }
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        //// ReSharper disable once InconsistentNaming
        private class CS0246Analyzer : DiagnosticAnalyzer
        {
            public static readonly CS0246Analyzer Default = new CS0246Analyzer();

            private CS0246Analyzer()
            {
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
                ImmutableArray.Create(
                    new DiagnosticDescriptor(
                        id: "CS0246",
                        title: string.Empty,
                        messageFormat: string.Empty,
                        category: string.Empty,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: false));

            public override void Initialize(AnalysisContext context)
            {
            }
        }
    }
}