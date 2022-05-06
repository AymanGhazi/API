using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;
using API.Helpers;

namespace API.interfaces
{
    public interface ILIkeRepository
    {
        Task<UserLike> GetUserLike(int SourceUserID, int LikedUserId);
        Task<AppUser> GetUserWithLikes(int UserID);
        Task<pageList<LikeDto>> GetuserLikes(likesparams likesparams);
        Task<bool> removeLike(int SourceUserID, int LikedUserId);
    }
}