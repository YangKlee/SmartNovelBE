using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;
namespace SmartNovelBE.Services
{
    public class FileStorageServices
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public FileStorageServices(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
        }
        public async Task<bool> UploadHtml(string path, string fileName, string htmlContent)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(htmlContent);
            var bucketName = _configuration["CloudflareR2:BucketName"];
            using (MemoryStream stream = new MemoryStream(bytes))
            {

                try
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = path + fileName,
                        InputStream = stream,
                        ContentType = "text/html",
                        DisablePayloadSigning = true
                    };
                    var response = await _s3Client.PutObjectAsync(putRequest);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            ;

        }
        public async Task<bool> UploadFile(string path, string fileName,IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            var bucketName = _configuration["CloudflareR2:BucketName"];
            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = path +fileName,
                    InputStream = memoryStream,
                    ContentType = file.ContentType,
                    // R2 khuyến nghị tắt tính năng ký payload trong một số trường hợp để tối ưu
                    DisablePayloadSigning = true
                };
                var response = await _s3Client.PutObjectAsync(putRequest);
                return true;
            }
            catch
            {
                return false;
            }
            return false;

        }
        public async Task<bool> DeleteFile(string path, string fileName)
        {
            var bucketName = _configuration["CloudflareR2:BucketName"];

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = path + fileName
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
