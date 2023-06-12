using System.CommandLine;
using System.Net;
using System.Net.NetworkInformation;

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
                description: "tcp connect timeout (-1 is auto)",
                getDefaultValue: () => -1);

            rootCommand.AddOption(ipOption);
            rootCommand.AddOption(startPortOption);
            rootCommand.AddOption(endPortOption);
            rootCommand.AddOption(intervalOption);
            rootCommand.AddOption(timeoutOption);
            rootCommand.SetHandler(async (ip, start, end, interval, timeout) =>
            {
                FastScanner scanner = new FastScanner(ip);
                if (timeout < 0)
                {
                    Console.WriteLine("Attempt to retrieve timeout through ICMP.");
                    Ping ping = new Ping();
                    PingReply replay = ping.Send(ip, 2000);
                    if (replay.Status == IPStatus.Success)
                        timeout = (int)replay.RoundtripTime * 5; //3次握手 + 2的波动
                    else
                        timeout = 600;
                    Console.WriteLine($"Timeout is {timeout}");
                }

                Console.WriteLine("Start port scanning.");
                for (ushort port = start; port < end; port++)
                {
                    Console.CursorLeft = 0;
                    Console.Write(port);
                    if (await scanner.Scan(port, timeout))
                    {
                        Console.CursorLeft = 0;
                        Console.WriteLine($"Discovery port: {port}");
                    }
                        
                }
            }, ipOption, startPortOption, endPortOption, intervalOption, timeoutOption);
            rootCommand.Invoke(args);
        }
    }
}