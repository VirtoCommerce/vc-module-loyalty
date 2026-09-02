using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQueryBuilderHandler : IQueryHandler<GetLoyaltyBalanceQuery, LoyaltyBalanceResult>
{
    private readonly ILoyaltyLogicService _loyaltyLogicService;

    public GetLoyaltyBalanceQueryBuilderHandler(ILoyaltyLogicService loyaltyLogicService)
    {
        _loyaltyLogicService = loyaltyLogicService;
    }

    public async Task<LoyaltyBalanceResult> Handle(GetLoyaltyBalanceQuery request, CancellationToken cancellationToken)
    {
        var result = await _loyaltyLogicService.GetLoyaltyBalanceAsync(new LoyaltyBalanceRequest
        {
            OrderId = request.OrderId,
            UserId = request.UserId,

            OrganizationId = request.OrganizationId,
        });

        return result;
    }
}
