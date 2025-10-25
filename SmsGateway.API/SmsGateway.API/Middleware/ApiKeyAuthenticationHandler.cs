using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SmsGateway.API.Middleware
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly string _configuredApiKey;
        private readonly TimeProvider _timeProvider;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration,
            TimeProvider timeProvider)
            : base(options, logger, encoder)
        {
            _configuredApiKey = configuration["SecretKey"] ?? string.Empty; ;
            _timeProvider = timeProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (string.IsNullOrEmpty(_configuredApiKey))
            {
                Logger.LogError("Configured API key is not set.");
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
            }

            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                Logger.LogWarning("❌ Authentication failed: X-Api-Key header not found.");
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                Logger.LogWarning("❌ Authentication failed: API key was empty.");
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
            }

            if (providedApiKey != _configuredApiKey)
            {
                Logger.LogWarning("❌ Authentication failed: API key mismatch.");
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
            }

            var currentTime = _timeProvider.GetUtcNow();
            Logger.LogInformation("🔑 Authentication succeeded at {Time}", currentTime);

            var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
