using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;

#if INTERCEPTORS
namespace NetEscapades.EnumGenerators.Tests;
#else
namespace NetEscapades.EnumGenerators.Tests.Roslyn4_04;
#endif

[UsesVerify]
public class InterceptorTests
{
    private readonly Dictionary<string, string> _analyzerOpts =
        new() { { "build_property.EnableEnumGeneratorInterceptor", "true" } };

    private readonly Dictionary<string, string> _features =
        new()
        {
            { "InterceptorsPreviewNamespaces", "NetEscapades.EnumGenerators" },
            { "InterceptorsNamespaces", "NetEscapades.EnumGenerators" },
        };
#if INTERCEPTORS
    [Fact]
    public Task CanInterceptToString()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                internal class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptHasFlag()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;
            using System;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                [Flags]
                internal enum MyEnum
                {
                    First = 1,
                    Second = 2,
                    Third = 4,
                }
                
                internal class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.HasFlag(MyEnum.First);
                        var var2 = MyEnum.Second.HasFlag(myValue);
                        var var3 = Property.HasFlag(MyEnum.First);
                        var var4 = _field.HasFlag(MyEnum.First);
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptEnumInGlobalNamespace()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;
            using System;

            [EnumExtensions]
            [Flags]
            internal enum MyEnum
            {
                First = 1,
                Second = 2,
                Third = 4,
            }
            
