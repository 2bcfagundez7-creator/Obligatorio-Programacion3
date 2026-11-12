using Obligatorio.WebLectores.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registramos HttpClient para comunicarnos con nuestra Web API
// La dirección está en appsettings.json para no escribirla en cada Controller
builder.Services.AddHttpClient<ServicioApi>(cliente =>
{
    string direccion = builder.Configuration["Api:BaseUrl"]
        ?? "https://localhost:7065/";
    cliente.BaseAddress = new Uri(direccion);
});

// Habilitamos las sesiones para guardar el JWT del lector
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Habilitamos las sesiones del MVC
app.UseSession();

app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
