﻿using Auth.API.Data;
using Auth.API.Models;
using Common.WebAPI.Auth;
using Common.WebAPI.Data;
using Common.WebAPI.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Auth.API.Controllers
{
  [Route("api/auth")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService<IdentityUser> _authService;
    private readonly IJwtService _jwtService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
      IAuthService<IdentityUser> authService,
      IJwtService jwtService,
      SignInManager<IdentityUser> signInManager,
      IUnitOfWork<ApplicationDbContext> unitOfWork,
      ILogger<AuthController> logger)
    {
      _authService = authService ?? throw new ArgumentNullException(nameof(authService));
      _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
      _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
      _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Endpoint para verificar se o usuário está autorizado
    /// </summary>
    // GET api/auth
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Result GetAsync() => Result.Ok();

    /// <summary>
    /// Login de usuário
    /// </summary>
    // POST api/auth/login    
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AccessTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<Result<AccessTokenDto>> LoginAsync([FromBody] LoginModel login, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation($"Login started {JsonSerializer.Serialize(login)}.");

      var user = await _authService.GetUserByUsernameOrEmail(login.UsernameOrEmail!);

      if (user is null)
      {
        _logger.LogWarning($"Login failed {login.UsernameOrEmail}.");
        return Result.Fail<AccessTokenDto>("Usuário ou senha não confere.");
      }

      var resultSign = await _signInManager.CheckPasswordSignInAsync(user!, login.Password, true);

      await _unitOfWork.CommitAsync(cancellationToken);

      if (resultSign.Succeeded)
      {
        _logger.LogInformation($"Login succeeded {login.UsernameOrEmail}.");
        return Result.Ok(await _jwtService.GenerateToken(user!.UserName));
      }
      else if (resultSign.IsLockedOut)
      {
        _logger.LogWarning($"Login failed {login.UsernameOrEmail}.");
        return Result.Fail<AccessTokenDto>("Usuário bloqueado.");
      }

      _logger.LogWarning($"Login failed {login.UsernameOrEmail}.");
      return Result.Fail<AccessTokenDto>($"Usuário ou senha não confere, restam {await _authService.GetFailedAccessAttempts(user)} tentativas.");
    }
  }
}