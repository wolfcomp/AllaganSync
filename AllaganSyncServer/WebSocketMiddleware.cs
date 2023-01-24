using System.Net.WebSockets;
using System.Runtime.InteropServices;

namespace AllaganSyncServer
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly WebSocketManager socketManager;
        private const byte[] helloMessage = new []{ };
        public WebSocketMiddleware(RequestDelegate next, WebSocketManager socketManager)
        {
            this.next = next;
            this.socketManager = socketManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await next.Invoke(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await socket.SendBinary(helloMessage);

            await Receive(socket, bytes =>
            {
                
            });
        }

        private async Task Receive(WebSocket socket, Action<byte[]> handleMessage)
        {
            while (socket.IsOpen())
            {
                handleMessage(await socket.RecieveBytes());
            }
        }
    }
}
