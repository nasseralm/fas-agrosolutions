using System.Security.Claims;

namespace FAS.Infra.Ioc
{
    /// <summary>
    /// Extensões para obter ProducerId/UserId do token JWT (uso nos serviços).
    /// </summary>
    public static class ClaimsPrincipalExtension
    {
        public static int GetId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst("id");
            if (idClaim != null && int.TryParse(idClaim.Value, out var id))
                return id;
            return 0;
        }

        public static int GetProducerId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("ProducerId") ?? user.FindFirst("id");
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
            return 0;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            var emailClaim = user.FindFirst("email");
            return emailClaim?.Value ?? string.Empty;
        }
    }
}

