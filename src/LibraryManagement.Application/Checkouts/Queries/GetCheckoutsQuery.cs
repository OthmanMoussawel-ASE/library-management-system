using AutoMapper;
using LibraryManagement.Application.Checkouts.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.Specifications;
using MediatR;

namespace LibraryManagement.Application.Checkouts.Queries;

public record GetCheckoutsQuery(QueryParameters Parameters) : IRequest<PagedResult<CheckoutRecordDto>>;

public class GetCheckoutsQueryHandler : IRequestHandler<GetCheckoutsQuery, PagedResult<CheckoutRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetCheckoutsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CheckoutRecordDto>> Handle(GetCheckoutsQuery query, CancellationToken cancellationToken)
    {
        Guid? patronId = null;
        var isStaff = _currentUser.Role is "Admin" or "Librarian";

        if (!isStaff)
        {
            var patron = await _unitOfWork.Patrons.GetByUserIdAsync(_currentUser.UserId!, cancellationToken);
            patronId = patron?.Id;
        }

        var spec = new CheckoutFilterSpecification(query.Parameters, patronId);
        var countSpec = new CheckoutFilterSpecification(query.Parameters, patronId, forCount: true);

        var checkouts = await _unitOfWork.Checkouts.FindAsync(spec, cancellationToken);
        var totalCount = await _unitOfWork.Checkouts.CountAsync(countSpec, cancellationToken);

        var dtos = _mapper.Map<List<CheckoutRecordDto>>(checkouts);
        return PagedResult<CheckoutRecordDto>.Create(dtos, totalCount, query.Parameters.PageNumber, query.Parameters.PageSize);
    }
}

public class CheckoutFilterSpecification : BaseSpecification<CheckoutRecord>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "checkedoutat", "duedate", "returnedat", "status"
    };

    public CheckoutFilterSpecification(QueryParameters parameters, Guid? patronId = null, bool forCount = false)
    {
        AddInclude(c => c.Book);
        AddInclude(c => c.Patron);

        if (patronId.HasValue)
            Criteria = c => c.PatronId == patronId.Value;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            AndCriteria(c =>
                c.Book.Title.ToLower().Contains(searchTerm) ||
                c.Patron.Email.ToLower().Contains(searchTerm) ||
                c.Patron.FullName.ToLower().Contains(searchTerm)
            );
        }

        if (parameters.Filters is not null)
        {
            foreach (var filter in parameters.Filters)
            {
                switch (filter.Key.ToLower())
                {
                    case "status" when Enum.TryParse<Domain.ValueObjects.CheckoutStatus>(filter.Value, true, out var status):
                        AndCriteria(c => c.Status == status);
                        break;
                    case "overdue" when bool.TryParse(filter.Value, out var overdue) && overdue:
                        AndCriteria(c => c.Status == Domain.ValueObjects.CheckoutStatus.Active && c.DueDate < DateTime.UtcNow);
                        break;
                }
            }
        }

        if (forCount) return;

        ApplyPaging((parameters.PageNumber - 1) * parameters.PageSize, parameters.PageSize);

        var sortBy = parameters.SortBy;
        if (!string.IsNullOrEmpty(sortBy) && AllowedSortFields.Contains(sortBy))
        {
            var isDescending = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            switch (sortBy.ToLower())
            {
                case "checkedoutat":
                    if (isDescending) ApplyOrderByDescending(c => c.CheckedOutAt);
                    else ApplyOrderBy(c => c.CheckedOutAt);
                    break;
                case "duedate":
                    if (isDescending) ApplyOrderByDescending(c => c.DueDate);
                    else ApplyOrderBy(c => c.DueDate);
                    break;
                case "returnedat":
                    if (isDescending) ApplyOrderByDescending(c => c.ReturnedAt!);
                    else ApplyOrderBy(c => c.ReturnedAt!);
                    break;
                case "status":
                    if (isDescending) ApplyOrderByDescending(c => c.Status);
                    else ApplyOrderBy(c => c.Status);
                    break;
            }
        }
        else
        {
            ApplyOrderByDescending(c => c.CheckedOutAt);
        }
    }
}
