﻿using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace API.Controllers
{
	public class AccountController : BaseApiController
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public AccountController(IMapper mapper,
								 UserManager<AppUser> userManager,
								 SignInManager<AppUser> signInManager) : base(mapper)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpGet]
		[Authorize]
		public async Task<ActionResult<UserDto>> GetCurrentUserAsync()
		{
			AppUser user = await _userManager.FindByEmailFromClaimsPrinciple(HttpContext.User);

			return Map<UserDto>(user);
		}

		[HttpGet]
		[Route("emailexists")]
		public async Task<ActionResult<bool>> CheckEmailExistAsync([FromQuery] string email)
		{
			return await _userManager.FindByNameAsync(email) != null;
		}

		[HttpGet]
		[Authorize]
		[Route("address")]
		public async Task<ActionResult<AddressDto>> GetUserAddressAsync()
		{
			AppUser user = await _userManager.FinByClaimsPrincipleWithAddressAsync(HttpContext.User);

			return Map<AddressDto>(user.Address);
		}

		[HttpPut]
		[Authorize]
		[Route("address")]
		public async Task<ActionResult<AddressDto>> UpdateAddressAsync([FromBody] AddressDto addressDto)
		{
			AppUser user = await _userManager.FinByClaimsPrincipleWithAddressAsync(HttpContext.User);

			user.Address = Map<Address>(addressDto);

			IdentityResult result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				return BadRequest("Problem updateing the user");
			}

			return Map<AddressDto>(user.Address);
		}

		[HttpPost]
		[Route("login")]
		public async Task<ActionResult<UserDto>> LoginAsync([FromBody] LoginDto loginDto)
		{
			AppUser user = await _userManager.FindByEmailAsync(loginDto.Email);

			if (user == null)
			{
				return Unauthorized(new ApiResponse(401));
			}

			SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

			if (!result.Succeeded)
			{
				return Unauthorized(new ApiResponse(401));
			}

			return Map<UserDto>(user);
		}

		[HttpPost]
		[Route("register")]
		public async Task<ActionResult<UserDto>> RegisterAsync([FromBody] RegisterDto registerDto)
		{
			if (CheckEmailExistAsync(registerDto.Email).Result.Value)
			{
				return new BadRequestObjectResult(new ApiValidationErrorResponse(new [] {"Email address is in use"}));
			}

			AppUser user = Map<AppUser>(registerDto);

			IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);

			if (!result.Succeeded)
			{
				return BadRequest(new ApiResponse(400));
			}

			return Map<UserDto>(user);
		}
	}
}
