﻿using AutoMapper;
using Falcon.Server.Features.Auth.DTOs;
using Falcon.Server.Features.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Falcon.Server.Features.Auth.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "Auth")]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AuthController> logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public AuthController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthController> logger, IMapper mapper, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mapper = mapper;
            this.logger = logger;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Creates new user", typeof(UserDTO))]
        [SwaggerOperation(Summary = "Register new user")]
        public async Task<ActionResult<UserToken>> RegisterAsync(UserDTO userDTO)
        {
            var user = mapper.Map<UserDTO, ApplicationUser>(userDTO);
            var result = await userManager.CreateAsync(user, userDTO.Password);
            if (result.Succeeded)
            {
                logger.LogInformation("New user created: {0}", user.UserName);
                return new UserToken(user.UserName, configuration["Jwt:Key"]);
            }
            else
            {
                var errors = result.Errors.ToList();
                return BadRequest(errors[0].Description ?? "Cannot register");
            }
        }

        [HttpPost("login")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User login", typeof(UserDTO))]
        [SwaggerOperation(Summary = "User login")]
        public async Task<ActionResult<UserToken>> LoginAsync(UserDTO userDTO)
        {
            var result = await signInManager.PasswordSignInAsync(userDTO.Username,
                userDTO.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                logger.LogInformation("User logged in: {0}", userDTO.Username);
                return new UserToken(userDTO.Username, configuration["Jwt:Key"]);
            }
            else
            {
                return BadRequest("Username or password invalid");
            }
        }
    }
}