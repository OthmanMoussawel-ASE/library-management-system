using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, string Name, string? Description) : IRequest<Result<CategoryDto>>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.", 404);

        category.Name = command.Name;
        category.Description = command.Description;

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("categories_", cancellationToken);

        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }
}
