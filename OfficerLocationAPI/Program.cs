using OfficerLocationAPI;

SocketConnection.Init();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                        builder =>
                        {
                            //builder.WithOrigins("http://localhost");
                            builder.WithOrigins("*");
                        });
});

builder.Services.AddControllers();

var app = builder.Build();

app.Urls.Add("http://ip-goes-here:5000");

// Configure the HTTP request pipeline.
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();