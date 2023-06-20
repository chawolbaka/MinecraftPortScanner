using System;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.CommandLine;
using MinecraftProtocol.Utils;

namespace MinecraftPortScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand("A minecraft port scanner");

            var ipOption = new Option<string>(
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
                IPAddress address;
                if (string.IsNullOrWhiteSpace(ip))
                {
                    while (true)
                    {
                        Console.Write("IP: ");
                        string input = Console.ReadLine();
                        try
                        {
                            address = (await NetworkUtils.GetIPEndPointAsync(input)).Address;
                            break;
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine(Messages.InputUnavailable);
                        }
                    }
                }
                else
                {
                    address = (await NetworkUtils.GetIPEndPointAsync(ip)).Address;
                }

                Scanner scanner = fast ? new FastScanner(address) : new FullScanner(address);
                if (timeout < 0)
                {
                    Console.WriteLine(Messages.ICMP);
                    Ping ping = new Ping();
                    PingReply replay = ping.Send(address, 2000);
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
                    Console.Write($"{address}:{port}");
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