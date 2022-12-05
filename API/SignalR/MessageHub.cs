
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub :Hub
    {
        private readonly IMapper _mapper;

        private readonly IUnitOfWork _uow;
        public MessageHub(IUnitOfWork uow,
                            IMapper mapper
        )  {
             _mapper = mapper;
             _uow=uow;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext=Context.GetHttpContext();
            var otherUser=httpContext.Request.Query["user"];
            var groupName=GetGroupName(Context.User.GetUserName(),otherUser);
             
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);
            await AddToGroup(groupName);

            var messages=await _uow.MessageRepository.GetMessageThread(Context.User.GetUserName(),otherUser);

            if(_uow.HasChanges()) await _uow.Complete();

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread",messages);

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller,string other)
        {
            var stringCompare=string.CompareOrdinal(caller,other)<0;

            return stringCompare? $"{caller}-{other}" : $"{other}-{caller}";

        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var username=Context.User.GetUserName();
            if(username==createMessageDTO.RecipientUsername.ToLower()){
                 throw new HubException("You cannot send messages to yourself");
            }

            var sender=await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient=await _uow.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

            if(recipient==null)
            {
                throw new HubException("Not found user");
            }

            var message=new Message{
                Sender=sender,
                Recipient=recipient,
                SenderUsername=sender.UserName,
                RecipientUsername=recipient.UserName,
                Content=createMessageDTO.Content
            };

            var groupName=GetGroupName(sender.UserName,recipient.UserName);
            var group=await _uow.MessageRepository.GetMessageGroup(groupName);

            if(group.Connections.Any(x=>x.Username==recipient.UserName)) 
            {
                message.DateRead=DateTime.UtcNow;
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                
                    await Clients.Group(groupName).SendAsync("New Message",_mapper.Map<MessageDto>(message));
            }
                
       

            
        }

        private async Task<bool> AddToGroup(string groupName){
            var group=await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection=new Connection(Context.ConnectionId,Context.User.GetUserName());

            if(group==null)
            {
                group=new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            return await _uow.Complete();
        }

        private async Task RemoveFromMessageGroup(){
            var connection=await _uow.MessageRepository.GetConnection(Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);
            await _uow.Complete();
        }
    }
}