using EventManagement.Api.Models;
using EventManagement.Api.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EventManagementContext _context;
        public AuthController(IConfiguration configuration, EventManagementContext context) {
           _configuration = configuration;
            //TODO: kada se napravi repository, u repository pomoću dependency injectiona ubaciti context, i pozivati samo repository
           _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Registrira korisnika.
        /// </summary>
        [HttpPost("register", Name = "Register")]
        public async Task<ActionResult<User>> Register([FromBody] UserDTO request) {
            //TODO: dovršiti registraciju -> Doraditi DTO objekt, i iz DTO objekta pospremiti sve u User objekt
            CreatePasswordHash(request.Password, out string passwordHash);
            User user = new();
            //TODO: dakle provjera da li postoji isti user sa istim usernameom ili emailom(mislim da bi te vrijednosti trebale biti unikatne.)
            //TODO: dodati u usera sve potrebne informacije: adresa, itd
            user.Username = request.Username;
            user.Email = request.Email;
            user.PasswordHash = passwordHash;

            //context objekt s kojim se pristupa bazi

            //TODO: operacije sa entity frameworkom ubaciti try-catch block i vratiti bad request(400) ukoliko se radi o grešci
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User successfully registered!");
        }

        //TODO: fixati Isusove warninge

        /// <summary>
        /// Prijavljuje korisnika.
        /// </summary>
        [HttpPost("login", Name = "Login")]
        public async Task<ActionResult<string>> Login([FromBody] UserDTO request)
        {
            User user = new User();
            //TODO: doraditi login, dodati validaciju, koristeći entity framework dohvatiti usera pomoću username-a ili emaila.
            
            //spajanje na bazu sa LINQ upitom koristeći context i DBSet<User>
            user = _context.Users.Where(u => u.Username == request.Username).FirstOrDefault();

            if (user.Username != request.Username) {
                return BadRequest("User with that username does not exist.");
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash)) {
                return BadRequest("Incorrect password.");
            }

            //ovdje se kreira jwt token i postavlja se u cookie, a response vraća samo poruku.
            Response.Cookies.Append("jwt", CreateToken(user),new CookieOptions
            {
                HttpOnly = true
            });

            return Ok("Success!");
        }

        /// <summary>
        /// Vraća trenutno prijavljenog korisnika.
        /// </summary>
        [HttpGet("get_current_user", Name = "GetCurrentUser")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                var jwt = Request.Cookies["jwt"];
                var token = VerifyJwt(jwt);
                string username = token.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                var userAddress = _context.Addresses.Find(user.AddressId);
                if (userAddress != null) { 
                    user.Address = userAddress;
                }
                return Ok(user);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Odjavljuje korisnika.
        /// </summary>
        [HttpGet("logout")]
        public async Task<ActionResult<string>> Logout() {
            Response.Cookies.Delete("jwt");
            return Ok("User logged out.");
        }

        //TODO: ove metode staviti negdje van controllera tipa repository klasu ili nešto drugo
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void CreatePasswordHash(string password, out string passwordHash) {
            //using (var hmac = new HMACSHA512())
            //{
            //    passwordSalt = hmac.Key;
            //    passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            //}
            passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string passwordHash) {
            //using (var hmac = new HMACSHA512(passwordSalt))
            //{
            //    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            //    return computedHash.SequenceEqual(passwordHash);

            //}
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public JwtSecurityToken VerifyJwt(string jwt) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Token").Value);

            tokenHandler.ValidateToken(jwt, new TokenValidationParameters { 

                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false

            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }
    }
}
