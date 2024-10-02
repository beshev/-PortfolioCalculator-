namespace Services.Interfaces
{
    using Microsoft.AspNetCore.Http;
    using Services.Models;

    // TODO: This can be made generic IFileService<T>
    public interface IFileService
    {
        public Task<IEnumerable<CryptoModel>> ParseFileAsync(IFormFile fileStream, CancellationToken cancellationToken);
    }
}
