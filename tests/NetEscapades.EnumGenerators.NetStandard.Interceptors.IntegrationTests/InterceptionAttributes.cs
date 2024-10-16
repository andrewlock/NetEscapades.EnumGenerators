using System;
using System.IO;
using Foo;
using NetEscapades.EnumGenerators;
using NetEscapades.EnumGenerators.NetStandard.IntegrationTests;

[assembly:Interceptable<EnumInSystem>]
[assembly:Interceptable<EnumInFoo>]
[assembly:Interceptable<EnumInNamespace>]
[assembly:Interceptable<EnumWithDisplayNameInNamespace>]
[assembly:Interceptable<StringTesting>]
[assembly:Interceptable<EnumWithExtensionInOtherNamespace>(ExtensionClassName="SomeExtension", ExtensionClassNamespace = "SomethingElse")]
[assembly:Interceptable<DateTimeKind>]
[assembly:Interceptable<FlagsEnum>]
[assembly:Interceptable<FileShare>]
