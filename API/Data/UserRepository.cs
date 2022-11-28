using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context,IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDTO> GetMemberAsync(string Username)
        {
            return await _context.Users
            .Where(x=>x.UserName==Username)
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync()
            ;
        }

          public async  Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var query=_context.Users.AsQueryable();
            query=query.Where(u=>u.UserName!=userParams.CurrentUsername);
            query=query.Where(u=>u.Gender==userParams.Gender);
            query=userParams.OrderBy switch{
                "created"=>query.OrderByDescending(u=>u.Created),
                _=>query.OrderByDescending(u=>u.LastActive)
            };

            var minDob=DateTime.Now.AddYears(-userParams.MaxAge-1);
            var maxDod=DateTime.Now.AddYears(-userParams.MinAge);

        query=query.Where(u=>u.DateOfBirth>=minDob && u.DateOfBirth<=maxDod);

            return await PagedList<MemberDTO>.CreateAsync(query.AsNoTracking().ProjectTo<MemberDTO>(_mapper.ConfigurationProvider),
            userParams.PageNumber,userParams.PageSize);
        }

   
   

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State=EntityState.Modified;
        }

    
    }
}