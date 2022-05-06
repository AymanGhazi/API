using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTos;
using API.Entities;
using API.interfaces;
using API.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        //we add policy via add authorization service
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
            .Include(r => r.Roles)
            .ThenInclude(r => r.Role)
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                userName = u.UserName,
                roles = u.Roles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();
            return Ok(users);
        }




        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditUserRoles(string username, [FromQuery] string roles)
        {
            var SelectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, SelectedRoles.Except(userRoles));
            if (!result.Succeeded)
            {
                BadRequest("failed to add to Roles");
            }
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(SelectedRoles));
            if (!result.Succeeded)
            {
                BadRequest("Failed To remove from Roles");
            }
            return Ok(await _userManager.GetRolesAsync(user));

        }



        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration()
        {

            var Photos = await _unitOfWork.PhotoRespository.GetUnApprovedPhotos();

            return Ok(Photos);
        }




        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var Photo = await _unitOfWork.PhotoRespository.GetPhotoByID(photoId);
            Photo.IsApproved = true;
            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            if (!user.Photos.Any(x => x.IsMain))
            {
                Photo.IsMain = true;
            }
            if (_unitOfWork.hasChanges())
            {
                await _unitOfWork.Complete();
            }
            return Ok();

        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> rejectPhoto(int photoId)
        {
            var Photo = await _unitOfWork.PhotoRespository.GetPhotoByID(photoId);
            Photo.IsApproved = false;
            if (Photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(Photo.PublicId);
                if (result.Result == "ok")
                {
                    _unitOfWork.PhotoRespository.RemovePhoto(Photo);
                }

            }
            else
            {
                _unitOfWork.PhotoRespository.RemovePhoto(Photo);
            }

            await _unitOfWork.Complete();

            return Ok();

        }


    }
}