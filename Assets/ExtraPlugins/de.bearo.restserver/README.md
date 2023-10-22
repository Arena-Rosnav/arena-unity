`---
title: REST Server Documentation
author: Markus Seidl
date: March 03, 2022
---

# Online Documentation

See [here](https://markus-seidl.de/unity/restserver/) for an exhaustive and up-to-date documentation.

# Offline Documentation

## Installation

* Download the RestServer Package via the Plugin Manager in Unity by selecting _Packages: My Assets_ in the _Package Manager_. If you don't have the _Package Manager_ tool window, you can enable it via the  Menu _Window -> Package Manager_.
* Select the _Rest Server_ Package click on _Download_ on the right side window pane.
* After a short download you can _Import_ the package into the project. Make sure that __all__ files are selected (by clicking _All_) and click _Import_.
* Create a new _GameObject_
* Add the _RestServer_ as a component to the created _GameObject_
* Make sure _AutoStart_ is set to true (or provide your custom start method) and the _GameObject_ is enabled. Everything is now setup and new endpoints can be registered.

## Receiving Requests

To receive requests you need to install the _Rest Server_ package, create a GameObject and add the _RestServer_ component to it. 
After the installation the rest server is start up default on [http://localhost:8080](http://localhost:8080) and can receive requests.

Sub-paths under [http://localhost:8080](http://localhost:8080) can now be registered and extended with custom functionality.

## Defining/Registering an Endpoint

Endpoint, or urls, that the server shall respond to, can be registered any time before or after the server has been started.
Only after an endpoint has been registered, it can be called from a possible client.

An endpoint can be registered like this:

```c#
using RestServer;
using UnityEngine;

public class Example : MonoBehaviour {
    // Reference to the RestServer instance 
    public RestServer.RestServer server;
    
    private void Start() {
        // Register the endpoint
        server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/position", (request) => {
            // do work
            
            // send information back
            request.CreateResponse().SendAsync();
        });
    }
}
```

The library provides two methods to register an endpoint:

* `void RegisterEndpoint(HttpMethod, String, Action)`
* `void RegisterEndpoint(HttpMethod, Regex, Action)`

The first parameter describes to which Http Verb the endpoint should respond to (GET, PUT, POST, ...) while the second
argument specifies the location of the endpoint. In the first overload, the string case, the endpoint is specified from
the server root. So the location `"/ping"` will be accessible through [http://localhost:8080/ping](http://localhost:8080/ping).
The second overload, the Regex case, allows to specify a regex the incoming location has to fulfill so to execute the
defined `Action`.
The `Action` is executed, when an incoming request matches both the `HttpMethod` and the `string`/`Regex`. `Action`s can
be specified with the Lambda syntax

```c#
server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/position", (request) => { 
    // Action
    request.CreateResponse().SendAsync();
});
```

or with a method reference

```c#
void Start()
{
    server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/position", Handler);
}

private void Handler(RestRequest request) {
    // Action
    request.CreateResponse().SendAsync();
}
```

More in-depth documentation can be found [online](https://markus-seidl.de/unity/restserver/). Otherwise the examples
are also worth a look.
