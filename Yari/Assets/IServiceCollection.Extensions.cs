using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Yari.Exceptions;

namespace Yari
{
    public static class IServiceCollectionExtensions
    {
        public static void AddYari(this IServiceCollection services, Action<YariOptions> optionsAction)
        {
            YariOptions options = new YariOptions();

            optionsAction.Invoke(options);
           
            services.AddSingleton<ActionManager>((serviceProvider) =>
            {
                options.dbActionExecuter.onLog = options.OnLog;

                ActionManager actionManager = new ActionManager();
                actionManager.dbActionExecuter = options.dbActionExecuter;

                actionManager.onLog = options.OnLog;

                return actionManager;
            });
        }
    }
}
