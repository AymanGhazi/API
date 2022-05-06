using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;
using API.interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRespository
    {
        private readonly DataContext __Context;
        public PhotoRepository(DataContext _Context)
        {
            __Context = _Context;
        }

        

        public async Task<Photo> GetPhotoByID(int PhotoID)
        {
            return await __Context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.id == PhotoID);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnApprovedPhotos()
        {
            return await __Context.Photos
            .IgnoreQueryFilters()
            .Where(x => x.IsApproved == false)
            .Select(x => new PhotoForApprovalDto
            {
                Id = x.id,
                Url = x.Url,
                Username = x.user.UserName,
                IsApprovedStatus = x.IsApproved
            }).ToListAsync();


        }
        public void RemovePhoto(Photo Photo)
        {
            __Context.Photos.Remove(Photo);
        }
    }
}