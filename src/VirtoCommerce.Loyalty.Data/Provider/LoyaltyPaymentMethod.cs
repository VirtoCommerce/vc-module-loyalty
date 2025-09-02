using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;

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
            return new ProcessPaymentRequestResult
            {
                IsSuccess = true,
                NewPaymentStatus = PaymentStatus.Paid
            };
        }

        public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
        {
            return NotSupportedResult<PostProcessPaymentRequestResult>();
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

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(System.Collections.Specialized.NameValueCollection queryString)
        {
            return new ValidatePostProcessRequestResult
            {
                IsSuccess = true,
            };
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
