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
                description: Messages.IP);

            var startPortOption = new Option<ushort>(
                aliases: new string[] { "-s", "--port-start" },
                description: Messages.StartPort,
                getDefaultValue: () => 1000);

            var endPortOption = new Option<ushort>(
                aliases: new string[] { "-e", "--port-end" },
                description: Messages.EndPort,
                getDefaultValue: () => ushort.MaxValue);

            var intervalOption = new Option<int>(
                aliases: new string[] { "-i", "--interval" },
                description: Messages.ScanInterval,
                getDefaultValue: () => 0);

            var timeoutOption = new Option<int>(
                aliases: new string[] { "-t", "--timeout" },
                description: Messages.TcpConnectTimeout,
                getDefaultValue: () => -1);

            var fastscannerOption = new Option<bool>(
                aliases: new string[] { "-f", "--fast" },
                description: Messages.FastScan);

            rootCommand.AddOption(ipOption);
            rootCommand.AddOption(startPortOption);
            rootCommand.AddOption(endPortOption);
            rootCommand.AddOption(intervalOption);
            rootCommand.AddOption(timeoutOption);
            rootCommand.AddOption(fastscannerOption);
            rootCommand.SetHandler(async (ip, start, end, interval, timeout, fast) =>
            {
                while (ip == null)
                {
                    Console.Write("IP: ");
                    string input = Console.ReadLine();
                    if (IPAddress.TryParse(input, out ip))
                        break;
                    else
                        Console.WriteLine(Messages.InputUnavailable);
                }

                Scanner scanner = fast ? new FastScanner(ip) : new FullScanner(ip);
                if (timeout < 0)
                {
                    Console.WriteLine(Messages.ICMP);
                    Ping ping = new Ping();
                    PingReply replay = ping.Send(ip, 2000);
                    if (replay.Status == IPStatus.Success)
                        timeout = (int)replay.RoundtripTime * 4; //3次握手 + 1的波动
                    else
                        timeout = 600;
                    Console.WriteLine($"Timeout is {timeout}");
                }

                Console.WriteLine(Messages.StartPortScanning);
                for (ushort port = start; port < end; port++)
                {
                    Console.CursorLeft = 0;
                    Console.Write(port);
                    if (await scanner.Scan(port, timeout))
                    {
                        Console.CursorLeft = 0;
                        Console.WriteLine($"{Messages.DiscoveryPort}{port}");
                    }
                }
            }, ipOption, startPortOption, endPortOption, intervalOption, timeoutOption, fastscannerOption);
            rootCommand.Invoke(args);
        }
    }
}