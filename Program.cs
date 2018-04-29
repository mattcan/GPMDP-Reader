using gpmdp_rdr.Providers;
using EntryPoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gpmdp_rdr
{
    struct ProviderEntry {
        public IProvider Provider;
        public int Weight;
    };

    class Program
    {
        static async Task Main(string[] args)
        {
            var options = Cli.Parse<CliArguments>(args);
            var logger = new Logger(options.DebugMode);

            List<ProviderEntry> providers = new List<ProviderEntry>() {
                new ProviderEntry() {
                    Provider = new JsonApi(options.JsonApiPath, logger),
                    Weight = 10
                },
                new ProviderEntry() {
                    Provider = await WebsocketApi.CreateWebsocketApi(logger),
                    Weight = 100
                }
            };

            IProvider provider = null;

            try {
                provider = providers
                    .Where(entry => entry.Provider.IsUseable())
                    .OrderByDescending(entry => entry.Weight)
                    .First()
                    .Provider;
            } catch (Exception e) {
                logger.Debug($"Error: {e.Message}");
                Console.WriteLine("No working API, please enable one in Google Play Music Desktop Player");
                Program.ExitWith(ExitCode.NO_WORKING_PROVIDER);
            }

            provider.Start(options.SavePath);

            Console.WriteLine("Press \'q\' to quit the sample.");
            while(Console.Read()!='q');
        }

        public static void ExitWith(ExitCode Code) {
            Environment.Exit((int) Code);
        }
    }
}
