using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors(x => x
//       .AllowAnyOrigin()
//       .AllowAnyMethod()
//       .AllowAnyHeader()
//       .WithExposedHeaders("Content-Disposition"));
app.UseHttpsRedirection();


app.MapPost("/upload", (HttpRequest request) =>
{
    var t = request;

    if (!request.Form.Files.Any())
    {
        return Results.BadRequest("Please upload a file");
    }

    

    foreach (var file in request.Form.Files)
    {
        string fileName = Path.GetFileName(file.FileName);
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "files", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
    }

    return Results.Ok("File uploaded successfully");
})
.Accepts<IFormFile>("multipart/form-data")
.Produces(200);

app.MapGet("/{fileName}",async (string fileName) =>
{
    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "files", fileName);

    if (!System.IO.File.Exists(filePath))
    {
        return Results.NotFound();
    }

    var provider = new FileExtensionContentTypeProvider();
    if (!provider.TryGetContentType(filePath, out var contentType))
    {
        contentType = "application/pdf";
    }

    var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

    return Results.File(bytes, contentType, Path.GetFileName(filePath));

});

app.Run();
