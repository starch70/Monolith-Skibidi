using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Traits;
using Content.Shared.Damage;
using Robust.Shared.Timing;
using System.Linq;
using Content.Server.Body.Components;

namespace Content.Server.Traits;

public sealed class BionicArmSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BionicArmComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BionicArmComponent component, ComponentStartup args)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        var root = _bodySystem.GetRootPartOrNull(uid, body);
        if (root == null)
            return;

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        // Get all body parts
        var parts = _bodySystem.GetBodyChildrenOfType(uid, BodyPartType.Arm, body);

        foreach (var part in parts)
        {
            var partComp = part.Component;
            if (partComp.Symmetry == BodyPartSymmetry.Left)
            {
                // Gets the hands before removing the arm.
                var hands = _bodySystem.GetBodyPartChildren(part.Id, partComp)
                    .Where(x => x.Component.PartType == BodyPartType.Hand);

                // Deletes the hands from existence.
                foreach (var hand in hands)
                {
                    QueueDel(hand.Id);
                }

                // Detachs and deletes the old arm. FLESH IS WEAK!
                _transform.AttachToGridOrMap(part.Id);
                QueueDel(part.Id);

                // Spawns and attachs the new arm.
                var leftArm = Spawn("JawsOfLifeLeftArm", xform.Coordinates);
                if (TryComp<BodyPartComponent>(leftArm, out var leftArmComp))
                {
                    _bodySystem.AttachPart(root.Value.Entity, "left arm", leftArm, root.Value.BodyPart, leftArmComp);
                }
            }
            else if (partComp.Symmetry == BodyPartSymmetry.Right)
            {
                // Gets the hands before removing the arm.
                var hands = _bodySystem.GetBodyPartChildren(part.Id, partComp)
                    .Where(x => x.Component.PartType == BodyPartType.Hand);

                // Deletes the hands from existence.
                foreach (var hand in hands)
                {
                    QueueDel(hand.Id);
                }

                // Detachs and deletes the old arm. FLESH IS WEAK!
                _transform.AttachToGridOrMap(part.Id);
                QueueDel(part.Id);

                // Spawns and attachs the new arm.
                var rightArm = Spawn("JawsOfLifeRightArm", xform.Coordinates);
                if (TryComp<BodyPartComponent>(rightArm, out var rightArmComp))
                {
                    _bodySystem.AttachPart(root.Value.Entity, "right arm", rightArm, root.Value.BodyPart, rightArmComp);
                }
            }
        }

        // Heals any bleeding caused by the robust arm replacement.
        if (TryComp<BloodstreamComponent>(uid, out var bloodstream))
        {
            _bloodstreamSystem.TryModifyBleedAmount(uid, -10f); // Stop bleeding
        }

        // Heals any damage caused by the robust arm replacement.
        if (TryComp<DamageableComponent>(uid, out var damageable))
        {
            var healDamage = new DamageSpecifier();
            healDamage.DamageDict.Add("Slash", -50); // Heal any slash damage.
            healDamage.DamageDict.Add("Piercing", -50); // Heal any piercing damage.
            healDamage.DamageDict.Add("Blunt", -50); // Heal any blunt damage.
            _damageableSystem.TryChangeDamage(uid, healDamage, true);
        }
    }
}
