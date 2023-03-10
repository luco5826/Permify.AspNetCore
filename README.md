# Permify.AspNetCore

This is an unofficial implementation of the [Permify](https://github.com/Permify/permify) authorization service to be used in Asp.NET Core applications

## Installation

This package is available as a NuGet package and can be added to your project through the command

```
dotnet add package Permify.AspNetCore
```


## Usage

```cs
// Program.cs
using Permify.AspNetCore.Extensions;

builder.Services.AddPermify(opts =>
{
    opts.Host = "http://localhost:3478";
});

// From now on, the IPermifyAuthorizationService singleton is available for DI


// MyController.cs
[HttpGet]
public Task<ActionResult> GetData()
{
    bool isUserAllowed = await _authorizationService.Can
    (
        // Extract from the HTTP request
        new Subject("1", "user"), 

        // Depending on the action this request is representing
        "get_data",

        // Extracted from request parameters
        new Entity("10", "datarepo")
    );

    if (!isUserAllowed)
    {
        return await Task.FromResult(Forbid());
    }

    // User is allowed...
}
```

## Build instructions

This project has been built with the following tools:
- `dotnet`: .NET Core SDK (v6.0.302) ([installation instructions](https://dotnet.microsoft.com/en-us/download/dotnet/6.0))
- `buf`: the protobuf management tool (v1.11.0) ([installation instructions](https://docs.buf.build/installation))

Once all the tools have been installed, clone the repository, generate the protobuf files following the `.proto` files specifications and build the project

```
git clone https://github.com/luco5826/Permify.AspNetCore.git
cd Permify.AspNetCore/src/Permify.AspNetCore/Protos
buf generate --include-imports
cd ..
dotnet build
```