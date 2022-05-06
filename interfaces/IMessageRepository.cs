using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;
using API.Helpers;

namespace API.interfaces
{
    public interface IMessageRepository
    {
        void AddGroup(Group Group);
        void RemoveConnection(Connection Connection);
        Task<Connection> GetConnection(string ConnectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupForConnection(string ConnectionId);
        void AddMessage(message message);
        void DeleteMessage(message message);

        Task<message> Getmessage(int id);

        Task<pageList<MessageDto>> GetMessagesForUser(MessageParams messageParams);

        Task<IEnumerable<MessageDto>> getMessageThread(string CurrentUserName, string ReceipientName);


    }
}