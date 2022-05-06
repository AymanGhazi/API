using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class presenceHub : Hub
    {
        private readonly presenceTracker _tracker;
        public presenceHub(presenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var IsOnline = await _tracker.UserConnected(Context.

            User.GetuserName(), Context.ConnectionId);
            if (IsOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetuserName());
            }



            var currentusers = await _tracker.GetOnlineUsers();

            await Clients.Caller.SendAsync("GetOnlineUsers", currentusers);


        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var IsOnline = await _tracker.userDisConnected(Context.User.GetuserName(), Context.ConnectionId);
            if (IsOnline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetuserName());
            }
            await base.OnDisconnectedAsync(exception);

        }

    }
}