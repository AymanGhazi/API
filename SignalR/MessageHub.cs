using System;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTos;
using API.Entities;
using API.extensions;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHubContext<presenceHub> _presenceHub;
        private readonly presenceTracker _tracker;
        public MessageHub(IMapper mapper, IHubContext<presenceHub> presenceHub, presenceTracker tracker, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _tracker = tracker;
            _presenceHub = presenceHub;
            _mapper = mapper;
        }
        public override async Task OnConnectedAsync()
        {
            //Get the context 
            var httpcontext = Context.GetHttpContext();
            //Get the Other user Connected
            var otheruser = httpcontext.Request.Query["user"].ToString();

            var GroupName = GetGroupName(httpcontext.User.GetuserName(), otheruser);

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            //add to Group
            var Group = await AddToGroup(GroupName);
            await Clients.Group(GroupName).SendAsync("UpdatedGroup", Group);
            //Get Messages Thread 
            var Messages = await _unitOfWork.MessageRepository.getMessageThread(httpcontext.User.GetuserName(), otheruser);
            if (_unitOfWork.hasChanges())
            {
                await _unitOfWork.Complete();
            }
            //Send the messages on the hub
            await Clients.Caller.SendAsync("ReceiveMessageThread", Messages);
            //send to the Online User

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var Group = await RemoveFromMessageGroup();
            await Clients.Group(Group.name).SendAsync("UpdatedGroup", Group);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetuserName();
            if (username == createMessageDto.RecipientUserName.ToLower())
            {
                throw new HubException("You can not send message to yourself");
            }
            var Sender = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(username);

            var receipient = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(createMessageDto.RecipientUserName);

            if (receipient == null)
            {
                throw new HubException("Not Found User");
            }
            var message = new message
            {
                Sender = Sender,
                Recipient = receipient,
                SenderUserName = Sender.UserName,
                RecipientUserName = receipient.UserName,
                content = createMessageDto.content
            };
            var groupName = GetGroupName(Sender.UserName, receipient.UserName);

            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            //mark the message read
            if (group.Connections.Any(x => x.Username == receipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }

            var connections = await _tracker.GetConnectionsForUser(receipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { username = Sender.UserName, knownas = Sender.KnownAs });

            }

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                var Groupname = GetGroupName(Sender.UserName, receipient.UserName);

                await Clients.Group(Groupname).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }
        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            var Connection = new Connection(Context.ConnectionId, Context.User.GetuserName());
            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(Connection);
            if (await _unitOfWork.Complete()) return group;
            throw new HubException("Failed to add to group");

        }

        private async Task<Group> RemoveFromMessageGroup()
        {

            var Group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var Connection = Group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            _unitOfWork.MessageRepository.RemoveConnection(Connection);
            if (await _unitOfWork.Complete()) return Group;
            throw new HubException("Failed to remove From Group");

        }
        public string GetGroupName(string caller, string otherUser)
        {
            var stringCompare = string.CompareOrdinal(caller, otherUser) < 0;
            return stringCompare ? $"{caller}-${otherUser}" : $"{otherUser}-${caller}";

        }
    }
}