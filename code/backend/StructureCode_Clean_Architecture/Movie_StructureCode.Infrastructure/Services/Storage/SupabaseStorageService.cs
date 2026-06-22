using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Movie_StructureCode.Application.Abstractions.Services.Storage;

namespace Movie_StructureCode.Infrastructure.Services.Storage
{
    public sealed class SupabaseStorageService : IStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseApiKey;
        private readonly string _bucketName;
        private readonly string _baseUrl;

        public SupabaseStorageService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _supabaseUrl = configuration["Storage:Supabase:Url"] ?? throw new ArgumentNullException("Storage:Supabase:Url is missing");
            _supabaseApiKey = configuration["Storage:Supabase:ApiKey"] ?? throw new ArgumentNullException("Storage:Supabase:ApiKey is missing");
            _bucketName = configuration["Storage:Supabase:BucketName"] ?? "movies";
            _baseUrl = configuration["Storage:BaseUrl"] ?? throw new ArgumentNullException("Storage:BaseUrl is missing");
        }

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string pathPrefix,
            string contentType,
            CancellationToken cancellationToken)
        {
            var requestUrl = $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/{_bucketName}/{pathPrefix.Trim('/')}/{fileName}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_supabaseApiKey}");
            request.Headers.Add("apikey", _supabaseApiKey);

            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content = streamContent;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Supabase storage upload failed: {response.StatusCode} - {errorResponse}");
            }

            return $"{pathPrefix.Trim('/')}/{fileName}";
        }

        public string? GetAbsoluteUrl(string? relativePath, string? pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativePath;
            }

            var prefix = string.IsNullOrWhiteSpace(pathPrefix) ? "" : $"{pathPrefix.Trim('/')}/";
            return $"{_baseUrl.TrimEnd('/')}/{prefix}{relativePath.TrimStart('/')}";
        }
    }
}
