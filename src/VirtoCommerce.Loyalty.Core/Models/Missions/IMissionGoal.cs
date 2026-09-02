namespace VirtoCommerce.Loyalty.Core.Models.Missions;

/// <summary>
/// Marker for the mission "goal" nodes stored inside the mission condition tree.
/// These nodes are not real predicates - their IsSatisfiedBy always returns true.
/// </summary>
public interface IMissionGoal
{
    string MissionType { get; }
}
