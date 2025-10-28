using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PatioVision.Service.Services;
using PatioVision.API.DTOs;
using Asp.Versioning;

namespace PatioVision.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza o login de um usuário e retorna um token JWT
        /// </summary>
        /// <param name="loginRequest">Credenciais de login (email e senha)</param>
        /// <returns>Token JWT e informações do usuário</returns>
        /// <response code="200">Login realizado com sucesso</response>
        /// <response code="401">Credenciais inválidas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Senha))
                return BadRequest("Email e senha são obrigatórios.");

            var usuario = await _authService.AuthenticateAsync(loginRequest.Email, loginRequest.Senha);

            if (usuario == null)
                return Unauthorized(new { message = "Email ou senha inválidos." });

            var token = _authService.GenerateJwtToken(usuario);

            var response = new LoginResponse
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = 480 * 60, // 480 minutos em segundos
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                PerfilId = usuario.Perfil
            };

            return Ok(response);
        }
    }
}
