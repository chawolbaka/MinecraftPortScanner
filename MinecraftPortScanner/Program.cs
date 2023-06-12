using System.CommandLine;
using System.Net;

namespace MinecraftPortScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand("A minecraft port scanner");


            var ipOption = new Option<IPAddress>(
                name: "-ip",
                description: "server ip");
            ipOption.IsRequired = true;

            var startPortOption = new Option<ushort>(
                aliases: new string[] { "-s", "--port-start" },
                description: "start port",
                getDefaultValue: () => 1000);

            var endPortOption = new Option<ushort>(
                aliases: new string[] { "-e", "--port-end" },
                description: "end port",
                getDefaultValue: () => ushort.MaxValue);

            var intervalOption = new Option<int>(
                aliases: new string[] { "-i", "--interval" },
                description: "scan interval",
                getDefaultValue: () => 0);

            var timeoutOption = new Option<int>(
                aliases: new string[] { "-t", "--timeout" },
                description: "tcp connect timeout",
                getDefaultValue: () => 100);

            rootCommand.AddOption(ipOption);
            rootCommand.AddOption(startPortOption);
            rootCommand.AddOption(endPortOption);
            rootCommand.AddOption(intervalOption);
            rootCommand.AddOption(timeoutOption);
            rootCommand.SetHandler(async (ip, start, end, interval, timeout) =>
            {
                FastScanner scanner = new FastScanner(ip);
                for (ushort port = start; port < end; port++)
                {
                    Console.CursorLeft = 0;
                    Console.Write(port);
                    if (await scanner.Scan(port, timeout))
                    {
                        Console.CursorLeft = 0;
                        Console.WriteLine($"Discovery Port: {port}");
                    }
                        
                }
            }, ipOption, startPortOption, endPortOption, intervalOption, timeoutOption);
            rootCommand.Invoke(args);
        }
    }
}