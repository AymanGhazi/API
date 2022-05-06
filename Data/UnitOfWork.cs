using System.Threading.Tasks;
using API.Date;
using API.interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UnitOfWork(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public IuserRepository UserRepository => new UserRepository(_context, _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

        public ILIkeRepository LikeRepository => new LikesRepository(_context);

        public IPhotoRespository PhotoRespository => new PhotoRepository(_context);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        //saving the changes of tracked data
        public bool hasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }

    }
}