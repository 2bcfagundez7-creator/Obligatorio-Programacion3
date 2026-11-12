using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Registramos los controladores de la API
// Le indica a ASP.NET Core que utilice los controladores para manejar las solicitudes HTTP entrantes.
// Esto es necesario para que la aplicación pueda procesar las solicitudes y devolver respuestas adecuadas.
builder.Services.AddControllers();


// CONFIGURACIÓN DE JWT

// Obtenemos la configuración JWT que guardamos en appsettings.json
var jwt = builder.Configuration.GetSection("Jwt");

// Obtenemos la clave secreta que se utiliza para firmar los tokens
string clave = jwt["Key"]
    ?? throw new Exception("Falta la clave JWT");

// Configuramos la autenticación de usuarios mediante tokens JWT
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Indicamos qué cosas debe comprobar la API cuando recibe un token
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Verifica quién generó el token
                ValidateIssuer = true,

                // Verifica para quién fue generado
                ValidateAudience = true,

                // Verifica que no haya vencido
                ValidateLifetime = true,

                // Verifica que la firma del token sea válida
                ValidateIssuerSigningKey = true,

                // Valores configurados en appsettings.json
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],

                // Clave utilizada para comprobar la firma
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(clave))
            };
    });

// Configuramos la autorización para controlar
// a qué funcionalidades puede acceder cada usuario
builder.Services.AddAuthorization();

// Configuramos Swagger para documentar la API
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    // Configuramos Swagger para utilizar JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingresar el token JWT"
    });

    // Permitimos que Swagger envíe el token en las solicitudes
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});



var app = builder.Build();

// Si la aplicación está en modo de desarrollo, habilitamos Swagger para documentar la API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirige las solicitudes HTTP a HTTPS
app.UseHttpsRedirection();

// Autenticación: identifica al usuario mediante su token JWT
app.UseAuthentication();

// Autorización: comprueba si el usuario tiene permiso
// para acceder al recurso solicitado
app.UseAuthorization();

// Permite utilizar los controladores
// Habilita las rutas que definamos en nuestros controladores mediante el atributo [Route] y los métodos HTTP (GET, POST, PUT, DELETE, etc.).
app.MapControllers();

// Inicia la aplicación y comienza a escuchar las solicitudes entrantes
app.Run();
