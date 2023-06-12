using MinecraftProtocol.Packets.Client;
using MinecraftProtocol.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPortScanner
{
    public abstract class Scanner
    {
        public virtual IPAddress Address { get; set; }

        private static byte[] PingRequestPacket = new PingRequestPacket().Pack();

        public Scanner(IPAddress address)
        {
            Address = address;
        }

        protected abstract Task<bool> IsValidAsync(Socket socket, int timeout);

        public virtual async Task<bool> Scan(ushort port, int timeout)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(timeout);
                await socket.ConnectAsync(Address, port, cts.Token);
            }
            catch (SocketException) { }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
           

            try
            {
                if (NetworkUtils.CheckConnect(socket))
                {
                    await NetworkUtils.SendDataAsync(socket, new HandshakePacket(Address.ToString(), port, HandshakeState.GetStatus, 2).Pack());
                    await NetworkUtils.SendDataAsync(socket, PingRequestPacket);
                }
            }
            catch (SocketException) { }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }

            bool result = await IsValidAsync(socket, timeout);
            socket.Dispose();
            return result;
        }
    }
}
