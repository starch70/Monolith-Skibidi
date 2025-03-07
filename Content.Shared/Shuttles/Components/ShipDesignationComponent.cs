using System;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Components;

/// <summary>
/// Component that adds a random designation to a shuttle name in format XX-###
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShipDesignationComponent : Component
{
    /// <summary>
    /// The generated ship designation in format XX-### (e.g. "DR-728")
    /// </summary>
    [DataField("designation"), AutoNetworkedField]
    public string? Designation;

    /// <summary>
    /// Whether to automatically generate a designation on startup if none exists
    /// </summary>
    [DataField("autoGenerate")]
    public bool AutoGenerate = true;
}
