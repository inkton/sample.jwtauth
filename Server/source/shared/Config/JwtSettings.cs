namespace Jwtauth.Config
{
  public class JwtSettings
  {
      public string Issuer { get; set; }
      public string Audience { get; set; }
      public bool RequireHttpsMetadata { get; set; }
      public string SecretKey { get; set; }
      public string EncryptKey { get; set; }
      public double LongTermExpireMinutes { get; set; }
      public double ShortTermExpireMinutes { get; set; }
      public double NotBeforeMinutes { get; set; }
    }
  }
