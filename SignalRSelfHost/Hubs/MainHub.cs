namespace SignalRSelfHost.Hubs
{
    using System;

    using Microsoft.AspNet.SignalR;

    public class MainHub : Hub
    {
        public void Send(string name, string message)
        {
            //Console.WriteLine("Context: {0}", Context.User.Identity.FullPath);
            Clients.All.sendMessage(name, message);
        }
    }
}