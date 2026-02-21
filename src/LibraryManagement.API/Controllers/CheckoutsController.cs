using LibraryManagement.Application.Checkouts.Commands;
using LibraryManagement.Application.Checkouts.DTOs;
using LibraryManagement.Application.Checkouts.Queries;
using LibraryManagement.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckoutsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CheckoutRecordDto>>> GetCheckouts([FromQuery] QueryParameters parameters)
    {
        var result = await _mediator.Send(new GetCheckoutsQuery(parameters));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutRecordDto>> CheckoutBook([FromBody] CheckoutRequest request)
    {
        var result = await _mediator.Send(new CheckoutBookCommand(request));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{checkoutId:guid}/return")]
    public async Task<ActionResult<CheckoutRecordDto>> ReturnBook(Guid checkoutId, [FromBody] ReturnRequest? request)
    {
        var result = await _mediator.Send(new ReturnBookCommand(checkoutId, request?.Notes));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("overdue")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<ActionResult<List<CheckoutRecordDto>>> GetOverdueBooks()
    {
        var result = await _mediator.Send(new GetOverdueBooksQuery());
        return Ok(result);
    }
}
