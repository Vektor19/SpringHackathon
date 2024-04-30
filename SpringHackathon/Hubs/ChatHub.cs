using Microsoft.AspNetCore.SignalR;

namespace SpringHackathon.Hubs
{
    /// <summary>
    /// ChatHub class for handling chat messages
    /// </summary>
    public class ChatHub : Hub
    {
        /// <summary>
        /// Sends a message to all clients
        /// </summary>
        /// <param name="user">The user sending the message</param>
        /// <param name="message">The message to send</param>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
