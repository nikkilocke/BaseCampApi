# BaseCampApi

This is a C# wrapper to (most of) the [Basecamp 3 API](https://github.com/basecamp/bc3-api).

It is provided with a Visual Studio 2019 build solution for .NET Standard, so can be used with any version of .NET.

There is a test project (for net core only) which also demonstrates usage.

## Using the API

In order to use the Basecamp API you need to register your application at (launchpad.37signals.com/integrations)[launchpad.37signals.com/integrations]. This returns a Client Id and Client Secret. When registering, you have to provide a redirect uri for the OAuth2 authorisation process. For simple use, provide something like http://localhost:9999 (choose another port number if you like).

This information has to be provided in an object that implements the [ISettings](../master/BaseCampApi/Settings.cs) interface, which is then used to create a BaseCampApi instance. A Settings class which imnplements this interface is provided, to save you work. This provides a static Load method, reads the settings from *LocalApplicationData*/BaseCampApi/Settings.json. On a Windows 10 machine, *LocalApplicationData* is `C:\Users\<USER>\AppData\Local`, on Linux it is `~user/.local/share`.

## Testing

In order to run the Unit Tests provided, you must provide additional data in your ISettings object - see the Settings object in [UnitTest1.cs](../master/Tests/UnitTest1.cs).

## Hooks for more complex uses

You do not have to use the provided Settings class, provided you have a class that implements ISettings.

As part of the OAuth2 process, the default implementation starts a browser to obtain authorisation. This is done by calling OpenBrowser. You can provide an alternative action to open a browser, or otherwise call the 37signals page to ask for authorisation.

Once authorisation is complete, the OAuth2 process will redirect the browser to the redirect url you provide in the settings. The default implementation provides an extremely dumb web server to listed on the redirect url port, and collect the `code=` parameter from the request. You can provide an alternative by providing a `WaitForRedirect` async function.

These options would be useful if you were using the Api in your own Web Server, for instance.

## License

This wrapper is licensed under creative commons share-alike, see [license.txt](../master/license.txt).
