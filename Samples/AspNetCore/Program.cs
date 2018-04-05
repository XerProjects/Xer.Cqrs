using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspNetCore
{
    class Program
    {
        static void Main(string[] args)
        {                
            BuildWebHost(args).Run();
        }

        static IWebHost BuildWebHost(string[] args)
        {
            // I have prepare multiple Startup classes to demonstrate different ways to register handlers.
            // Check them out at Startup folder. You can specify a different Startup here to test it.
            string startupSelection = args.FirstOrDefault();
            
            // if (string.Equals(startupSelection, "attribute", StringComparison.OrdinalIgnoreCase))
            // {
            //     return WebHost.CreateDefaultBuilder(args.Skip(1).ToArray())
            //                   .UseStartup<StartupWithAttributeRegistration>()
            //                   .Build();
            // }
            if (string.Equals(startupSelection, "container", StringComparison.OrdinalIgnoreCase))
            {
                return WebHost.CreateDefaultBuilder(args.Skip(1).ToArray())
                              .UseStartup<StartupWithContainerRegistration>()
                              .Build();
            }
            else if (string.Equals(startupSelection, "mixed", StringComparison.OrdinalIgnoreCase))
            {
                return WebHost.CreateDefaultBuilder(args.Skip(1).ToArray())
                              .UseStartup<StartupWithMixedRegistration>()
                              .Build();
            }

            // Simple as default.
            return WebHost.CreateDefaultBuilder(args.Skip(1).ToArray())
                            .UseStartup<StartupWithSimpleRegistration>()
                            .Build();
        }
    }
}
