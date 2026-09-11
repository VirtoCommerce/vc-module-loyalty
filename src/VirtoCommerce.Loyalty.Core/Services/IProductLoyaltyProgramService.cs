using System.Threading.Tasks;
using VirtoCommerce.Loyalty.Core.Models;

namespace VirtoCommerce.Loyalty.Core.Services;

public interface IProductLoyaltyProgramService
{
    Task<LoyaltyProgram> GetTopLoyaltyProgramAsync(LoyaltyProgramEvaluationContext loyaltyContext);
}
