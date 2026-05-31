using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrasturcture.Hubs
{
    public class CodeHub : Hub
    {
        public async Task SendCode(int meetingId, string code)
        {
            await Clients.OthersInGroup(meetingId.ToString()).SendAsync("ReceiveCode", code);
        }
        public async Task SendLanguage(int meetingId, string language)
        {
            await Clients.OthersInGroup(meetingId.ToString()).SendAsync("ReceiveLanguage", language);
        }

        public async Task JoinMeetingRoom(int meetingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, meetingId.ToString());
        }

        public async Task LeaveMeetingRoom(int meetingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId.ToString());
        }

        public async Task SendCodeOutput(string roomId, string output)
        {
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveCodeOutput", output);
        }
    }
}