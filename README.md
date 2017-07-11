# FluentdClient.Sharp

Fluentd client for C# and VB.NET that is thin and simple.

Currently, it supports TCP connection only.

## Install

from NuGet - [FluentdClient.Sharp](https://www.nuget.org/packages/FluentdClient.Sharp/)

```ps1
PM > Install-Package FluentdClient.Sharp
```

## How to use

At first, you must create an instance of ```FluentdClient```, with giving the informations of fluentd server(host and port) and the serializer(messagepack formatted), or the instance of ```FluentdSetting``` including those.

Next, you can connect to fluetnd server with calling ```ConnectAsync``` method.

Finally, you can send a message to fluentd server to call ```SendAsync``` method.
(If you don't call ```ConnectAsync``` method before, ```FluetndClient``` calls it automatically.)

```csharp
using (var client = new FluentdClient("172.0.0.1", 24224, new MsgPackSerializer()))
{
    await client.ConnectAsync();

    await client.SendAsync("sample-tag", new { MachineName = Environment.MachineName });
}
```

If you use ```FluentdSetting``` when you create the instance of ```FluentdClient```, you can set the timeout and how to handle exceptions when sending messages to fluentd server.

```csharp
var setting = new FluentdSetting("172.0.0.1", 24224, new MessagePackSerializer());

// set timeout(ms)
setting.Timeout = 5000;
// set how to handle exception
setting.ExceptionHandler = new Action<Exception>(ex => Console.WriteLine(ex));

using (var client = new FluentdClient(setting))
{
    // …
}
```

## Serializers

You can choice the serialization libraries, MsgPack-Cli or MessagePack for C#.

* [MsgPack-Cli](https://www.nuget.org/packages/MsgPack.Cli/) - [Github](https://github.com/msgpack/msgpack-cli)
* [MessagePack for C#](https://www.nuget.org/packages/MessagePack/) - [Github](https://github.com/neuecc/MessagePack-CSharp)

MsgPack-Cli is the standard MessagePack library for .NET, that is included in [msgpack Github repository](https://github.com/msgpack).

MessagePack for C# is the extreamly fast MessagePack library for .NET, and faster than other binary / JSON format serializers.

You can get implemented libraries from NuGet.

* [FluentdClient.Sharp.MsgPack](https://www.nuget.org/packages/FluentdClient.Sharp.MsgPack/) (Using MsgPack-Cli inner.)
* [FluentdClient.Sharp.MessagePack](https://www.nuget.org/packages/FluentdClient.Sharp.MessagePack/) (Using MessagePack for C# inner.)

```ps1
PM > FluentdClient.Sharp.MsgPack
PM > FluentdClient.Sharp.MessagePack
```

Both can be injected the custom serialization setting when creating instances.

```MsgPackSerializer```(implementation of MsgPack-Cli) recieves a instance of ```SerializationContext```. You can see about ```SerializationContext``` [here](https://github.com/msgpack/msgpack-cli/blob/master/samples/Samples/Sample03_SerializationContextAndOptions.cs).

If you don't give a instance of ```SerializationContext```, ```SerializationContext.Default``` is given.

```csharp
var context = new SerializationContext();

var serializer = new MsgPackSerializer(context);
```

```MessagePackSerializer```(implementation of MessagePack for C#) recieves a instance of ```IMessagePackFormatterResolver```. You can see about ```IMessagePackFormatterResolver``` [here](https://github.com/neuecc/MessagePack-CSharp#extension-pointiformatterresolver).

If you don't give a instance of ```IMessagePackFormatterResolver```, ```TypelessContractlessStandardResolver.Instance``` is given.

```MultipleFormatterResolver``` is used inner actually because it includes ```PayloadFormatterResolver``` that resolves the format of ```Payload```(message class) and is required.

Currently, ```MessagePackSerializer``` can't serialize a nested typed object.

```csharp
var serializer = new MessagePackSerializer(StandardResolver.Instance);
```

## Lisence

under [MIT Lisence](https://opensource.org/licenses/MIT)