# Yari
A JSON Middleware for Database Development

# How to use Yari in your ASP.NET .net core application?

In your Startup.cs file:

using Yari.MySql;

public void ConfigureServices(IServiceCollection services)
{
    ...
    
    services.AddYari(options => options.UserMySql(Configuration.GetConnectionString("Default")));
}

In your controllers:

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

# When to use Yari?

It's simple way to put the power of a relational database in the hands of SPA developers.  

# Why Yari?

It simply was about time! 

