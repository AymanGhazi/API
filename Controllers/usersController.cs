using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTos;
using API.Entities;
using API.extensions;
using API.Helpers;
using API.interfaces;
using API.services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Controllers
{
    [Authorize]
    public class usersController : BaseApiController
    {


        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;

        public usersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        //add endPoints
        [HttpGet]
        public async Task<ActionResult<pageList<MemberDto>>> GetUsers([FromQuery] UserParams userParams) //get list of users
        {
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetuserName());

            userParams.currentUserName = User.GetuserName();

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            //if null update the gender params

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            // var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users);
        }


        //api/users/3
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) //get list of users
        {
            var cuurrentUsername = User.GetuserName();
            return await _unitOfWork.UserRepository.GetMemberAsync(username, cuurrentUsername == username);
        }

        /// <summary>
        /// update into the database via the token which we get by the username
        /// </summary>
        /// <param name="memberUpdateDto"></param>
        /// <returns>NoContent or BadRequest</returns>
        [HttpPut]
        public async Task<ActionResult> updateUser(memberUpdateDto memberUpdateDto)
        {
            var user = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(User.GetuserName());
            //mapping the param into the user
            _mapper.Map(memberUpdateDto, user);
            _unitOfWork.UserRepository.update(user);
            if (await _unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(User.GetuserName());
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
           
            user.Photos.Add(photo);
            if (await _unitOfWork.Complete())
            {
                // return CreatedAtRoute("GetUser", _mapper.Map<PhotoDto>(photo));
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> setMainPhoto(int photoid)
        {
            var user = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(User.GetuserName());
            var photo = user.Photos.FirstOrDefault(x => x.id == photoid);

            if (photo.IsMain) return BadRequest("This is Already you main photo");
            var currentPhoto = user.Photos.FirstOrDefault(e => e.IsMain == true);
            if (currentPhoto != null) currentPhoto.IsMain = false;
            photo.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("failed to set the new Photo");

        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserbyUserNameAsync(User.GetuserName());
            var photo = user.Photos.FirstOrDefault(x => x.id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) { return BadRequest("You can not Delete your main Photo"); }

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error);
                }

            }
            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("failed to delete the photo");
        }
    }
}