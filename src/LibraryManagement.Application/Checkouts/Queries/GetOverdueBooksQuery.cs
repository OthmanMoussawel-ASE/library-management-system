using AutoMapper;
using LibraryManagement.Application.Checkouts.DTOs;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Checkouts.Queries;

public record GetOverdueBooksQuery : IRequest<List<CheckoutRecordDto>>;

public class GetOverdueBooksQueryHandler : IRequestHandler<GetOverdueBooksQuery, List<CheckoutRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOverdueBooksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CheckoutRecordDto>> Handle(GetOverdueBooksQuery query, CancellationToken cancellationToken)
    {
        var overdue = await _unitOfWork.Checkouts.GetOverdueAsync(cancellationToken);
        return _mapper.Map<List<CheckoutRecordDto>>(overdue);
    }
}
