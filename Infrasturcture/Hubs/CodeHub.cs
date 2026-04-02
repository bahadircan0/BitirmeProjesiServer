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

        // Kullanıcı toplantı odasına katılınca gruba ekle
        public async Task JoinMeetingRoom(int meetingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, meetingId.ToString());
        }

        // Kullanıcı ayrılınca gruptan çıkar
        public async Task LeaveMeetingRoom(int meetingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId.ToString());
        }
    }
}
