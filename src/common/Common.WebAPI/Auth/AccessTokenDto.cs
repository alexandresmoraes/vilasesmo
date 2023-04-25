﻿namespace Common.WebAPI.Auth
{
  public class AccessTokenDto
  {
    public string AccessToken { get; private set; }
    public int ExpiresIn { get; private set; }
    public string TokenType { get; private set; }
    public string RefreshToken { get; private set; }
    public long IssuedUtc { get; private set; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    public AccessTokenDto(string accessToken, int expiresIn, string tokenType, string refreshToken)
    {
      AccessToken = accessToken;
      ExpiresIn = expiresIn;
      TokenType = tokenType;
      RefreshToken = refreshToken;
    }
  }
}