using LibraryManagement.Application.AI.Commands;
using LibraryManagement.Application.AI.Queries;
using LibraryManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIService _aiService;

    public AIController(IMediator mediator, IAIService aiService)
    {
        _mediator = mediator;
        _aiService = aiService;
    }

    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations()
    {
        var result = await _mediator.Send(new GetBookRecommendationsQuery());
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("smart-search")]
    public async Task<IActionResult> SmartSearch([FromBody] SmartSearchRequest request)
    {
        var result = await _mediator.Send(new SmartSearchQuery(request.Query));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("generate-description")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionRequest request)
    {
        var result = await _mediator.Send(new GenerateBookDescriptionCommand(request.Title, request.Author));
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(new { description = result.Value });
    }

    [HttpPost("categorize")]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public async Task<IActionResult> CategorizeBook([FromBody] CategorizeRequest request)
    {
        var result = await _mediator.Send(new CategorizeBookCommand(request.Title, request.Author, request.Description));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetAIStatus()
    {
        return Ok(new { available = _aiService.IsAvailable });
    }
}

public record SmartSearchRequest(string Query);
public record GenerateDescriptionRequest(string Title, string Author);
public record CategorizeRequest(string Title, string Author, string? Description);
