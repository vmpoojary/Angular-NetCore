using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController:BaseApiController
    {
    private  readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService ;

        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager,ITokenService tokenService,
                                IMapper mapper
                             )
        {
            _tokenService = tokenService;
            _userManager=userManager;
            _mapper=mapper;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await UserExists(registerDTO.UserName))
            {
                return BadRequest("Username is taken");
            }

            var user=_mapper.Map<AppUser>(registerDTO);

        
            
                user.UserName=registerDTO.UserName;
        

            var result=await _userManager.CreateAsync(user,registerDTO.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult=await _userManager.AddToRoleAsync(user,"Member");

            if(!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDTO{
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user),
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user=await _userManager.Users
            .Include(p=>p.Photos)  
            .SingleOrDefaultAsync(x=>x.UserName==loginDTO.Username); 
            if(user==null) return Unauthorized("Invalid username");

            var result=await _userManager.CheckPasswordAsync(user,loginDTO.Password);

            if(!result) return Unauthorized("Invalid Password");


            return   new UserDTO{
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user),
                PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x=>x.UserName==username.ToLower());
        }
    }
}