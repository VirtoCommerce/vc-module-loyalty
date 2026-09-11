using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Extensions;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.Loyalty.Data.Provider
{
    public class LoyaltyPaymentMethod : PaymentMethod
    {
        private readonly ILoyaltyLogicService _loyaltyLogicService;
        private readonly IStoreService _storeService;

        public LoyaltyPaymentMethod(ILoyaltyLogicService loyaltyLogicService, IStoreService storeService) : base(nameof(LoyaltyPaymentMethod))
        {
            _loyaltyLogicService = loyaltyLogicService;
            _storeService = storeService;
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Unknown;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;

        public override Task<ProcessPaymentRequestResult> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            // empty result, actual payment processed in post process step
            return Task.FromResult(new ProcessPaymentRequestResult
            {
                IsSuccess = true,
            });
        }

        public override Task<ValidatePostProcessRequestResult> ValidatePostProcessRequestAsync(NameValueCollection queryString, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new ValidatePostProcessRequestResult
            {
                IsSuccess = true,
            });
        }

        public override async Task<PostProcessPaymentRequestResult> PostProcessPaymentAsync(PostProcessPaymentRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            // check balance against order total
            var order = (CustomerOrder)request.Order;

            var store = request.Store as Store ?? await _storeService.GetByIdAsync(request.StoreId ?? order.StoreId);
            var organizationBalanceMode = store?.IsOrganizationBalanceCalculationMode();

            var balance = organizationBalanceMode == true
                ? await _loyaltyLogicService.GetOrganizationBalanceAsync(order.OrganizationId)
                : await _loyaltyLogicService.GetUserBalanceAsync(order.CustomerId);

            if (balance < order.Total)
            {
                return new PostProcessPaymentRequestResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Insufficient loyalty points balance.",
                };
            }

            // create loyalty transaction
            var context = CreateLoyaltyContextByOrder(order);

            if (organizationBalanceMode == true)
            {
                context.OrganizationId = order.OrganizationId;
            }

            var amountResult = new LoyaltyAmountResult
            {
                Amount = order.Total,
                OperationType = ModuleConstants.LoyaltyPrograms.RedeemedOperationType,
            };

            var redeemResult = await _loyaltyLogicService.LogLoyaltyProgramOperationAsync(context, amountResult);
            var result = new PostProcessPaymentRequestResult
            {
                IsSuccess = redeemResult,
            };

            if (redeemResult)
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                result.ErrorMessage = "Failed redeem loyalty points for this order.";
            }

            return result;
        }

        private static LoyaltyProgramEvaluationContext CreateLoyaltyContextByOrder(CustomerOrder order)
        {
            var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
            context.ContextObjectType = nameof(CustomerOrder);
            context.OrderId = order.Id;
            context.UserId = order.CustomerId;
            return context;
        }
    }
}
