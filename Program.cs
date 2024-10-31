using LabProject.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Додайте сервіси до контейнера
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CinemaContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Зареєструйте BlobService
builder.Services.AddSingleton(new BlobService(
    builder.Configuration["AzureBlobStorage:ConnectionString"],
    builder.Configuration["AzureBlobStorage:ContainerName"]
));

var app = builder.Build();

// Налаштуйте конвеєр запитів HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cinemas}/{action=Index}/{id?}");

app.Run();
