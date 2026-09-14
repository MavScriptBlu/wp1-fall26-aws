using Amazon.S3;
using Wp1Fall26Aws.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddDocumentStorage(builder.Configuration);

var app = builder.Build();

// Beanstalk pings this by default. A 404 here reads as a degraded
// environment even though nothing is actually wrong.
app.MapGet("/", () => Results.Ok());

// No AWS call here on purpose - a health check that depends on S3 reports
// unhealthy during an unrelated outage.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/documents", async (HttpRequest request, IDocumentStore store, ILoggerFactory loggerFactory, CancellationToken ct) =>
{
    var logger = loggerFactory.CreateLogger("Documents");

    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { errors = new[] { "No file was provided." } });
    }

    var form = await request.ReadFormAsync(ct);
    var file = form.Files["file"] ?? form.Files.FirstOrDefault();

    if (file is null)
    {
        return Results.BadRequest(new { errors = new[] { "No file was provided." } });
    }

    var upload = new DocumentUpload(file.FileName, file.ContentType, file.Length, file.OpenReadStream());

    try
    {
        var result = await store.StoreAsync(upload, ct);

        if (!result.Success)
        {
            return Results.BadRequest(new { errors = result.Errors });
        }

        logger.LogInformation("Document uploaded. Key={Key} SizeBytes={Size}", result.ObjectKey, upload.ContentLength);

        return Results.Json(new { objectKey = result.ObjectKey, metadata = result.Metadata }, statusCode: StatusCodes.Status201Created);
    }
    catch (Exception ex)
    {
        // Never echo the AWS exception back to the caller - it can contain
        // a bucket name, an ARN, or a request ID. Log it server-side only.
        logger.LogError(ex, "Document storage failed.");
        return Results.Json(new { errors = new[] { "An error occurred while storing the document." } }, statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.Run();
