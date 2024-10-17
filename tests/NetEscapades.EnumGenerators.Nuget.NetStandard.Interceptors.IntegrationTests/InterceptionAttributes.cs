using System;
using System.IO;
using Foo;
using NetEscapades.EnumGenerators;
#if NETSTANDARD_INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.NetStandard.IntegrationTests;
#elif NUGET_NETSTANDARD_INTERCEPTOR_TESTS
using NetEscapades.EnumGenerators.Nuget.NetStandard.Interceptors.IntegrationTests;
#else
#error Unknown integration tests
#endif

[assembly:Interceptable<EnumInSystem>]
[assembly:Interceptable<EnumInFoo>]
[assembly:Interceptable<EnumInNamespace>]
[assembly:Interceptable<EnumWithDisplayNameInNamespace>]
[assembly:Interceptable<StringTesting>]
[assembly:Interceptable<EnumWithExtensionInOtherNamespace>(ExtensionClassName="SomeExtension", ExtensionClassNamespace = "SomethingElse")]
[assembly:Interceptable<DateTimeKind>]
[assembly:Interceptable<FlagsEnum>]
[assembly:Interceptable<FileShare>]
