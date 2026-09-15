namespace Wp1Fall26Aws.Storage;

// Business-facing storage boundary. Wraps IS3ObjectClient so the API layer
// never has to know it's S3 underneath.
// This is a good example of the Dependency Inversion Principle (DIP).
/// <summary>
/// Storage boundary the API layer depends on. Callers get a validate-then-store
/// operation without knowing S3 sits behind it.
/// </summary>
public interface IDocumentStore
{
    /// <summary>
    /// Validates and stores a document, returning either the resulting object key
    /// and metadata or the list of validation errors.
    /// </summary>
    Task<StoreResult> StoreAsync(DocumentUpload upload, CancellationToken ct = default);
}
