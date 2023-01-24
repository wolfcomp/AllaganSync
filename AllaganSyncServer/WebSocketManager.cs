using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace AllaganSyncServer
{
    public class WebSocketManager
    {
        private static ConcurrentDictionary<string, WebSocket> sockets = new();

        public WebSocket GetSocketById(string id)
        {
            return sockets.GetValueOrDefault(id);
        }

        public List<WebSocket> GetAll()
        {
            return sockets.Values.ToList();
        }

        public string GetId(WebSocket socket)
        {
            return sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void AddSocket(string key, WebSocket socket)
        {
            sockets.TryAdd(key, socket);
        }

        public async Task RemoveSocket(string id)
        {
            sockets.TryRemove(id, out var socket);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by the WebSocketManager",
                                    cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string id, string message)
        {
            if (!sockets.TryGetValue(id, out var socket))
            {
                return;
            }

            if (socket.State != WebSocketState.Open)
            {
                return;
            }

            await socket.SendText(message);
        }

        public async Task SendBinaryMessageAsync(string id, byte[] message)
        {
            if (!sockets.TryGetValue(id, out var socket))
            {
                return;
            }

            if (socket.State != WebSocketState.Open)
            {
                return;
            }

            await socket.SendBinary(message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(pair.Key, message);
                }
            }
        }

        public async Task SendBinaryMessageToAllAsync(byte[] message)
        {
            foreach (var pair in sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendBinaryMessageAsync(pair.Key, message);
                }
            }
        }
    }

    public static class WebSocketExtensions
    {
        public static async Task SendText(this WebSocket socket, string data)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task SendBinary(this WebSocket socket, byte[] data)
        {
            var segment = new ArraySegment<byte>(data);
            await socket.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public static async Task<byte[]> RecieveBytes(this WebSocket socket)
        {
            var buffer = new byte[4096];
            var segment = new ArraySegment<byte>(buffer);
            var result = await socket.ReceiveAsync(segment, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                return Array.Empty<byte>();
            }

            var count = result.Count;
            while (!result.EndOfMessage)
            {
                if (count >= buffer.Length)
                {
                    var newBuffer = new byte[buffer.Length * 2];
                    buffer.CopyTo(newBuffer, 0);
                    buffer = newBuffer;
                }

                segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                result = await socket.ReceiveAsync(segment, CancellationToken.None);
                count += result.Count;
            }

            var outputBuffer = new byte[count];
            Array.Copy(buffer, outputBuffer, count);
            return outputBuffer;
        }

        public static async Task<string> ReceiveString(this WebSocket socket)
        {
            var buffer = await socket.RecieveBytes();
            return Encoding.UTF8.GetString(buffer);
        }

        public static bool IsOpen(this WebSocket socket)
        {
            return socket.State == WebSocketState.Open;
        }
    }
}
