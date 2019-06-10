# BaseCampApi

This is a C# wrapper to (most of) the [Basecamp 3 API](https://github.com/basecamp/bc3-api).

It is provided with Visual Studio 2019 build solutions for .NET4, Net Core 2 and Net Core 3.

There is a test project (for net core only) which also demonstrates usage.

## Using the API

In order to use the Basecamp API you need to register your application at (launchpad.37signals.com/integrations)[launchpad.37signals.com/integrations]. This returns a Client Id and Client Secret. When registering, you have to provide a redirect uri for the OAuth2 authorisation process. For simple use, provide something like http://localhost:9999 (choose another port number if you like).

This information has to be provided in an object that implements the [ISettings](../blob/master/BaseCampApi/Settings.cs) interface, which is then used to create a BaseCampApi instance. A Settings class which imnplements this interface is provided, to save you work. This provides a static Load method, reads the settings from *LocalApplicationData*/BaseCampApi/Settings.json. On a Windows 10 machine, *LocalApplicationData* is `C:\Users\<USER>\AppData\Local`, on Linux it is `~user/.local/share`.

## Testing

In order to run the Unit Tests provided, you must provide additional data in your ISettings object - see the Settings object in [UnitTest1.cs](../blob/master/Tests/UnitTest1.cs).
