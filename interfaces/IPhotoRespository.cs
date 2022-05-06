using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;

namespace API.interfaces
{
    public interface IPhotoRespository
    {
        Task<IEnumerable<PhotoForApprovalDto>> GetUnApprovedPhotos();

        Task<Photo> GetPhotoByID(int PhotoID);
        void RemovePhoto(Photo Photo);


    }
}