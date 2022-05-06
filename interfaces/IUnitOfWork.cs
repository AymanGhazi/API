using System.Threading.Tasks;

namespace API.interfaces
{
    public interface IUnitOfWork
    {
        IuserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        ILIkeRepository LikeRepository { get; }
        IPhotoRespository PhotoRespository { get; }

        Task<bool> Complete();
        bool hasChanges();



    }
}