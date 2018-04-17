using gpmdp_rdr.Providers;
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
            // TODO this might be wrong when its compiled to an executable
            if(args.Length != 2) {
                Console.WriteLine($"Require 2 arguments, received {args.Length}");
                Console.WriteLine("Usage: obs-gpmdp <json-store-directory> <write-to>");
                return;
            }

            List<ProviderEntry> providers = new List<ProviderEntry>() {
                new ProviderEntry() {
                    Provider = new JsonApi(args[0]),
                    Weight = 10
                },
                new ProviderEntry() {
                    Provider = await WebsocketApi.CreateWebsocketApi(),
                    Weight = 100
                }
            };

            IProvider provider = null;

            try {
                provider = providers
                    .Where(entry => entry.Provider.IsUseable())
                    .OrderBy(entry => entry.Weight)
                    .First()
                    .Provider;
            } catch (Exception e) {
                Logger.Debug($"Error: {e.Message}");
                Console.WriteLine("No working API, please enable one in Google Play Music Desktop Player");
                Program.ExitWith(ExitCode.NO_WORKING_PROVIDER);
            }

            await provider.Start(args[1]);

            Console.WriteLine("Press \'q\' to quit the sample.");
            while(Console.Read()!='q');
        }

        public static void ExitWith(ExitCode Code) {
            Environment.Exit((int) Code);
        }
    }
}
