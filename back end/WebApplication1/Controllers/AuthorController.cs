using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Dto;
using WebApplication1.Role;
using WebApplication1.User;

namespace WebApplication1.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly UserManager<Myuser> _userManager;
        //private readonly SignInManager<Myuser> _signInManager;
        private readonly IConfiguration _config;
        private readonly RoleManager<Myrole> _role;
        public AuthorController(UserManager<Myuser> userManager, IConfiguration config, RoleManager<Myrole> role)
        {
            _userManager = userManager;
            //_signInManager = signInManager;
            _config = config;
            _role = role;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new Myuser();
            {
                user.Email = registerDto.Email;
                user.UserName = registerDto.Email;
            }
            var result =await _userManager.CreateAsync(user,registerDto.Password);
            if (!result.Succeeded) return BadRequest("注册失败，可能是数据异常");
            return Ok(user.Email);
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user==null)return BadRequest("没有这个用户");

            if (await _userManager.IsLockedOutAsync(user)) return BadRequest("账号锁定+" + user.LockoutEnd);
            bool succeed=await _userManager.CheckPasswordAsync(user,loginDto.Password);
            if (!succeed)
            {
                await _userManager.AccessFailedAsync(user);
                return BadRequest("密码错误");
            }

            var signingAlgoruth = SecurityAlgorithms.HmacSha256;

            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString())
                //new Claim(ClaimTypes.Role,"admin")
            };
            var rolenames = await _userManager.GetRolesAsync(user);
            foreach (var rolename in rolenames)
            {
                var chaim = new Claim(ClaimTypes.Role, rolename);
                claims.Add(chaim);
            }
            var secretByte = Encoding.UTF8.GetBytes(_config.GetSection("Jwt").Get<Jwtsettings>().SecKey);//拿到密钥的翻译
            var signingKey = new SymmetricSecurityKey(secretByte); //通过byte转化成密钥
            var signingCredemtials = new SigningCredentials(signingKey, signingAlgoruth);//算法加密
            var token = new JwtSecurityToken(
                issuer: _config["Authentication:Issuer"],  //发布者
                audience: _config["Authentication:Audience"],//发布给谁
                claims,//用户信息
                notBefore: DateTime.UtcNow,//创建时间

                expires: DateTime.UtcNow.AddDays(1),//过期时间
                signingCredemtials  //数字签名  验证方式
                );
            var tokenstr = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(tokenstr);
        }

        [HttpPost]
        public async Task<IActionResult> SendResetPasswordToken([FromBody] string usename)
        {
            Myuser user = await _userManager.FindByNameAsync(usename);
            if (user == null)
            {
                return BadRequest("用户名不存在");

            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            Console.WriteLine(token);
            return Ok();
        }
        [HttpPut]
        public async Task<IActionResult> ResetPassword(string username, string token, string resetpassword)
        {
            Myuser user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest("用户名不存在");
            }
            var result = await _userManager.ResetPasswordAsync(user, token, resetpassword);
            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                return Ok("重置了");
            }
            else
            {
                await _userManager.AccessFailedAsync(user);
                return BadRequest("密码重置失败");
            }
        }
    }
}
