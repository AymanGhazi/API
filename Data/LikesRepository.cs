using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTos;
using API.Entities;
using API.extensions;
using API.Helpers;
using API.interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Date
{
    public class LikesRepository : ILIkeRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserLike> GetUserLike(int SourceUserID, int LikedUserId)
        {
            return await _context.Likes.FindAsync(SourceUserID, LikedUserId);
        }
        //return the users I like or users who like me
        public async Task<pageList<LikeDto>> GetuserLikes(likesparams likesparams)
        {
            var Users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if (likesparams.predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserID == likesparams.userID);
                Users = likes.Select(like => like.LikedUser);
            }
            if (likesparams.predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserID == likesparams.userID);
                Users = likes.Select(like => like.sourceUser);
            }
            var likedusers = Users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalcAge(),
                photoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });
            return await pageList<LikeDto>.CreateAsync(likedusers, likesparams.PageNumber, likesparams.PageSize);
        }

        //list of liked users by this user 
        public async Task<AppUser> GetUserWithLikes(int UserID)
        {
            return await _context.Users
            .Include(x => x.UsersILike)
            .FirstOrDefaultAsync(x => x.Id == UserID);
        }
        public async Task<bool> removeLike(int SourceUserID, int LikedUserId)
        {
            var userlike = await _context.Likes.FindAsync(SourceUserID, LikedUserId);

            _context.Likes.Remove(userlike);
            if (_context.SaveChanges() > 0)
            {
                return true;
            }
            return false;
        }
    }
}