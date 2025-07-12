# Sufficit.Asterisk.FastAGI

[![NuGet](https://img.shields.io/nuget/v/Sufficit.Asterisk.FastAGI.svg)](https://www.nuget.org/packages/Sufficit.Asterisk.FastAGI/)

## Description

`Sufficit.Asterisk.FastAGI` provides a **high-performance FastAGI server implementation** for .NET applications, enabling Asterisk to delegate dialplan control to external applications efficiently. This library allows you to create complex, dynamic call logic using the full power of .NET while maintaining excellent performance and reliability.

## Features

### High-Performance Server
- **Asynchronous FastAGI server** with high concurrency support
- **Connection pooling** and efficient resource management
- **Script routing system** for organizing call logic
- **Automatic request parsing** and response formatting
- **Exception handling** and error recovery
- **Comprehensive logging** and diagnostics

### Advanced Call Control
- **Full AGI command support** - Answer, Dial, Playback, Record, etc.
- **DTMF handling** - Wait for digits, get user input
- **Variable management** - Get/Set channel variables
- **Call flow control** - Conditional logic and branching
- **Database integration** - Direct database access during calls
- **External API integration** - REST calls during call processing

### Flexible Architecture
- **Script-based routing** - Map URLs to specific call handlers
- **Dependency injection** support for business logic
- **Middleware pipeline** for request/response processing
- **Custom authentication** and authorization
- **Health monitoring** and metrics collection

### Framework Support
- **Multi-target framework support** (.NET Standard 2.0, .NET 7, 8, 9)
- **ASP.NET Core integration** for web-based scenarios
- **Background service** support for standalone applications
- **Modern async/await** patterns throughout

### FastAGI vs Traditional AGI
**FastAGI** offers significant advantages over traditional AGI:
- **Network-based communication** - No process spawning overhead
- **Connection pooling** - Reuse connections for better performance
- **Scalability** - Handle multiple concurrent calls efficiently
- **Language flexibility** - Use any .NET language for call logic
- **Rich debugging** - Full debugging capabilities in your IDE

## Installation
dotnet add package Sufficit.Asterisk.FastAGI
## Usage

For detailed usage examples and documentation, see [USAGE.md](USAGE.md).

## License

This project is licensed under the [MIT License](LICENSE).

## References and Thanks

This project stands on the shoulders of giants in the Asterisk .NET community. We are deeply grateful to the original authors and contributors who paved the way:

### Reference Projects

- **[Asterisk.NET by roblthegreat](https://github.com/roblthegreat/Asterisk.NET)** - A foundational library that provided essential insights into AGI protocol implementation, command formatting, and response parsing patterns that informed our FastAGI architecture.

- **[AsterNET by AsterNET Team](https://github.com/AsterNET/AsterNET)** - An excellent reference for AGI command implementations, channel variable handling, and network communication patterns with Asterisk servers.

These projects were crucial in understanding AGI protocol specifications, connection handling, and the intricacies of communicating with Asterisk over network sockets. Our implementation leverages this collective knowledge while introducing modern server architecture and performance optimizations.

**Made with ❤️ by the Sufficit Team**