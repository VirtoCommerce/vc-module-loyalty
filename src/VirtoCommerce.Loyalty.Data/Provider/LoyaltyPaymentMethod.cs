using System.Collections.Specialized;
using VirtoCommerce.Loyalty.Core;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Loyalty.Data.Provider
{
    public class LoyaltyPaymentMethod : PaymentMethod
    {
        private readonly ILoyaltyLogicService _loyaltyLogicService;

        public LoyaltyPaymentMethod(ILoyaltyLogicService loyaltyLogicService) : base(nameof(LoyaltyPaymentMethod))
        {
            _loyaltyLogicService = loyaltyLogicService;
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Unknown;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;

        public override ProcessPaymentRequestResult ProcessPayment(ProcessPaymentRequest request)
        {
            // empty result, actual payment processed in post process step
            return new ProcessPaymentRequestResult
            {
                IsSuccess = true,
            };
        }

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(NameValueCollection queryString)
        {
            return new ValidatePostProcessRequestResult
            {
                IsSuccess = true,
            };
        }

        public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
        {
            // check balance against order total
            var order = (CustomerOrder)request.Order;
            var balance = _loyaltyLogicService.GetUserBalanceAsync(order.CustomerId)
                .GetAwaiter()
                .GetResult();

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
            var amountResult = new LoyaltyAmountResult
            {
                Amount = order.Total,
                OperationType = ModuleConstants.LoyaltyPrograms.RedeemedOperationType,
            };

            var redeemResult = _loyaltyLogicService.LogLoyaltyProgramOperationAsync(context, amountResult).GetAwaiter().GetResult();
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

        public override VoidPaymentRequestResult VoidProcessPayment(VoidPaymentRequest request)
        {
            return NotSupportedResult<VoidPaymentRequestResult>();
        }

        public override CapturePaymentRequestResult CaptureProcessPayment(CapturePaymentRequest request)
        {
            return NotSupportedResult<CapturePaymentRequestResult>();
        }

        public override RefundPaymentRequestResult RefundProcessPayment(RefundPaymentRequest request)
        {
            return NotSupportedResult<RefundPaymentRequestResult>();
        }

        private static LoyaltyProgramEvaluationContext CreateLoyaltyContextByOrder(CustomerOrder order)
        {
            var context = AbstractTypeFactory<LoyaltyProgramEvaluationContext>.TryCreateInstance();
            context.ContextObjectType = nameof(CustomerOrder);
            context.OrderId = order.Id;
            context.UserId = order.CustomerId;
            return context;
        }

        private static T NotSupportedResult<T>() where T : PaymentRequestResultBase, new()
        {
            var result = new T
            {
                IsSuccess = false,
                ErrorMessage = "Not supported by LoyaltyPaymentMethod",
            };

            return result;
        }
    }
}
