var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options  =>
{
options.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");
if(app.Environment.IsDevelopment())
options.RoutePrefix  =  "swagger";
else
options.RoutePrefix  =  string.Empty;
}
);
app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
