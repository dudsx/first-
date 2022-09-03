using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1;
using WebApplication1.Role;
using WebApplication1.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(builder => builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod())
);
builder.Services.AddDbContext<MyDbcontext>(opt =>
{
    var constr = builder.Configuration.GetSection("Constr").Value;
    var ver = new MySqlServerVersion(new Version(8, 0, 29));
    opt.UseMySql(constr, ver);
});
builder.Services.Configure<Jwtsettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        var JwtSetting = builder.Configuration.GetSection("Jwt").Get<Jwtsettings>();
        byte[] keybytes = Encoding.UTF8.GetBytes(JwtSetting.SecKey);//����2����
        var seckey = new SymmetricSecurityKey(keybytes);//����Կ����
        opt.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = seckey
        };//����У���
    });
builder.Services.AddDataProtection();//������ܷ���
builder.Services.AddIdentityCore<Myuser>(opt =>
{
    opt.Password.RequireDigit = false;//��ĸ
    opt.Password.RequireLowercase = false;//��Сд
    opt.Password.RequireNonAlphanumeric = false;//
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;//����
    opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;//������������
    opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
});
var idbuilder = new IdentityBuilder(typeof(Myuser), typeof(Myrole), builder.Services);//��ܹ��� ʵ���Զ�����
idbuilder.AddEntityFrameworkStores<MyDbcontext>(
    ).AddDefaultTokenProviders().AddUserManager<UserManager<Myuser>>()
    .AddDefaultTokenProviders().AddRoleManager<RoleManager<Myrole>>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
