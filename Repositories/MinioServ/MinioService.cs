//using AngleSharp.Text;
//using Project_Api.Controllers;
using System.Collections.ObjectModel;
//using iTextSharp.text.pdf;
using System.Net.Security;
//using iTextSharp.text.pdf.security;
//using Image = iTextSharp.text.Image;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
//using iTextSharp.text;
namespace MinioServ.MinioServ;

public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _basePath;
    private readonly string _windows;
    private readonly string _minIO;
    private readonly IConfiguration _config;
    public MinioService(IConfiguration configuration)
    {
        _config = configuration;
        var minioConfig = configuration.GetSection("Minio");
        _basePath = configuration["uploadPath"];

        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        var httpClient = new HttpClient(httpClientHandler);

        _minioClient = new MinioClient()
            .WithEndpoint(minioConfig["Endpoint"])
            .WithCredentials(minioConfig["AccessKey"], minioConfig["SecretKey"])
            .WithSSL(Convert.ToBoolean(minioConfig["Secure"]))
            .WithHttpClient(httpClient)
            .Build();


        _windows = configuration["IsWindows"];
        _minIO = configuration["MinIO"];

    }

    public async Task UploadFileAsync(IFormFile file, string objectName)
    {
        try
        {
            var getListBucketsTask = await _minioClient.ListBucketsAsync().ConfigureAwait(false);


            var beArgs = new BucketExistsArgs()
                .WithBucket(_basePath);
            bool found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_basePath);
                await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }

            // Upload a file to bucket.
            using (var stream = file.OpenReadStream())
            {
                var putObjectArgs = new PutObjectArgs()
                .WithBucket(_basePath)
                .WithObject(objectName)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);
                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                Console.WriteLine("Successfully uploaded " + objectName);
            }




        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            throw;
        }
    }
    public async Task UploadFileAsync2(IFormFile file, string objectName, bool isPDF, string fileName, string sync_pdf_pass = "")
    {
        try
        {
            if (Convert.ToBoolean(_windows) || !Convert.ToBoolean(_minIO))
            {
                string path = System.IO.Path.Combine(_basePath, objectName);
                if (isPDF)
                {
                    path = path.Remove(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar)) + System.IO.Path.DirectorySeparatorChar + fileName + ".pdf";
                    if (!File.Exists(path)) File.Delete(path);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    path = path.Remove(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar)) + System.IO.Path.DirectorySeparatorChar + "Attachments";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    string fullPath = System.IO.Path.Combine(path, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);

                    }
                }
            }
            else
            {
                string path = objectName;
                if (isPDF)
                {
                    if (await FileExistsAsync(objectName))
                    {
                        path = objectName.Remove(objectName.LastIndexOf("/")) + "/" + fileName + ".pdf";

                    }
                    else
                    {
                        await DeleteFileAsync(objectName);
                    }


                }
                else
                {
                    path = objectName.Remove(objectName.LastIndexOf("/")) + "/Attachments" + fileName;
                }
                var getListBucketsTask = await _minioClient.ListBucketsAsync().ConfigureAwait(false);
                using (var stream = file.OpenReadStream())
                {
                    var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_basePath)
                    .WithObject(path)
                            .WithStreamData(stream)
                            .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType);
                    await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            throw;
        }
    }
    public async Task UploadFile(IFormFile file, string subPath, string fileName, string fileExtention, string contentType, bool addbox)
    {
        try
        {
            if (!Convert.ToBoolean(_minIO))
            {
                if (!Convert.ToBoolean(_windows))
                {
                    subPath = subPath.Replace("//", @"\");
                    fileName = fileName.Replace("//", @"\");
                }
                else
                {
                    subPath = subPath.Replace(@"\", "//");
                    fileName = fileName.Replace(@"\", "//");
                }
                string path = System.IO.Path.Combine(_basePath, subPath);
                string pathDirectory = path.Remove(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                if (!Directory.Exists(pathDirectory)) Directory.CreateDirectory(pathDirectory);
                string pathFile = System.IO.Path.Combine(pathDirectory, fileName + fileExtention);
                if (File.Exists(pathFile)) File.Delete(pathFile);
                using (var fileStream = new FileStream(pathFile, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                if (addbox)
                {
                    // AddSignatureFieldToPdf(path + "2.pdf", path + ".pdf");

                }
            }
            else
            {
                var beArgs = new BucketExistsArgs().WithBucket(_basePath);
                bool found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs().WithBucket(_basePath);
                    await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                subPath = subPath.Replace("//", @"\");
                subPath = subPath.Replace(@"\", "/");
                fileName = fileName.Replace(@"\", "/");
                var signfile = subPath.Remove(subPath.LastIndexOf("/")) + "/" + fileName;
                subPath = subPath.Remove(subPath.LastIndexOf("/")) + "/" + fileName + fileExtention;
                string path = subPath;
                if (await FileExistsAsync(subPath))
                {
                    path = subPath.Remove(subPath.LastIndexOf("/")) + "/" + fileName + fileExtention;
                }
                else
                {
                    await DeleteFileAsync(subPath);
                }
                using (var stream = file.OpenReadStream())
                {
                    var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_basePath)
                    .WithObject(path)
                            .WithStreamData(stream)
                            .WithObjectSize(file.Length)
                    .WithContentType(contentType);
                    await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                }
                ;
                if (addbox)
                {
                    //await AddSignatureFieldToPdf(signfile, fileName);
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            throw;
        }
    }

    public static HttpClient CreateHttpClientIgnoringCertificateValidation()
    {
        // Ignore SSL certificate validation (not recommended for production)
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        return new HttpClient(handler);
    }

    public async Task<byte[]> GetObjectAsByteArrayAsync(string presignedUrl)
    {
        using (HttpClient client = CreateHttpClientIgnoringCertificateValidation())
        {
            // Send a GET request to the presigned URL
            HttpResponseMessage response = await client.GetAsync(presignedUrl);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content into a byte array
                byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                return byteArray;
            }
            else
            {
                throw new Exception("Failed to fetch object from presigned URL.");
            }
        }
    }


    public async Task<returnImage> GetFile(string subPath, string fileName, string fileExtention)
    {
        try
        {
            ObservableCollection<returnImage> imagescollection = new ObservableCollection<returnImage>();
            returnImage returnImage = new returnImage();
            byte[] imageArray = null;

            if (!Convert.ToBoolean(_minIO))
            {
                if (!Convert.ToBoolean(_windows))
                {
                    subPath = subPath.Replace("//", @"\");
                }
                else
                {
                    subPath = subPath.Replace(@"\", "//");
                }
                string path = System.IO.Path.Combine(_basePath, subPath);
                string pathDirectory = path.Remove(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                if (!Directory.Exists(pathDirectory)) new InvalidOperationException("لا يوجد ملف");
                string pathFile = System.IO.Path.Combine(pathDirectory, fileName + fileExtention);
                if (!System.IO.File.Exists(pathFile))
                {
                    pathFile = System.IO.Path.Combine(_basePath, "noPDF.pdf");
                }
                imageArray = System.IO.File.ReadAllBytes(pathFile);
                //imageArray = flatten(imageArray);
                //imageArray = await AddWatermark(imageArray, "test");
                returnImage.blob = imageArray;
                returnImage.URL = null;
            }
            else
            {

                subPath = subPath.Replace(@"\", "/");
                // Get the file from MinIO and store it in the memory stream
                subPath = subPath.Remove(subPath.LastIndexOf("/")) + "/" + fileName + fileExtention;

                string presignedUrl = "";
                try
                {
                    var statArgs = new StatObjectArgs().WithBucket(_basePath).WithObject(subPath);
                    var objectStat = await _minioClient.StatObjectAsync(statArgs).ConfigureAwait(false);
                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                                                   .WithBucket(_basePath)
                                                      .WithObject(subPath)
                                                      .WithExpiry(3600); // URL expiry time in seconds (1 hour)
                    presignedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
                }
                catch
                {
                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                                                   .WithBucket(_basePath)
                                                      .WithObject("noPDF.pdf")
                                                      .WithExpiry(3600); // URL expiry time in seconds (1 hour)
                    presignedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
                }
                returnImage.URL = presignedUrl;
                returnImage.blob = null;
                // Return the byte array of the file content

            }

            return returnImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving file: {ex.Message}");
            throw;
        }
    }
    public async Task<ObservableCollection<returnImage>> GetFiles(string filePath, string? subPath)
    {
        try
        {
            ObservableCollection<returnImage> imagescollection = new ObservableCollection<returnImage>();

            if (!Convert.ToBoolean(_minIO))
            {
                if (!Convert.ToBoolean(_windows))
                {
                    subPath = subPath.Replace("//", @"\");
                    filePath = filePath.Replace("//", @"\");
                }
                else
                {
                    subPath = subPath.Replace(@"\", "//");
                    filePath = filePath.Replace(@"\", "//");
                }
                string path = System.IO.Path.Combine(_basePath, filePath);
                string pathDirectory = System.IO.Path.Combine(path.Remove(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar)), subPath);
                if (!Directory.Exists(pathDirectory)) return imagescollection;
                var files = Directory.GetFiles(pathDirectory);
                foreach (var file in files)
                {
                    string filepath = System.IO.Path.Combine(pathDirectory, file);
                    byte[] imageArray = System.IO.File.ReadAllBytes(filepath);
                    var extension = System.IO.Path.GetExtension(file.Remove(0, file.LastIndexOf('.')));
                    if (extension == ".pdf")
                    {
                        //imageArray = flatten(imageArray);
                    }
                    System.IO.Path.GetExtension(file.Remove(0, file.LastIndexOf('.')));
                    returnImage returnImage = new returnImage();
                    returnImage.blob = imageArray;
                    returnImage.EXTENSION = extension;
                    imagescollection.Add(returnImage);
                }
            }
            else
            {
                subPath = subPath.Replace("//", "/");
                filePath = filePath.Replace("//", "/");
                string path = filePath.Remove(filePath.LastIndexOf("/")) + "/" + subPath;
                ListObjectsArgs args = new ListObjectsArgs()
                              .WithBucket(_basePath)
                              .WithPrefix(path)
                              .WithRecursive(true);

                await foreach (Minio.DataModel.Item item in _minioClient.ListObjectsEnumAsync(args))
                {
                    var presignedGetObjectArgs = new PresignedGetObjectArgs()
                                                   .WithBucket(_basePath)
                                                      .WithObject(item.Key)
                                                      .WithExpiry(3600); // URL expiry time in seconds (1 hour)
                    string presignedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
                    var extension = System.IO.Path.GetExtension(item.Key);
                    returnImage returnImage = new returnImage();
                    returnImage.URL = presignedUrl;
                    returnImage.EXTENSION = extension;
                    imagescollection.Add(returnImage);
                    // Convert the memory stream to a byte array and add it to the list   
                }
            }
            return imagescollection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving file: {ex.Message}");
            throw;
        }
    }


    public async Task<returnImage> GetFileBlob(string subPath, string fileName, string fileExtention)
    {
        try
        {
            ObservableCollection<returnImage> imagescollection = new ObservableCollection<returnImage>();
            returnImage returnImage = new returnImage();
            byte[] imageArray = null;
            if (!Convert.ToBoolean(_minIO))
            {
                if (!Convert.ToBoolean(_windows))
                {
                    subPath = subPath.Replace("//", @"\");
                }
                else
                {
                    subPath = subPath.Replace(@"\", "//");
                }
                string path = System.IO.Path.Combine(_basePath, subPath);
                string pathDirectory = path.Remove(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                if (!Directory.Exists(pathDirectory)) new InvalidOperationException("لا يوجد ملف");
                string pathFile = System.IO.Path.Combine(pathDirectory, fileName + "." + fileExtention);
                if (!System.IO.File.Exists(pathFile))
                {
                    pathFile = System.IO.Path.Combine(_basePath, "noPDF.pdf");
                }
                imageArray = System.IO.File.ReadAllBytes(pathFile);
                //imageArray = flatten(imageArray);
                //imageArray = await AddWatermark(imageArray, "test");
                returnImage.blob = imageArray;
                returnImage.URL = null;
            }
            else
            {
                subPath = subPath.Replace(@"\", "/");
                // Get the file from MinIO and store it in the memory stream
                subPath = subPath.Remove(subPath.LastIndexOf("/")) + "/" + fileName + "." + fileExtention;

                byte[] fileBlob;
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        // Get the file from MinIO and store it in the memory stream
                        await _minioClient.GetObjectAsync(new GetObjectArgs()
                        .WithBucket(_basePath)
                            .WithObject(subPath)
                            .WithCallbackStream(async stream =>
                            {
                                stream.CopyTo(memoryStream);
                            }));
                        // Return the byte array of the file content
                        fileBlob = memoryStream.ToArray();
                    }
                }
                catch
                {
                    fileBlob = null;
                }
                returnImage.URL = "";
                returnImage.blob = fileBlob;
                // Return the byte array of the file content
            }
            return returnImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving file: {ex.Message}");
            throw;
        }
    }

    
    public async Task<byte[]> GetFileAsync(string objectName)
    {
        try
        {
            // Create a memory stream to store the retrieved file
            using (var memoryStream = new MemoryStream())
            {
                // Get the file from MinIO and store it in the memory stream
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_basePath)
                    .WithObject(objectName)
                    .WithCallbackStream(async stream =>
                    {
                        await stream.CopyToAsync(memoryStream);
                    }));

                // Return the byte array of the file content
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving file: {ex.Message}");
            throw;
        }
    }
    public async Task DownloadFileAsync(string objectName, string destinationPath)
    {
        try
        {
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_basePath)
                .WithObject(objectName)
                .WithFile(destinationPath));

            Console.WriteLine($"Successfully downloaded {objectName} to {destinationPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            throw;
        }
    }
    public async Task DeleteFileAsync(string objectName)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_basePath)
                .WithObject(objectName));

            Console.WriteLine($"Successfully deleted {objectName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            throw;
        }
    }
    public async Task<bool> FileExistsAsync(string objectName)
    {
        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_basePath)
                .WithObject(objectName));

            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }

 
    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600)
    {
        try
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expirySeconds);

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating presigned URL: {ex.Message}");
            throw;
        }
    }


}
public class returnImage
{
    public string? itemImageSrc { get; set; }
    public string? PDF { get; set; }
    public byte[]? blob { get; set; }
    public string? EXTENSION { get; set; }

    public string? URL { get; set; }

}
public class TextCoordinates
{
    public TextCoordinates()
    {
    }

    public TextCoordinates(double x, double y, int pageNo)
    {
        X = (float)x;
        Y = (float)y;
        PageNo = pageNo;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public int PageNo { get; set; }

}
