using LibraryManagement.Application.Authors.Commands;
using LibraryManagement.Application.Authors.Queries;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuthorDto>>> GetAll([FromQuery] QueryParameters parameters)
    {
        var result = await _mediator.Send(new GetAuthorsQuery(parameters));
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<AuthorDto>>> GetAllSimple()
    {
        var result = await _mediator.Send(new GetAllAuthorsQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<ActionResult<AuthorDto>> Create([FromBody] CreateAuthorRequest request)
    {
        var result = await _mediator.Send(new CreateAuthorCommand(request.FirstName, request.LastName, request.Biography));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<ActionResult<AuthorDto>> Update(Guid id, [FromBody] UpdateAuthorRequest request)
    {
        var result = await _mediator.Send(new UpdateAuthorCommand(id, request.FirstName, request.LastName, request.Biography));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteAuthorCommand(id));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record CreateAuthorRequest(string FirstName, string LastName, string? Biography);
public record UpdateAuthorRequest(string FirstName, string LastName, string? Biography);
