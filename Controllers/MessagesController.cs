using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;
using API.extensions;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetuserName();
            if (username == createMessageDto.RecipientUserName.ToLower())
            {
                return BadRequest("You can not send message to yourself");
            }
            var Sender = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(username);

            var receipient = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(createMessageDto.RecipientUserName);

            if (receipient == null)
            {
                return NotFound();
            }
            var message = new message
            {
                Sender = Sender,
                Recipient = receipient,
                SenderUserName = Sender.UserName,
                RecipientUserName = receipient.UserName,
                content = createMessageDto.content
            };
            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }
            return BadRequest("Failed to send the messsage");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetuserName();
            var Messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(Messages.CurrentPage, Messages.PageSize, Messages.TotalCount, Messages.TotalPages);
            return Messages;
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetuserName();
            var message = await _unitOfWork.MessageRepository.Getmessage(id);

            if (message.Sender.UserName != username && message.Recipient.UserName != username)
            {
                return Unauthorized();
            }

            if (message.Sender.UserName == username)
            {
                message.senderDeleted = true;
            }
            if (message.Recipient.UserName == username)
            {
                message.RecipientDeleted = true;
            }

            if (message.RecipientDeleted && message.senderDeleted)
            {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }
            if (await _unitOfWork.Complete())
            {
                return Ok();
            }
            return BadRequest("problem deleting the Message");
        }
    }
}