using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public void AddGroup(Group Group)
        {
            _context.Groups.Add(Group);
        }

        public void AddMessage(message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string ConnectionId)
        {
            return await _context.Connections.FindAsync(ConnectionId);
        }

        public async Task<Group> GetGroupForConnection(string ConnectionId)
        {
            return await _context.Groups
            .Include(c => c.Connections)
              .Where(c => c.Connections.Any(x => x.ConnectionId == ConnectionId)).FirstOrDefaultAsync();
        }

        public async Task<message> Getmessage(int id)
        {
            return await _context.Messages.Include(u => u.Sender).Include(u => u.Recipient).SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.name == groupName);
        }

        public async Task<pageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var Query = _context.Messages
            .OrderByDescending(m => m.MessageSent)
            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
            .AsQueryable();

            Query = messageParams.container switch
            {
                "inbox" => Query.Where(u => u.RecipientUserName == messageParams.UserName && u.RecipientDeleted == false),
                "outbox" => Query.Where(u => u.SenderUserName == messageParams.UserName && u.SenderDeleted == false),

                _ => Query.Where(u => u.RecipientUserName == messageParams.UserName && u.DateRead == null && u.RecipientDeleted == false)
            };



            return await pageList<MessageDto>.CreateAsync(Query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> getMessageThread(string CurrentUserName, string ReceipientName)
        {
            var Messages = await _context.Messages
             .Where(
            t => t.Recipient.UserName == CurrentUserName && t.RecipientDeleted == false
             && t.Sender.UserName == ReceipientName
             || t.Recipient.UserName == ReceipientName
             && t.Sender.UserName == CurrentUserName
             && t.senderDeleted == false
             )
             .OrderBy(m => m.MessageSent)
             .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
             .ToListAsync();

            var UnreadMessages = Messages.Where(m => m.DateRead == null && m.RecipientUserName == CurrentUserName).ToList();
            if (UnreadMessages.Any())
            {
                foreach (var message in UnreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }
            return Messages;
        }

        public void RemoveConnection(Connection Connection)
        {
            _context.Connections.Remove(Connection);
        }


    }
}