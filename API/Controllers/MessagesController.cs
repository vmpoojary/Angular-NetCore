
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController:BaseApiController
    {
        public IMapper _mapper { get; }
        private readonly IUnitOfWork _unitOfWork;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork){
            _unitOfWork = unitOfWork;
            _mapper=mapper;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username=User.GetUserName();
            if(username==createMessageDTO.RecipientUsername.ToLower()){
                return BadRequest("You cannot send messages to yourself");
            }

            var sender=await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient=await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

            if(recipient==null)
            {
                return NotFound();
            }

            var message=new Message{
                Sender=sender,
                Recipient=recipient,
                SenderUsername=sender.UserName,
                RecipientUsername=recipient.UserName,
                Content=createMessageDTO.Content
            };
            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDto>(message));
       

            return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            messageParams.Username=User.GetUserName();
            var messages=await _unitOfWork.MessageRepository.GetMessageForUser(messageParams);
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,
                                                            messages.PageSize,messages.TotalCount,messages.TotalPages
                                                            ));
            
            return messages;
        }

        // [HttpGet("thread/{username}")]
        // public async Task<ActionResult<IEnumerable<MemberDTO>>> GetMessageThread(string username)
        // {
        //     var currentUserName=User.GetUserName();
        //     return Ok(await _unitOfWork.MessageRepository.GetMessageThread(currentUserName,username));
        // }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username=User.GetUserName();
            var message=await _unitOfWork.MessageRepository.GetMessage(id);

            if(message.SenderUsername != username && message.RecipientUsername!=username)
                return Unauthorized();

            if(message.SenderUsername==username) message.SenderDeleted=true;
            if(message.RecipientUsername==username) message.RecipientDeleted=true;

            if(message.SenderDeleted && message.RecipientDeleted){
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }


}