using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.ValueObjects;
using MediatR;

namespace LibraryManagement.Application.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>;

public class DashboardStatsDto
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int TotalAuthors { get; set; }
    public int ActiveCheckouts { get; set; }
    public int OverdueCheckouts { get; set; }
    public int? TotalPatrons { get; set; }
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery query, CancellationToken cancellationToken)
    {
        var allBooks = await _unitOfWork.Books.GetAllAsync(cancellationToken);
        var totalBooks = allBooks.Count;
        var availableBooks = allBooks.Count(b => b.AvailableCopies > 0);

        var totalAuthors = await _unitOfWork.Repository<Domain.Entities.Author>().CountAsync(cancellationToken: cancellationToken);

        var isStaff = _currentUser.Role is "Admin" or "Librarian";

        if (isStaff)
        {
            var allCheckouts = await _unitOfWork.Checkouts.GetAllAsync(cancellationToken);
            var activeCheckouts = allCheckouts.Count(c => c.Status == CheckoutStatus.Active);
            var overdueCheckouts = allCheckouts.Count(c => c.Status == CheckoutStatus.Active && c.DueDate < DateTime.UtcNow);
            var totalPatrons = await _unitOfWork.Patrons.CountAsync(cancellationToken: cancellationToken);

            return Result<DashboardStatsDto>.Success(new DashboardStatsDto
            {
                TotalBooks = totalBooks,
                AvailableBooks = availableBooks,
                TotalAuthors = totalAuthors,
                ActiveCheckouts = activeCheckouts,
                OverdueCheckouts = overdueCheckouts,
                TotalPatrons = totalPatrons
            });
        }

        var patron = await _unitOfWork.Patrons.GetByUserIdAsync(_currentUser.UserId!, cancellationToken);

        var myActive = 0;
        var myOverdue = 0;

        if (patron is not null)
        {
            var patronCheckouts = await _unitOfWork.Checkouts.GetActiveByPatronIdAsync(patron.Id, cancellationToken);
            myActive = patronCheckouts.Count;
            myOverdue = patronCheckouts.Count(c => c.DueDate < DateTime.UtcNow);
        }

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto
        {
            TotalBooks = totalBooks,
            AvailableBooks = availableBooks,
            TotalAuthors = totalAuthors,
            ActiveCheckouts = myActive,
            OverdueCheckouts = myOverdue
        });
    }
}
