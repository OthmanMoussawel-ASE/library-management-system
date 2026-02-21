using LibraryManagement.Application.Books.Commands;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Books.Queries;
using LibraryManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooks([FromQuery] QueryParameters parameters)
    {
        var result = await _mediator.Send(new GetBooksQuery(parameters));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> GetBook(Guid id)
    {
        var result = await _mediator.Send(new GetBookByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookRequest request)
    {
        var result = await _mediator.Send(new CreateBookCommand(request));
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetBook), new { id = result.Value!.Id }, result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<ActionResult<BookDto>> UpdateBook(Guid id, [FromBody] UpdateBookRequest request)
    {
        var result = await _mediator.Send(new UpdateBookCommand(id, request));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<ActionResult> DeleteBook(Guid id)
    {
        var result = await _mediator.Send(new DeleteBookCommand(id));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
