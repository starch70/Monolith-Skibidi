using Content.Shared._RMC14.GhostColor;
using Content.Shared.Ghost;
using Robust.Client.GameObjects;

namespace Content.Client._RMC14.GhostColor;

public sealed class GhostColorSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        var defaultColor = Color.FromHex("#FFFFFF88");
        var colors = EntityQueryEnumerator<GhostColorComponent, SpriteComponent>();
        while (colors.MoveNext(out var colorComp, out var sprite))
        {
            // If the entity has a GhostComponent, respect its color
            if (TryComp<GhostComponent>(colorComp.Owner, out var ghostComp) && ghostComp.color != Color.White)
            {
                sprite.Color = ghostComp.color;
            }
            else
            {
                sprite.Color = colorComp.Color ?? defaultColor;
            }
        }
    }
}
