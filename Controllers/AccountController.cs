using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTos;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(
            UserManager<AppUser> userManager,
             SignInManager<AppUser> signInManager,
             ITokenService tokenService,
             IMapper mapper)
        {
            _mapper = mapper;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _tokenService = tokenService;

        }
        //[fromBody] is not necessary as we get [ApiController in the decorated of the controller]
        [HttpPost("register")]

        public async Task<ActionResult<userDto>> Register(RegisterDTo regitsreDto)
        {
            //see below
            if (await userExist(regitsreDto.username)) return BadRequest("username is taken");

            var user = _mapper.Map<AppUser>(regitsreDto);
            //Disposing the hmac with using so we dispose it after it finishes
            //username must be string to be hashed so we have to make DTAs , object that holds the same types from the front end
            //and  assign it to  the main class AppUser

            user.UserName = regitsreDto.username.ToLower();


            var result = await userManager.CreateAsync(user, regitsreDto.password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            var RoleResult = await userManager.AddToRoleAsync(user, "Member");
            if (!RoleResult.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return new userDto
            {
                userName = user.UserName,
                token = await _tokenService.CreateToken(user),
                knownas = user.KnownAs,
                Gender = user.Gender
            };
        }
        //login
        [HttpPost("login")]
        public async Task<ActionResult<userDto>> login(loginDto loginDto)
        {
            var user = await userManager.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == loginDto.username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.password, false);

            if (!result.Succeeded) return Unauthorized();

            return new userDto
            {
                userName = user.UserName,
                token = await _tokenService.CreateToken(user),
                photoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownas = user.KnownAs,
                Gender = user.Gender
            };
        }

        //search for exist username
        private async Task<bool> userExist(string username)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }


    }
}