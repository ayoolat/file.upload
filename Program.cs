using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http.Features;

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
    var bodySizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (bodySizeFeature is not null && bodySizeFeature.MaxRequestBodySize > 31457280)
    {
        return Results.BadRequest("Request body large");
    }

    if (!request.Form.Files.Any())
    {
        return Results.BadRequest("Please upload a file");
    }
    List<string> allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };

    foreach (var file in request.Form.Files)
    {
        String fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

       
        if (!allowedExtensions.Contains(fileExtension))
            return Results.BadRequest("Invalid file type.");

        if (file.Length > 7340032)
            return Results.BadRequest("File too large");

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
