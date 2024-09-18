using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Services.Kdf;
using WebApplication1.Services.FileName;
using WebApplication1.Services.OTP;
using WebApplication1.Servises.Hash;
using Microsoft.Extensions.FileProviders;
using WebApplication1.Services.Upload;
using WebApplication1.Middleware.SessionAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/* ̳��� ��� ��������� ����� - �� builder �� ���� �������������
 * ��������� - ������������ ���������� � ������ �� �������� 
 * "���� ����� �� IHashService - ������ ��'���� ����� Md5HashService" 
 */
//builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IHashService, ShaHashService>();
builder.Services.AddSingleton<IKdfService, Pbkdf1Service>();
builder.Services.AddSingleton<IFileUploader, FileUploadService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => 
{
    options.IdleTimeout = TimeSpan.FromHours(3); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

// ��������� ��������� �����
builder.Services.AddDbContext<DataContext>( options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LocalDb")
        )
);


//// ��������� 6-��������� OTP ������
//builder.Services.AddTransient<IOTPGenerator, OTPRandomSix>();
//// ��������� 4-��������� OTP ������
//builder.Services.AddTransient<IOTPGenerator, OTPRandomFour>();


//// ��������� ������ ��������� ���� �����
//builder.Services.AddTransient<IFileNameGenerator, FileNameGenerator>();


//// ��������� ������ ��� ������������ TempData � ����
//builder.Services.AddSession();
//builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.UseSessionAuth();

/*app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/Uploads"
});
*/

app.MapControllerRoute(  //�������������
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
