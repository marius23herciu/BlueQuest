﻿using BlueQuest.Data;
using BlueQuest.DTOs;
using BlueQuest.Models;
using BlueQuest.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlueQuest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly BlueQuestDbContext _context;

        public AuthController(IConfiguration configuration, IUserService userService, BlueQuestDbContext employeesDbContext)
        {
            _configuration = configuration;
            _userService = userService;
            this._context = employeesDbContext;
        }

        [HttpGet, Authorize]
        [Route("get-name")]
        public ActionResult<string> GetMe()
        {
            var userName = _userService.GetMyName();
            return Ok(userName);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto request)
        {

            //check if username is in the database before creating password
            var checkIfUserExists = await _context.Users.FirstOrDefaultAsync(e => e.Username == request.Username);

            if (checkIfUserExists != null)
            {
                return NotFound("User allready exists.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var usersDepartment = _context.Departments.FirstOrDefaultAsync(d => d.Name == request.Department).Result;

            if (usersDepartment == null)
            {
                usersDepartment = new Department
                {
                    Name = request.Department
                };
                usersDepartment.Users.Add(user);
                await _context.Departments.AddAsync(usersDepartment);
            }
            else
            {
                usersDepartment.Users.Add(user);
            }

            user.Id = 0;
            user.Username = request.Username;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            //add user to Db
            await _context.Users.AddAsync(user);
            //save chages
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticatedResponse>> Login(UserLoginDto request)
        {
            if (request is null)
            {
                return BadRequest("Invalid client request. Insert username and password.");
            }

            var checkUser = await _context.Users.FirstOrDefaultAsync(e => e.Username == request.Username);

            if (checkUser == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, checkUser.PasswordHash, checkUser.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(checkUser);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);

            AuthenticatedResponse response = new AuthenticatedResponse()
            {
                Token = token,
                RefreshToken = refreshToken.Token
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("token-valid-or-not-{token}")]
        public ActionResult<bool> TokenIsValid([FromRoute] string token)
        {
            // token gets the name of the method at the begining, so I'm using stringbuilder to remove the first 12 letters and 2 -
            StringBuilder sb = new StringBuilder();
            var tokenToChars = token.ToCharArray();
            for (int i = 0; i < 5; i++)
            {
                sb.Append(tokenToChars[i]);
            }
            if (sb.ToString() == "token")
            {
                sb.Clear();
                for (int i = 0; i < tokenToChars.Length; i++)
                {
                    if (i > 13)
                    {
                        sb.Append(tokenToChars[i]);
                    }
                }
                token = sb.ToString();
            }


            if (string.IsNullOrEmpty(token))
            {
                return Ok(false);
            }

            var jwtToken = new JwtSecurityToken(token);


            if ((jwtToken == null) || (jwtToken.ValidTo < DateTime.Now))
            {
                return Ok(false);
            }

            return Ok(true);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);


            return jwt;
        }


        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            //return Ok(token);
            return Ok(new AuthenticatedResponse()
            {
                Token = token,
                RefreshToken = newRefreshToken.Token
            }); ;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


    }
}
