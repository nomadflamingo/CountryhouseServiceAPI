using CountryhouseService.API.Defaults;
using CountryhouseService.API.Dtos;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CountryhouseService.API.Controllers
{
    [Route("api/v1/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }


        // POST account/register
        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync([FromBody] RegisterDto registerDto, string? returnUrl = null)
        {
            // Check that password and password confirm fields are equal
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                ModelState.AddModelError(
                    nameof(registerDto.ConfirmPassword),
                    "Password and confirm password fields don't match");
                return BadRequest(ModelState);
            }

            // Ensure that role is valid
            if (!UserRoleNames.namesArray.Contains(registerDto.Role))
            {
                ModelState.AddModelError(
                    nameof(registerDto.Role),
                    $"Role {registerDto.Role} does not exist");
                return BadRequest(ModelState);
            }

            // Create new user object
            User user = new()
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            // Insert user in database
            var registerResult = await _userManager.CreateAsync(user, registerDto.Password);


            if (registerResult.Succeeded)
            {
                // Add user to role
                await _userManager.AddToRoleAsync(user, registerDto.Role);

                // Assign uploaded avatar
                if (registerDto.AvatarId != null)
                {
                    Avatar? avatar = await _unitOfWork.AvatarsRepository.GetAsync((int)registerDto.AvatarId);
                    if (avatar == null)
                    {
                        ModelState.AddModelError(nameof(registerDto.AvatarId), "Avatar not found");
                        return NotFound(ModelState);
                    }

                    // Check that the avatar isn't already used
                    if (avatar.UserId != null)
                    {
                        ModelState.AddModelError(nameof(registerDto.AvatarId), "Avatar is already in use");
                        return BadRequest(ModelState);
                    };
                    avatar.UserId = user.Id;

                    user.PreviewAvatarSource = avatar.Source;
                }

                // Log in with new credentials if succeeded
                await _signInManager.PasswordSignInAsync(registerDto.Email,
                    registerDto.Password,
                    false,
                    false);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return Ok();
            }
            else  // Process errors
            {
                foreach (var error in registerResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

        }


        // POST account/login
        [HttpPost("login")]
        public async Task<ActionResult> LoginAsync([FromBody] LogInDto logInDto, string? returnUrl = null)
        {
            var result = await _signInManager.PasswordSignInAsync(logInDto.Email,
                                                                  logInDto.Password,
                                                                  false,
                                                                  false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return Ok();
            }
            else
            {
                ModelState.AddModelError(string.Empty,
                    "Invalid email or password. Please, provide valid credentials or create new account");
                return Unauthorized(ModelState);
            }
        }


        // POST account/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
