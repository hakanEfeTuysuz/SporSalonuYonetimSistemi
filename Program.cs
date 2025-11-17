using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSistemi.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// AddDefaultIdentity bloğunu güncelliyoruz
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Bunu zaten yapmıştık

    // --- YENİ EKLENEN ŞİFRE AYARLARI ---
    // Geliştirme (development) için şifre kurallarını gevşetiyoruz
    options.Password.RequireDigit = false;           // Rakam zorunlu DEĞİL
    options.Password.RequireLowercase = false;       // Küçük harf zorunlu DEĞİL
    options.Password.RequireUppercase = false;       // Büyük harf zorunlu DEĞİL
    options.Password.RequireNonAlphanumeric = false; // Özel karakter zorunlu DEĞİL
    options.Password.RequiredLength = 3;             // Gerekli minimum uzunluk (sau=3 karakter)
    // --- BİTİŞ ---
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
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

app.MapRazorPages();

// ---------- ROL VE ADMIN OLUŞTURMA KODU (BAŞLANGIÇ) ----------
// Servisleri "scope" (kapsam) olarak çözümlüyoruz
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Gerekli servisleri (Rol Yöneticisi, Kullanıcı Yöneticisi) alıyoruz
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // ----- Roller Yoksa Oluştur -----
    // "Admin" rolü
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    // "Uye" rolü
    if (!await roleManager.RoleExistsAsync("Uye"))
    {
        await roleManager.CreateAsync(new IdentityRole("Uye"));
    }

    // ----- Admin Kullanıcısını Oluştur (Hocanın istediği) -----
    // PDF'te istenen email'i buraya yaz (ya da test için kendininkini)
    // NOT: 'ogrencinumarasi' kısmını kendi numaranla değiştirmelisin.
    var adminEmail = "b231210098@sakarya.edu.tr";
    var adminSifre = "sau"; // PDF'te istenen şifre 

    // Bu email'de bir kullanıcı var mı diye bak
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null) // Kullanıcı yoksa, oluştur
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Email'i doğrulanmış kabul et
        };

        // Kullanıcıyı 'sau' şifresiyle oluştur
        var result = await userManager.CreateAsync(adminUser, adminSifre);

        if (result.Succeeded)
        {
            // Kullanıcı başarıyla oluşursa, onu "Admin" ROLÜNE ata
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
// ---------- ROL VE ADMIN OLUŞTURMA KODU (BİTİŞ) ----------


app.Run(); // Bu satır en sonda kalmalı
