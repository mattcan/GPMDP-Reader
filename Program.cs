using System;
using System.IO;
using gpmdp_rdr.Providers;

namespace gpmdp_rdr
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO this might be wrong when its compiled to an executable
            if(args.Length != 2) {
                Console.WriteLine($"Require 2 arguments, received {args.Length}");
                Console.WriteLine("Usage: obs-gpmdp <json-store-directory> <write-to>");
                return;
            }

            JsonApi w = new JsonApi(args[0]);
            w.Start();
        }
    }
}
