namespace Services.Implementatons
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Services.Interfaces;
    using Services.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CryptoModel>> ParseFileAsync(IFormFile formFile, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Parsing the file");

            using var reader = new StreamReader(formFile.OpenReadStream());
            var portfolioEntries = new List<CryptoModel>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line))
                {
                    _logger.LogWarning($"Invalid file line: {line}");
                    continue;
                }

                var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3
                    || !decimal.TryParse(parts[0], out decimal amount)
                    || !decimal.TryParse(parts[2], out decimal initialPrice))
                {
                    _logger.LogWarning($"Invalid file line: {line}");
                    continue;
                }

                var coin = parts[1];
                var cryptoEntry = new CryptoModel { Amount = amount, Coin = coin, InitialPrice = initialPrice };
                portfolioEntries.Add(cryptoEntry);
            }

            return portfolioEntries;
        }
    }
}
