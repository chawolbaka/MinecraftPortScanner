using MinecraftProtocol.IO.Extensions;
using MinecraftProtocol.Packets;
using MinecraftProtocol.Packets.Client;
using MinecraftProtocol.Packets.Server;
using MinecraftProtocol.Utils;
using System.Net;
using System.Net.Sockets;

namespace MinecraftPortScanner
{
    public class FullScanner : Scanner
    {
        public FullScanner(IPAddress address) : base(address) { }

        protected override async Task<bool> IsValidAsync(Socket socket, int timeout)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(timeout * 4);
                Packet packet = await ProtocolUtils.ReceivePacketAsync(socket, -1, cts.Token);
                if (!PingResponsePacket.TryRead(packet, -1, out _))
                    return false;

                long code = DateTime.Now.Millisecond;
                byte[] RequestPacket = new PingPacket(code).Pack();

                await NetworkUtils.SendDataAsync(socket, RequestPacket, cts.Token);
                Packet ResponesPacket = await ProtocolUtils.ReceivePacketAsync(socket,-1, cts.Token);

                return ResponesPacket.Id == PongPacket.GetPacketId() && ResponesPacket.AsByteReader().ReadLong() == code;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
