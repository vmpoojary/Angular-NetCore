using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        
        
        private readonly IUnitOfWork _uow;

        public IPhotoService _photoService;
        public UsersController(IUnitOfWork uow, IMapper mapper,IPhotoService photoService)
        {
            _uow = uow;
            _mapper = mapper;
            _photoService=photoService;
        }

        [HttpGet]
        
        public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers( [FromQuery]UserParams userParams)
        {
            var gender = await _uow.UserRepository.GetUserGender(User.GetUserName());
            userParams.CurrentUsername=User.GetUserName();

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender=gender=="male"?"female":"male";
            }
            


            var users=await  _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount
            ,users.TotalPages));
            return  Ok(users);
            
        }

         [HttpGet("{username}",Name="GetUser")]
         
        public async Task<ActionResult<MemberDTO>> GetUsers(string username)
        {
            return  await _uow.UserRepository.GetMemberAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());

            _mapper.Map(memberUpdateDTO, user);

            _uow.UserRepository.Update(user);

            if (await _uow.Complete()) return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public  async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());
            var result=await _photoService.AddPhotoAsync(file);
            if(result.Error!=null){
                return BadRequest(result.Error.Message);
            }

            var photo=new Photo{
                Url=result.SecureUrl.AbsoluteUri,
                PublicId=result.PublicId
            };
            if(user.Photos.Count==0)
            {
                photo.IsMain=true;
            }

            user.Photos.Add(photo);
            if(await _uow.Complete())
            {
                //   return _mapper.Map<PhotoDTO>(photo);
                return CreatedAtRoute("GetUser",new{username=user.UserName},_mapper.Map<PhotoDTO>(photo));
            }
          

            return BadRequest("Probelem adding photos");
        }

        [HttpPut("set-main-phpto/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user=await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());
            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo.IsMain) return BadRequest("This is already your main photo");
            var currentMain=user.Photos.FirstOrDefault(x=>x.IsMain);
            if(currentMain!=null) currentMain.IsMain=false;
            photo.IsMain=true;

            if(await _uow.Complete()) return NoContent();
            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user=await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());
            var photo   =user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo==null) return NotFound();
            if(photo.IsMain) return BadRequest("You cannot delete your main photo");
            if(photo.PublicId!=null)
            {
                var result=await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error!=null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo) ;
            if(await _uow.Complete()) return Ok();
            return BadRequest("Failed to delete photo");
        }

    }
}