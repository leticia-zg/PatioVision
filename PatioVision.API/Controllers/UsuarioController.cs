using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PatioVision.Service.Services;
using PatioVision.Core.Models;
using api.Resources;
using Asp.Versioning;

namespace api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna todos os usuários cadastrados com paginação.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
                                             [FromQuery] string? search = null, [FromQuery] string? sort = null)
        {
            var result = await _service.GetPagedAsync(pageNumber, pageSize, search, sort ?? "-DtCriacao");

            var items = result.Items.Select(u =>
            {
                var res = new Resource<Usuario> { Data = u };
                res.Links.Add("self", new Link($"/api/v1/Usuario/{u.Id}", "GET"));
                res.Links.Add("update", new Link($"/api/v1/Usuario/{u.Id}", "PUT"));
                res.Links.Add("delete", new Link($"/api/v1/Usuario/{u.Id}", "DELETE"));
                return res;
            }).ToList();

            var page = new PagedResource<Usuario>
            {
                Items = items,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            page.Links.Add("self", new Link(PaginationLinkBuilder.BuildUrl(Request, result.PageNumber, result.PageSize), "GET"));
            if (result.PageNumber > 1)
                page.Links.Add("prev", new Link(PaginationLinkBuilder.BuildUrl(Request, result.PageNumber - 1, result.PageSize), "GET"));
            if (result.PageNumber < result.TotalPages)
                page.Links.Add("next", new Link(PaginationLinkBuilder.BuildUrl(Request, result.PageNumber + 1, result.PageSize), "GET"));
            if (result.TotalPages > 0)
            {
                page.Links.Add("first", new Link(PaginationLinkBuilder.BuildUrl(Request, 1, result.PageSize), "GET"));
                page.Links.Add("last", new Link(PaginationLinkBuilder.BuildUrl(Request, result.TotalPages, result.PageSize), "GET"));
            }

            return Ok(page);
        }

        /// <summary>
        /// Obtém um usuário pelo identificador.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(Guid id)
        {
            var usuario = _service.GetById(id);
            if (usuario == null)
                return NotFound();

            var resource = new Resource<Usuario> { Data = usuario };
            resource.Links.Add("self", new Link($"/api/v1/Usuario/{id}", "GET"));
            resource.Links.Add("update", new Link($"/api/v1/Usuario/{id}", "PUT"));
            resource.Links.Add("delete", new Link($"/api/v1/Usuario/{id}", "DELETE"));
            resource.Links.Add("all", new Link("/api/v1/Usuario", "GET"));

            return Ok(resource);
        }

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public IActionResult Post([FromBody] Usuario usuario)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Nome) || string.IsNullOrWhiteSpace(usuario.Email))
                return BadRequest("Dados inválidos.");

            var newUsuario = _service.Create(usuario);

            var resource = new Resource<Usuario> { Data = newUsuario };
            resource.Links.Add("self", new Link($"/api/v1/Usuario/{newUsuario.Id}", "GET"));
            resource.Links.Add("update", new Link($"/api/v1/Usuario/{newUsuario.Id}", "PUT"));
            resource.Links.Add("delete", new Link($"/api/v1/Usuario/{newUsuario.Id}", "DELETE"));
            resource.Links.Add("all", new Link("/api/v1/Usuario", "GET"));

            return CreatedAtAction(nameof(GetById), new { id = newUsuario.Id }, resource);
        }

        /// <summary>
        /// Atualiza completamente um usuário existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put(Guid id, [FromBody] Usuario usuario)
        {
            if (usuario == null || id != usuario.Id)
                return BadRequest("Dados inválidos.");

            var updated = _service.Update(id, usuario);
            if (!updated)
                return NotFound();

            var resource = new Resource<Usuario> { Data = usuario };
            resource.Links.Add("self", new Link($"/api/v1/Usuario/{usuario.Id}", "GET"));
            resource.Links.Add("update", new Link($"/api/v1/Usuario/{usuario.Id}", "PUT"));
            resource.Links.Add("delete", new Link($"/api/v1/Usuario/{usuario.Id}", "DELETE"));
            resource.Links.Add("all", new Link("/api/v1/Usuario", "GET"));

            return Ok(resource);
        }

        /// <summary>
        /// Remove um usuário.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("ID inválido.");

            var deleted = _service.Delete(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

