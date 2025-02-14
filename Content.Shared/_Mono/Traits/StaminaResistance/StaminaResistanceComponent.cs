using Robust.Shared.GameStates;

namespace Content.Shared.Traits;

[RegisterComponent, NetworkedComponent]
public sealed partial class StaminaResistanceComponent : Component
{
    /// <summary>
    /// The multiplier for stamina damage resistance. Lower values mean more resistance.
    /// </summary>
    [DataField("coefficient")]
    public float Coefficient = 0.8f; // 20% reduction in stamina damage
} 