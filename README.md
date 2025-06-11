<h1>
  Sufficit.Asterisk.FastAGI
  <a href="[https://github.com/sufficit](https://github.com/sufficit)"><img src="[https://avatars.githubusercontent.com/u/66928451?s=200&v=4](https://avatars.githubusercontent.com/u/66928451?s=200&v=4)" alt="Sufficit Logo" width="80" align="right"></a>
</h1>

[![NuGet](https://img.shields.io/nuget/v/Sufficit.Asterisk.FastAGI.svg)]([https://www.nuget.org/packages/Sufficit.Asterisk.FastAGI/](https://www.nuget.org/packages/Sufficit.Asterisk.FastAGI/))

## 📖 About the Project

`Sufficit.Asterisk.FastAGI` implements a **Fast Asterisk Gateway Interface (FastAGI)** server for the Sufficit platform. It allows Asterisk to delegate dialplan control to an external application efficiently, enabling the creation of complex and dynamic call logic.

### ✨ Key Features

* High-performance FastAGI server.
* Mapping of AGI scripts to specific routes or classes.
* Integration with the business logic of `Sufficit.Asterisk.Core`.
* Asynchronous communication with Asterisk.

## 🚀 Getting Started

To run this FastAGI server, you will need to configure your Asterisk dialplan to forward calls to it.

### 📋 Prerequisites

* .NET SDK (e.g., .NET 6.0 or higher)
* An Asterisk server.
* Knowledge of Asterisk dialplan and AGI.

### 📦 NuGet Package

Install the package into your project via the .NET CLI or the NuGet Package Manager Console.

**.NET CLI:**

    dotnet add package Sufficit.Asterisk.FastAGI

**Package Manager Console:**

    Install-Package Sufficit.Asterisk.FastAGI

### Asterisk Dialplan (`extensions.conf`)

To use this server, add an AGI call in your Asterisk dialplan.

**Example:**

    [default]
    ; Forward calls for extension 1234 to the FastAGI server
    exten => 1234,1,NoOp(Starting FastAGI script)
     same => n,AGI(agi://127.0.0.1/my-script-route)
     same => n,Hangup()

*In the example above, `127.0.0.1` is the address of your FastAGI server and `my-script-route` is the route your application will handle.*

## 🛠️ Usage

The logic for each AGI script is implemented within your application code. You can map different "script" routes to different functionalities.

**Example of how to handle an AGI script:**

    using Sufficit.Asterisk.FastAGI;

    // In your AGI routing logic, map "my-script-route" to this class
    public class MyScriptAGI : IAGIScript
    {
        public async Task Execute(IAGIChannel channel)
        {
            await channel.Answer();
            await channel.StreamFile("welcome");
            // ...
            // Your custom logic here
            // ...
            await channel.Hangup();
        }
    }

## 🤝 Contributing

Contributions are welcome! If you want to add new features or fix bugs, please submit a Pull Request.

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 📧 Contact

Sufficit - [contato@sufficit.com.br](mailto:contato@sufficit.com.br)

Project Link: [https://github.com/sufficit/sufficit-asterisk-fastagi](https://github.com/sufficit/sufficit-asterisk-fastagi)