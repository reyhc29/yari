# Yari
A JSON Middleware for Database Development

## When to use Yari?

* If you are a fan of relational database development and want to call all your stored procedures from your client side
* If you are writing and SPA application and do not want to worry about writing and http service

## How to use Yari in your ASP.NET .net core application?

In your Startup.cs file:

```csharp
using Yari.MySql;

public void ConfigureServices(IServiceCollection services)
{
    ...
    
    services.AddYari(options => options.UserMySql(Configuration.GetConnectionString("Default")));
}
```

In your controllers:

```csharp
private readonly ActionManager actionManager;

public YariController(ActionManager actionManager)
{
    this.actionManager = actionManager;
}

[HttpPost]
public IActionResult Post([FromBody]JObject content)
{
    try
    {
        ActionDescriptor actionDescritor = new ActionDescriptor(content);

        JObject result = actionManager.Execute(actionDescritor);

        return new OkObjectResult(result);
    }
    catch (Exception ex)
    {
        return new BadRequestObjectResult(ex.Message);
    }
}
```


## Why Yari?

It simply was about time! 

