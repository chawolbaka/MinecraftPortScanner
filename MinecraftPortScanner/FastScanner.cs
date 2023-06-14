using MinecraftProtocol.Packets;
using MinecraftProtocol.Packets.Server;
using MinecraftProtocol.Utils;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MinecraftPortScanner
{
    public class FastScanner : Scanner
    {
        public FastScanner(IPAddress address) : base(address) { }

        protected override async Task<bool> IsValidAsync(Socket socket, int timeout)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(timeout);
                Packet packet = await ProtocolUtils.ReceivePacketAsync(socket, -1, cts.Token);
                return packet.Id == PingResponsePacket.GetPacketID();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
