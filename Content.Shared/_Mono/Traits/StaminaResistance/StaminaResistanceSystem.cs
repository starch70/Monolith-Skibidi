using Content.Shared.Damage.Events;

namespace Content.Shared.Traits;

public sealed class StaminaResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaminaResistanceComponent, TakeStaminaDamageEvent>(OnStaminaDamage);
    }

    private void OnStaminaDamage(EntityUid uid, StaminaResistanceComponent component, ref TakeStaminaDamageEvent args)
    {
        args.Multiplier *= component.Coefficient;
    }
} 