            internal class InnerClass
            {
                public MyEnum _field = default;
                public MyEnum Property {get;set;} = default;
                public void MyTest()
                {
                    var myValue = MyEnum.Second;
                    var var1 = myValue.HasFlag(MyEnum.First);
                    var var2 = MyEnum.Second.HasFlag(myValue);
                    var var3 = Property.HasFlag(MyEnum.First);
                    var var4 = _field.HasFlag(MyEnum.First);
                    var var5 = myValue.ToString();
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptEnumInDifferentNamespace()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;
            using System;

            namespace Foo
            {
                [EnumExtensions(ExtensionClassNamespace = "Bar", ExtensionClassName = "SomethingElse")]
                [Flags]
                internal enum MyEnum
                {
                    First = 1,
                    Second = 2,
                    Third = 4,
                }
            }
            
            namespace Baz
            {
                using Foo;
                internal class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.HasFlag(MyEnum.First);
                        var var2 = MyEnum.Second.HasFlag(myValue);
                        var var3 = Property.HasFlag(MyEnum.First);
                        var var4 = _field.HasFlag(MyEnum.First);
                        var var5 = myValue.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanHandleEnumsWithSameNameInDifferentNamespaces()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;
            using System;

            namespace Foo
            {
                [EnumExtensions]
                [Flags]
                internal enum MyEnum
                {
                    First = 1,
                    Second = 2,
                    Third = 4,
                }
            }

            namespace Bar
            {
                [EnumExtensions]
                [Flags]
                internal enum MyEnum
                {
                    First = 1,
                    Second = 2,
                    Third = 4,
                }
            }
            
            namespace Baz
            {
                internal class InnerClass
                {
                    public Foo.MyEnum _field = default;
                    public Bar.MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = Foo.MyEnum.Second;
                        var var1 = myValue.HasFlag(Foo.MyEnum.First);
                        var var2 = Foo.MyEnum.Second.HasFlag(myValue);
                        var var3 = Property.HasFlag(Bar.MyEnum.First);
                        var var4 = _field.HasFlag(Foo.MyEnum.First);
                        var var5 = myValue.ToString();
                        var var6 = Bar.MyEnum.First.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptMultipleEnumsToString()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                [EnumExtensions]
                internal enum AnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                internal enum YetAnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }
                
                public class InnerClass
                {
                    public void MyTest()
                    {
                        var result = AnotherEnum.First.ToString();
                        AssertValue1(MyEnum.First);
                        AssertValue2(AnotherEnum.Second);
                        AssertValue1(MyEnum.Third);
                        var result2 = YetAnotherEnum.First.ToString();
                        AssertValue3(YetAnotherEnum.Second);
                        
                        void AssertValue1(MyEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue2(AnotherEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue3(YetAnotherEnum value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptEnumMarkedAsNotInterceptable()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;
            using System;

            namespace MyTestNameSpace
            {
                [EnumExtensions(IsInterceptable = false)]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                [EnumExtensions]
                [Flags]
                internal enum AnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }

                internal enum YetAnotherEnum
                {
                    First = 0,
                    Second = 1,
                    Third = 2,
                }
                
                internal class InnerClass
                {
                    public void MyTest()
                    {
                        var result = AnotherEnum.First.ToString();
                        AssertValue1(MyEnum.First);
                        AssertValue2(AnotherEnum.Second);
                        AssertValue1(MyEnum.Third);
                        var result2 = YetAnotherEnum.First.ToString();
                        AssertValue3(YetAnotherEnum.Second);
                        
                        var hasValue = AnotherEnum.Second.HasFlag(AnotherEnum.First);
                        
                        void AssertValue1(MyEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue2(AnotherEnum value)
                        {
                            var toString = value.ToString();
                        }

                        void AssertValue3(YetAnotherEnum value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptExternalEnumToString()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>()]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptUsingInterceptableAttributeToString()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>(IsInterceptable = false)]
            [assembly:Interceptable<StringComparison>]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptExternalEnumMarkedAsNotInterceptable()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;
            
            [assembly:EnumExtensions<StringComparison>()]
            [assembly:NetEscapades.EnumGenerators.EnumExtensions<DateTimeKind>(IsInterceptable = false)]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);
                        
                        var var5 = DateTimeKind.Unspecified.ToString();

                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanInterceptToStringWhenCsharp11()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                internal class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var opts = new TestHelpers.Options(LanguageVersion.CSharp11, _analyzerOpts, _features, input);
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(opts);

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptToStringWhenDisabled()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                internal class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotGenerateWarningsForUseOfObsoleteEnums_CS0612_Issue97()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                [Obsolete]
                Second,
            }

            [EnumExtensions]
            public enum OtherEnum
            {
                First,
                Second,
            }
            
            internal class InnerClass
            {
                public void MyTest()
                {
            #pragma warning disable CS0612
            #pragma warning disable CS0618
                    var var1 = OtherEnum.Second.ToString();
                    var var2 = MyEnum.Second.ToString();
            #pragma warning restore CS0612
            #pragma warning restore CS0618
                }
            }

            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }


    [Fact]
    public Task DoesNotGenerateWarningsForUseOfObsoleteEnums_CS0618_Issue97()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum MyEnum
            {
                First,
                [Obsolete("This is obsolete")]
                Second,
            }

            [EnumExtensions]
            [Obsolete("This is obsolete")]
            public enum OtherEnum
            {
                First,
                Second,
            }
            
            internal class InnerClass
            {
                public void MyTest()
                {
            #pragma warning disable CS0612
            #pragma warning disable CS0618
                    var var1 = OtherEnum.Second.ToString();
                    var var2 = MyEnum.Second.ToString();
            #pragma warning restore CS0612
            #pragma warning restore CS0618
                }
            }

            """;
        var (diagnostics, output) = TestHelpers.GetGeneratedOutput<EnumGenerator>(new(input));

        Assert.Empty(diagnostics);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task DoesNotInterceptToStringWhenOldCsharp()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                internal class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var opts = new TestHelpers.Options(LanguageVersion.CSharp10, _analyzerOpts, _features, input);
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(opts);

        diagnostics.Should().ContainSingle(x => x.Id == DiagnosticHelper.CsharpVersionLooLow.Id);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

#else
    [Fact]
    public Task CanNotInterceptEnumToStringInOldSDK()
    {
        const string input =
            """
            using NetEscapades.EnumGenerators;

            namespace MyTestNameSpace
            {
                [EnumExtensions]
                internal enum MyEnum
                {
                    First = 0,
                    Second = 1,
                }
                
                public class InnerClass
                {
                    public MyEnum _field = default;
                    public MyEnum Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = MyEnum.Second;
                        var var1 = myValue.ToString();
                        var var2 = MyEnum.Second.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                    }
                }
            }
            """;
        var (diagnostics, output) =
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        diagnostics.Should().ContainSingle(x => x.Id == DiagnosticHelper.SdkVersionTooLow.Id);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

    [Fact]
    public Task CanNotInterceptInterceptableAttributeEnumInOldSDK()
    {
        const string input =
            """
            using System;
            using NetEscapades.EnumGenerators;

            [assembly:EnumExtensions<StringComparison>(IsInterceptable = false)]
            [assembly:Interceptable<StringComparison>]

            namespace MyTestNameSpace
            {
                public class InnerClass
                {
                    public StringComparison _field = default;
                    
                    public StringComparison Property {get;set;} = default;
                    public void MyTest()
                    {
                        var myValue = StringComparison.Ordinal;
                        var var1 = myValue.ToString();
                        var var2 = StringComparison.Ordinal.ToString();
                        var var3 = Property.ToString();
                        var var4 = _field.ToString();
                        AssertValue(StringComparison.OrdinalIgnoreCase);
            
                        void AssertValue(StringComparison value)
                        {
                            var toString = value.ToString();
                        }
                    }
                }
            }
            """;
        var (diagnostics, output) = 
            TestHelpers.GetGeneratedTrees<EnumGenerator, TrackingNames>(new(_analyzerOpts, _features, input));

        diagnostics.Should()
            .OnlyContain(x => x.Id == DiagnosticHelper.SdkVersionTooLow.Id)
            .And.ContainSingle(x => x.Location == Location.None)
            .And.ContainSingle(x => x.Location != Location.None);
        return Verifier.Verify(output).ScrubExpectedChanges().UseDirectory("Snapshots");
    }

#endif
}