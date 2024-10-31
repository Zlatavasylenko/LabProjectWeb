using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobService(string connectionString, string containerName)
    {
        _connectionString = connectionString;
        _containerName = containerName;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        // Додайте файл в контейнер
        var blobClient = blobContainerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, true);

        // Поверніть URL файлу
        return blobClient.Uri.ToString();
    }
}
