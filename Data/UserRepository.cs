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
    public class UserRepository : IuserRepository
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users

            .Include(p => p.Photos)
            .ToListAsync();
        }

        public async Task<AppUser> GetUserbyIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        //to make the id is the token provider
        //1- in token service
        //2- in claims principle extensions
        public async Task<AppUser> GetUserbyUserNameAsync(string UserName)
        {
            return await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == UserName);
        }


        public void update(AppUser user)
        {
            //let him see the changes
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<pageList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var Query = _context.Users.AsQueryable();

            Query = Query.Where(u => u.UserName != userParams.currentUserName);

            Query = Query.Where(u => u.Gender == userParams.Gender);

            var MinDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var MaxDob = DateTime.Today.AddYears(-userParams.MinAge);

            Query = Query.Where(u => u.DateOfBirth >= MinDob && u.DateOfBirth <= MaxDob);


            Query = userParams.orderBy switch
            {
                "created" => Query.OrderByDescending(u => u.Created),
                _ => Query.OrderByDescending(u => u.LastActive),
            };

            return await pageList<MemberDto>.CreateAsync(Query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<MemberDto> GetMemberAsync(string username, bool IsCurrentUser)
        {


            var Query = _context.Users.Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
            if (IsCurrentUser)
            {
                Query = Query.IgnoreQueryFilters();
            }
            return await Query.FirstOrDefaultAsync();
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(x => x.UserName == username)
              .Select(x => x.Gender)
              .FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetUserByPhotoId(int PhotoId)
        {
            return await _context.Users
            .Include(p => p.Photos)
            .IgnoreQueryFilters()
            .Where(p => p.Photos.Any(p => p.id == PhotoId))
            .FirstOrDefaultAsync();
        }
    }
}