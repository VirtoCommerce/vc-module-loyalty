using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Loyalty.ExperienceApi.Queries;

public class GetLoyaltyBalanceQueryBuilderHandler : IQueryHandler<GetLoyaltyBalanceQuery, LoyaltyBalanceResult>
{
    private readonly ILoyaltyLogicService _loyaltyLogicService;
    private readonly ICustomerOrderService _customerOrderService;

    public GetLoyaltyBalanceQueryBuilderHandler(ILoyaltyLogicService loyaltyLogicService, ICustomerOrderService customerOrderService)
    {
        _loyaltyLogicService = loyaltyLogicService;
        _customerOrderService = customerOrderService;
    }

    public async Task<LoyaltyBalanceResult> Handle(GetLoyaltyBalanceQuery request, CancellationToken cancellationToken)
    {
        // if UserId is not provided get user from order, if order is not provided return 0
        var result = new LoyaltyBalanceResult();

        CustomerOrder order = null;

        var userId = request.UserId;
        if (userId.IsNullOrEmpty() && request.OrderId.IsNullOrEmpty())
        {
            order = await _customerOrderService.GetNoCloneAsync(request.OrderId, CustomerOrderResponseGroup.WithPrices.ToString());

            userId = order?.CustomerId;
        }

        if (userId.IsNullOrEmpty())
        {
            return result;
        }

        result.CurrentBalance = result.ResultBalance = await _loyaltyLogicService.GetUserBalanceAsync(userId);

        if (order != null)
        {
            result.ResultBalance = Math.Max(result.CurrentBalance - order.Total, 0);
        }

        return result;
    }
}
