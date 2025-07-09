<h1>
  Sufficit.Asterisk.FastAGI
  <a href="https://github.com/sufficit"><img src="https://avatars.githubusercontent.com/u/66928451?s=200&v=4" alt="Sufficit Logo" width="80" align="right"></a>
</h1>

[![NuGet](https://img.shields.io/nuget/v/Sufficit.Asterisk.FastAGI.svg)](https://www.nuget.org/packages/Sufficit.Asterisk.FastAGI/)

## 📖 About the Project

`Sufficit.Asterisk.FastAGI` provides a **high-performance FastAGI server implementation** for .NET applications, enabling Asterisk to delegate dialplan control to external applications efficiently. This library allows you to create complex, dynamic call logic using the full power of .NET while maintaining excellent performance and reliability.

### 🎯 FastAGI vs Traditional AGI

**FastAGI** offers significant advantages over traditional AGI:
- **Network-based communication** - No process spawning overhead
- **Connection pooling** - Reuse connections for better performance  
- **Scalability** - Handle multiple concurrent calls efficiently
- **Language flexibility** - Use any .NET language for call logic
- **Rich debugging** - Full debugging capabilities in your IDE

## ✨ Key Features

### 🚀 High-Performance Server
* **Asynchronous FastAGI server** with high concurrency support
* **Connection pooling** and efficient resource management
* **Script routing system** for organizing call logic
* **Automatic request parsing** and response formatting
* **Exception handling** and error recovery
* **Comprehensive logging** and diagnostics

### 🎯 Advanced Call Control
* **Full AGI command support** - Answer, Dial, Playback, Record, etc.
* **DTMF handling** - Wait for digits, get user input
* **Variable management** - Get/Set channel variables
* **Call flow control** - Conditional logic and branching
* **Database integration** - Direct database access during calls
* **External API integration** - REST calls during call processing

### 🏗️ Flexible Architecture
* **Script-based routing** - Map URLs to specific call handlers
* **Dependency injection** support for business logic
* **Middleware pipeline** for request/response processing
* **Custom authentication** and authorization
* **Health monitoring** and metrics collection

### 🌐 Framework Support
* **Multi-target framework support** (.NET Standard 2.0, .NET 7, 8, 9)
* **ASP.NET Core integration** for web-based scenarios
* **Background service** support for standalone applications
* **Modern async/await** patterns throughout

## 🚀 Getting Started

### 📦 Installationdotnet add package Sufficit.Asterisk.FastAGI
### 📋 Prerequisites
* **.NET SDK** - Version 7.0 or higher recommended
* **Asterisk server** - Configured to use FastAGI
* **Network connectivity** - Asterisk server must reach your FastAGI application

## ⚙️ Basic Setup

### 1. Configure Asterisk Dialplan
Add FastAGI calls to your Asterisk dialplan (`extensions.conf`):
[default]
; Basic FastAGI call
exten => 1000,1,NoOp(Starting FastAGI Script)
 same => n,AGI(agi://fastagi-server.company.com:4573/welcome)
 same => n,Hangup()

; FastAGI with parameters
exten => 1001,1,NoOp(Customer Service)
 same => n,AGI(agi://fastagi-server.company.com:4573/customer-service?queue=support&priority=high)
 same => n,Hangup()

; Advanced call routing
exten => _1XXX,1,NoOp(Extension Call)
 same => n,AGI(agi://fastagi-server.company.com:4573/extension-call?target=${EXTEN}&caller=${CALLERID(num)})
 same => n,Hangup()
### 2. Create FastAGI Serverusing Sufficit.Asterisk.FastAGI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

// Program.cs
var builder = Host.CreateApplicationBuilder(args);

// Configure FastAGI server
builder.Services.AddFastAGIServer(options =>
{
    options.Port = 4573;
    options.MaxConcurrentConnections = 100;
    options.ConnectionTimeout = TimeSpan.FromMinutes(5);
});

// Register your AGI scripts
builder.Services.AddScoped<WelcomeScript>();
builder.Services.AddScoped<CustomerServiceScript>();
builder.Services.AddScoped<ExtensionCallScript>();

var host = builder.Build();

// Configure script routing
var agiServer = host.Services.GetRequiredService<FastAGIServer>();
agiServer.MapScript("/welcome", typeof(WelcomeScript));
agiServer.MapScript("/customer-service", typeof(CustomerServiceScript));
agiServer.MapScript("/extension-call", typeof(ExtensionCallScript));

await host.RunAsync();
## 🎯 Script Implementation

### Basic Script Exampleusing Sufficit.Asterisk.FastAGI;

public class WelcomeScript : IAGIScript
{
    private readonly ILogger<WelcomeScript> _logger;
    
    public WelcomeScript(ILogger<WelcomeScript> logger)
    {
        _logger = logger;
    }
    
    public async Task ExecuteAsync(IAGIChannel channel, IAGIRequest request)
    {
        try
        {
            _logger.LogInformation("Welcome script starting for channel: {Channel}", channel.ChannelName);
            
            // Answer the call
            await channel.AnswerAsync();
            
            // Play welcome message
            await channel.StreamFileAsync("welcome");
            
            // Wait for user input
            var digit = await channel.WaitForDigitAsync(5000); // 5 second timeout
            
            if (digit != null)
            {
                _logger.LogInformation("User pressed: {Digit}", digit);
                await channel.SayDigitsAsync(digit.ToString());
            }
            else
            {
                await channel.StreamFileAsync("timeout");
            }
            
            // Graceful hangup
            await channel.HangupAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in welcome script");
            await channel.HangupAsync();
        }
    }
}
### Advanced Customer Service Scriptusing Sufficit.Asterisk.FastAGI;

public class CustomerServiceScript : IAGIScript
{
    private readonly ILogger<CustomerServiceScript> _logger;
    private readonly ICustomerService _customerService;
    private readonly IQueueService _queueService;
    
    public CustomerServiceScript(
        ILogger<CustomerServiceScript> logger,
        ICustomerService customerService,
        IQueueService queueService)
    {
        _logger = logger;
        _customerService = customerService;
        _queueService = queueService;
    }
    
    public async Task ExecuteAsync(IAGIChannel channel, IAGIRequest request)
    {
        try
        {
            await channel.AnswerAsync();
            
            // Get caller information
            var callerNumber = await channel.GetVariableAsync("CALLERID(num)");
            var customer = await _customerService.GetCustomerByPhoneAsync(callerNumber);
            
            if (customer != null)
            {
                // Existing customer - personalized greeting
                await channel.StreamFileAsync("welcome-back");
                await channel.SayDigitsAsync(customer.AccountNumber);
                
                // Set customer info as channel variables
                await channel.SetVariableAsync("CUSTOMER_ID", customer.Id.ToString());
                await channel.SetVariableAsync("CUSTOMER_TIER", customer.ServiceTier);
                
                // Direct to appropriate queue based on service tier
                var queueName = customer.ServiceTier switch
                {
                    "Premium" => "premium-support",
                    "Business" => "business-support", 
                    _ => "general-support"
                };
                
                await TransferToQueue(channel, queueName, customer.ServiceTier);
            }
            else
            {
                // New customer - collect information
                await HandleNewCustomer(channel, callerNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in customer service script");
            await channel.StreamFileAsync("technical-difficulties");
            await channel.HangupAsync();
        }
    }
    
    private async Task HandleNewCustomer(IAGIChannel channel, string phoneNumber)
    {
        await channel.StreamFileAsync("new-customer-welcome");
        
        // Collect customer information
        var accountNumber = await CollectAccountNumber(channel);
        if (accountNumber != null)
        {
            var customer = await _customerService.GetCustomerByAccountAsync(accountNumber);
            if (customer != null)
            {
                // Update customer phone number
                await _customerService.UpdatePhoneNumberAsync(customer.Id, phoneNumber);
                await channel.StreamFileAsync("account-updated");
                await TransferToQueue(channel, "general-support", "Standard");
                return;
            }
        }
        
        // Couldn't identify customer - transfer to sales
        await channel.StreamFileAsync("transfer-to-sales");
        await TransferToQueue(channel, "sales", "New");
    }
    
    private async Task<string?> CollectAccountNumber(IAGIChannel channel)
    {
        const int maxAttempts = 3;
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await channel.StreamFileAsync("enter-account-number");
            
            var digits = await channel.GetDataAsync("beep", 10000, 8); // 8 digit account number
            
            if (!string.IsNullOrEmpty(digits) && digits.Length == 8 && digits.All(char.IsDigit))
            {
                // Confirm the number
                await channel.StreamFileAsync("you-entered");
                await channel.SayDigitsAsync(digits);
                await channel.StreamFileAsync("press-1-to-confirm");
                
                var confirmation = await channel.WaitForDigitAsync(5000);
                if (confirmation == "1")
                {
                    return digits;
                }
            }
            
            if (attempt < maxAttempts)
            {
                await channel.StreamFileAsync("invalid-please-try-again");
            }
        }
        
        await channel.StreamFileAsync("too-many-attempts");
        return null;
    }
    
    private async Task TransferToQueue(IAGIChannel channel, string queueName, string tier)
    {
        _logger.LogInformation("Transferring {Channel} to queue {Queue} (tier: {Tier})", 
            channel.ChannelName, queueName, tier);
            
        // Set queue-specific variables
        await channel.SetVariableAsync("QUEUE_NAME", queueName);
        await channel.SetVariableAsync("SERVICE_TIER", tier);
        
        // Play hold music while transferring
        await channel.StreamFileAsync("please-hold");
        
        // Transfer to Asterisk queue (this exits the AGI script)
        await channel.ExecAsync("Queue", $"{queueName},300,t"); // 5 minute timeout
    }
}
### Extension-to-Extension Call Scriptusing Sufficit.Asterisk.FastAGI;

public class ExtensionCallScript : IAGIScript
{
    private readonly ILogger<ExtensionCallScript> _logger;
    private readonly IExtensionService _extensionService;
    
    public ExtensionCallScript(ILogger<ExtensionCallScript> logger, IExtensionService extensionService)
    {
        _logger = logger;
        _extensionService = extensionService;
    }
    
    public async Task ExecuteAsync(IAGIChannel channel, IAGIRequest request)
    {
        try
        {
            // Get target extension from query parameters
            var targetExtension = request.QueryParameters.GetValueOrDefault("target");
            var callerNumber = request.QueryParameters.GetValueOrDefault("caller");
            
            if (string.IsNullOrEmpty(targetExtension))
            {
                await channel.StreamFileAsync("invalid-extension");
                await channel.HangupAsync();
                return;
            }
            
            // Check if target extension exists and is available
            var extension = await _extensionService.GetExtensionAsync(targetExtension);
            if (extension == null)
            {
                await channel.StreamFileAsync("extension-not-found");
                await channel.HangupAsync();
                return;
            }
            
            if (!extension.IsActive)
            {
                await channel.StreamFileAsync("extension-unavailable");
                await channel.HangupAsync();
                return;
            }
            
            // Check for call forwarding
            if (!string.IsNullOrEmpty(extension.ForwardToNumber))
            {
                _logger.LogInformation("Extension {Extension} forwarded to {ForwardTo}", 
                    targetExtension, extension.ForwardToNumber);
                    
                await TransferCall(channel, extension.ForwardToNumber, "forwarded");
                return;
            }
            
            // Check for do not disturb
            if (extension.DoNotDisturb)
            {
                await channel.StreamFileAsync("do-not-disturb");
                await channel.VoicemailAsync(targetExtension);
                return;
            }
            
            // Normal extension call
            await TransferCall(channel, $"SIP/{targetExtension}", "direct");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in extension call script");
            await channel.StreamFileAsync("call-failed");
            await channel.HangupAsync();
        }
    }
    
    private async Task TransferCall(IAGIChannel channel, string destination, string type)
    {
        _logger.LogInformation("Transferring call to {Destination} (type: {Type})", destination, type);
        
        // Set caller ID if needed
        await channel.SetCallerIdAsync($"Internal <{destination}>");
        
        // Dial the destination
        await channel.ExecAsync("Dial", $"{destination},30,rt");
        
        // If dial fails, go to voicemail (this code runs if Dial returns)
        await channel.VoicemailAsync(destination);
    }
}
## 🔧 Advanced Configuration

### Server Configurationbuilder.Services.AddFastAGIServer(options =>
{
    options.Port = 4573;
    options.BindAddress = IPAddress.Any;
    options.MaxConcurrentConnections = 200;
    options.ConnectionTimeout = TimeSpan.FromMinutes(10);
    options.ReceiveTimeout = TimeSpan.FromSeconds(30);
    options.SendTimeout = TimeSpan.FromSeconds(30);
    options.EnableKeepAlive = true;
    options.LogConnections = true;
});
### Middleware Pipelinepublic class LoggingMiddleware : IAGIMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    
    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }
    
    public async Task InvokeAsync(IAGIContext context, AGIDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("AGI request started: {Script} from {Channel}", 
            context.Request.Script, context.Channel.ChannelName);
        
        try
        {
            await next(context);
            _logger.LogInformation("AGI request completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGI request failed after {Duration}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

// Register middleware
builder.Services.AddScoped<LoggingMiddleware>();
agiServer.UseMiddleware<LoggingMiddleware>();
### Authentication Middlewarepublic class AuthenticationMiddleware : IAGIMiddleware
{
    private readonly ILogger<AuthenticationMiddleware> _logger;
    
    public async Task InvokeAsync(IAGIContext context, AGIDelegate next)
    {
        // Check for API key in query parameters
        var apiKey = context.Request.QueryParameters.GetValueOrDefault("api_key");
        
        if (string.IsNullOrEmpty(apiKey) || !IsValidApiKey(apiKey))
        {
            _logger.LogWarning("Unauthorized AGI request from {Channel}", context.Channel.ChannelName);
            await context.Channel.StreamFileAsync("unauthorized");
            await context.Channel.HangupAsync();
            return;
        }
        
        await next(context);
    }
    
    private bool IsValidApiKey(string apiKey)
    {
        // Implement your API key validation
        return apiKey == "your-secret-api-key";
    }
}
## 📊 Performance and Monitoring

### Health Checksbuilder.Services.AddHealthChecks()
    .AddCheck<FastAGIHealthCheck>("fastagi-server");

// Health check implementation
public class FastAGIHealthCheck : IHealthCheck
{
    private readonly FastAGIServer _server;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_server.IsRunning)
            return HealthCheckResult.Unhealthy("FastAGI server is not running");
            
        var data = new Dictionary<string, object>
        {
            ["port"] = _server.Port,
            ["active_connections"] = _server.ActiveConnections,
            ["total_requests"] = _server.TotalRequests,
            ["uptime"] = _server.Uptime
        };
        
        return HealthCheckResult.Healthy("FastAGI server is running", data);
    }
}
### Metrics Collectionbuilder.Services.AddScoped<MetricsCollectionMiddleware>();

public class MetricsCollectionMiddleware : IAGIMiddleware
{
    private readonly IMetrics _metrics;
    
    public async Task InvokeAsync(IAGIContext context, AGIDelegate next)
    {
        var timer = _metrics.StartTimer("agi.request.duration");
        
        try
        {
            await next(context);
            _metrics.Increment("agi.request.success");
        }
        catch (Exception)
        {
            _metrics.Increment("agi.request.error");
            throw;
        }
        finally
        {
            timer.Stop();
        }
    }
}
## 🧪 Testing

### Unit Testing AGI Scripts[Test]
public async Task WelcomeScript_ShouldAnswerAndPlayWelcome()
{
    // Arrange
    var mockChannel = new Mock<IAGIChannel>();
    var mockRequest = new Mock<IAGIRequest>();
    var script = new WelcomeScript(Mock.Of<ILogger<WelcomeScript>>());
    
    // Act
    await script.ExecuteAsync(mockChannel.Object, mockRequest.Object);
    
    // Assert
    mockChannel.Verify(c => c.AnswerAsync(), Times.Once);
    mockChannel.Verify(c => c.StreamFileAsync("welcome"), Times.Once);
}
### Integration Testing[Test]
public async Task FastAGIServer_ShouldHandleMultipleConcurrentRequests()
{
    // Arrange
    var server = new FastAGIServer(options);
    await server.StartAsync();
    
    // Act
    var tasks = Enumerable.Range(0, 10)
        .Select(_ => SimulateAGIRequest(server.Port))
        .ToArray();
        
    await Task.WhenAll(tasks);
    
    // Assert
    Assert.That(server.TotalRequests, Is.EqualTo(10));
}
## 🎯 **Production Deployment**

### Docker Configuration# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 4573
ENTRYPOINT ["dotnet", "YourFastAGIApp.dll"]
### Kubernetes Deployment# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: fastagi-server
spec:
  replicas: 3
  selector:
    matchLabels:
      app: fastagi-server
  template:
    metadata:
      labels:
        app: fastagi-server
    spec:
      containers:
      - name: fastagi-server
        image: your-registry/fastagi-server:latest
        ports:
        - containerPort: 4573
        env:
        - name: FastAGI__Port
          value: "4573"
        - name: FastAGI__MaxConcurrentConnections
          value: "100"
---
apiVersion: v1
kind: Service
metadata:
  name: fastagi-service
spec:
  selector:
    app: fastagi-server
  ports:
  - port: 4573
    targetPort: 4573
  type: LoadBalancer
## 🤝 Contributing

We welcome contributions! Areas of focus:

### 🎯 Contribution Opportunities
* **New AGI Commands** - Additional Asterisk AGI command implementations
* **Middleware Components** - Authentication, logging, metrics middleware
* **Performance Optimizations** - Connection pooling and memory efficiency improvements
* **Testing Tools** - AGI script testing utilities and mocks
* **Documentation** - Real-world examples and deployment guides
* **Integration Examples** - Database, API, and service integrations

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🆘 Support

- 📖 **Documentation**: [GitHub Repository](https://github.com/sufficit/sufficit-asterisk-fastagi)
- 🐛 **Issues**: [GitHub Issues](https://github.com/sufficit/sufficit-asterisk-fastagi/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/sufficit/sufficit-asterisk-fastagi/discussions)
- 📧 **Email**: support@sufficit.com

## 🔗 Related Projects

- **[Sufficit.Asterisk.Core](https://github.com/sufficit/sufficit-asterisk-core)** - Core events, actions, and configuration models
- **[Sufficit.Asterisk.Manager](https://github.com/sufficit/sufficit-asterisk-manager)** - AMI provider and service infrastructure
- **[Sufficit.AMIEvents](https://github.com/sufficit/sufficit-ami-events)** - Business-specific AMI integration service
- **[Sufficit.Asterisk.Utils](https://github.com/sufficit/sufficit-asterisk-utils)** - Asterisk utility functions and helpers
- **[Sufficit.Asterisk.Shared](https://github.com/sufficit/sufficit-asterisk-shared)** - Shared components and extensions

---

**Made with ❤️ by the Sufficit Team**