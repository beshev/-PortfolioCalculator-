namespace PortfolioCalculator.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    using Services.Interfaces;
    using Services.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly ILogger<PortfolioController> _logger;
        private readonly IPortfolioService _portfolioService;
        private readonly IFileService _fileService;
        private readonly IMemoryCache _memoryCache;

        public PortfolioController(
            ILogger<PortfolioController> logger,
            IPortfolioService portfolioService,
            IFileService fileService,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _portfolioService = portfolioService;
            _fileService = fileService;
            _memoryCache = memoryCache;
        }

        [HttpPost("calculate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculatePortfolio(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not provided");

            var portfolioEntries = await _fileService.ParseFileAsync(file, cancellationToken);
            var porfolio = await _portfolioService.CalculatePortfolioAsync(portfolioEntries, cancellationToken);

            var userSessionId = HttpContext.Session.GetString(Constants.UserSessionKey);

            // TODO: The messages used for the logging can be moved to some constant classes!
            _logger.LogInformation("Setting the chache");
            _memoryCache.Set(userSessionId, JsonConvert.SerializeObject(portfolioEntries));

            return Ok(porfolio);
        }

        [HttpGet("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshPortfolio(CancellationToken cancellationToken)
        {
            var userSessionId = HttpContext.Session.GetString(Constants.UserSessionKey);
            if (!_memoryCache.TryGetValue(userSessionId, out string value))
            {
                // TODO: These messages should be moved to some constant classes.
                return BadRequest("The user do not contains portfolio data");
            }

            var portfolioEntries = JsonConvert.DeserializeObject<IEnumerable<CryptoModel>>(value);
            var porfolio = await _portfolioService.CalculatePortfolioAsync(portfolioEntries, cancellationToken);

            return Ok(porfolio);
        }
    }
}
