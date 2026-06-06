using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NguyenDucTruong_2380614167.Data;
using NguyenDucTruong_2380614167.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });

builder.Services.AddControllersWithViews();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await SeedDataAsync(app);

app.Run();

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();

    string[] roles = { "ADMIN", "STUDENT" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin",
            Email = "admin@gmail.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "ADMIN");
        }
    }

    await SeedCategoriesAsync(context);
    await SeedCoursesAsync(context);
}

static async Task SeedCategoriesAsync(ApplicationDbContext context)
{
    string[] categoryNames =
    {
        "Lập trình",
        "Cơ sở dữ liệu",
        "Mạng máy tính"
    };

    var categories = await context.Categories
        .OrderBy(c => c.Id)
        .ToListAsync();

    for (int i = 0; i < categoryNames.Length; i++)
    {
        if (i < categories.Count)
        {
            categories[i].Name = categoryNames[i];
        }
        else
        {
            context.Categories.Add(new Category { Name = categoryNames[i] });
        }
    }

    await context.SaveChangesAsync();
}

static async Task SeedCoursesAsync(ApplicationDbContext context)
{
    var lapTrinh = await context.Categories.FirstAsync(c => c.Name == "Lập trình");
    var csdl = await context.Categories.FirstAsync(c => c.Name == "Cơ sở dữ liệu");
    var mang = await context.Categories.FirstAsync(c => c.Name == "Mạng máy tính");

    var courseData = new[]
    {
        new Course { Name = "Lập trình web", Credits = 3, Lecturer = "Nguyễn Văn A", Image = "/images/web.svg", CategoryId = lapTrinh.Id },
        new Course { Name = "ASP.NET Core MVC", Credits = 3, Lecturer = "Trần Thị B", Image = "/images/mvc.svg", CategoryId = lapTrinh.Id },
        new Course { Name = "Entity Framework Core", Credits = 2, Lecturer = "Lê Văn C", Image = "/images/efcore.svg", CategoryId = lapTrinh.Id },
        new Course { Name = "Cơ sở dữ liệu", Credits = 3, Lecturer = "Phạm Thị D", Image = "/images/database.svg", CategoryId = csdl.Id },
        new Course { Name = "SQL Server", Credits = 2, Lecturer = "Hoàng Văn E", Image = "/images/sql.svg", CategoryId = csdl.Id },
        new Course { Name = "Mạng máy tính căn bản", Credits = 3, Lecturer = "Đỗ Thị F", Image = "/images/network.svg", CategoryId = mang.Id },
        new Course { Name = "Bảo mật ứng dụng web", Credits = 2, Lecturer = "Nguyễn Văn G", Image = "/images/security.svg", CategoryId = lapTrinh.Id }
    };

    var courses = await context.Courses
        .OrderBy(c => c.Id)
        .ToListAsync();

    for (int i = 0; i < courseData.Length; i++)
    {
        if (i < courses.Count)
        {
            UpdateCourse(courses[i], courseData[i]);
        }
        else
        {
            context.Courses.Add(courseData[i]);
        }
    }

    await context.SaveChangesAsync();
}

static void UpdateCourse(Course course, Course source)
{
    course.Name = source.Name;
    course.Image = source.Image;
    course.Credits = source.Credits;
    course.Lecturer = source.Lecturer;
    course.CategoryId = source.CategoryId;
}
