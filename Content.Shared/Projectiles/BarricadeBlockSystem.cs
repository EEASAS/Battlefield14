//using Robust.Shared.Physics.Events;
//using Content.Shared.BarricadeBlock; // BF14
//using Robust.Shared.Random; // BF14
//
//namespace Content.Shared.Projectiles;
//
//public abstract partial class BarricadeBlockSystem : EntitySystem
//{
//
//    [Dependency] private readonly IRobustRandom _random = default!; // BF14
//    [Dependency] private readonly SharedTransformSystem _transform = default!;
//
//    public override void Initialize()
//    {
//        base.Initialize();
//
//        SubscribeLocalEvent<ProjectileComponent, PreventCollideEvent>(PreventCollision);
//    }
//
//    //ported from civ14
//    private void PreventCollision(EntityUid uid, ProjectileComponent component, ref PreventCollideEvent args)
//    {
//        if (component.IgnoreShooter && (args.OtherEntity == component.Shooter || args.OtherEntity == component.Weapon))
//        {
//            args.Cancelled = true;
//        }
//        //check for BarricadeBlock component (percentage of chance to hit/pass over)
//        if (TryComp(args.OtherEntity, out BarricadeBlockComponent? BarricadeBlock))
//        {
//            var alwaysPassThrough = false;
//            //_sawmill.Info("Checking BarricadeBlock...");
//            if (component.Shooter is { } shooterUid && Exists(shooterUid))
//            {
//                // Condition 1: Directions are the same (using cardinal directions).
//                // Or, if bidirectional, directions can be opposite.
//                var shooterWorldRotation = _transform.GetWorldRotation(shooterUid);
//                var BarricadeBlockWorldRotation = _transform.GetWorldRotation(args.OtherEntity);
//
//                var shooterDir = shooterWorldRotation.GetCardinalDir();
//                var BarricadeBlockDir = BarricadeBlockWorldRotation.GetCardinalDir();
//
//                bool directionallyAllowed = false;
//                if (shooterDir == BarricadeBlockDir)
//                {
//                    directionallyAllowed = true;
//                    //_sawmill.Debug("Shooter and BarricadeBlock facing same cardinal direction.");
//                }
//                else if (BarricadeBlock.Bidirectional)
//                {
//                    var oppositeBarricadeBlockDir = (Direction)(((int)BarricadeBlockDir + 4) % 8);
//                    if (shooterDir == oppositeBarricadeBlockDir)
//                    {
//                        directionallyAllowed = true;
//                        //_sawmill.Debug("Shooter and BarricadeBlock facing opposite cardinal directions (bidirectional pass).");
//                    }
//                }
//
//                if (directionallyAllowed)
//                {
//                    // Condition 2: Firer is within 1 tile of the BarricadeBlock.
//                    var shooterCoords = Transform(shooterUid).Coordinates;
//                    var BarricadeBlockCoords = Transform(args.OtherEntity).Coordinates;
//
//                    if (shooterCoords.TryDistance(EntityManager, BarricadeBlockCoords, out var distance) &&
//                        distance <= 1.5f)
//                    {
//                        alwaysPassThrough = true;
//                    }
//                }
//            }
//
//            if (alwaysPassThrough)
//            {
//                args.Cancelled = true;
//            }
//            else
//            {
//                //_sawmill.Debug("BarricadeBlock direction/distance check failed or shooter not valid.");
//                // Standard BarricadeBlock blocking logic if the special conditions are not met.
//                var rando = _random.NextFloat(0.0f, 100.0f);
//                if (rando >= BarricadeBlock.Blocking)
//                {
//                    args.Cancelled = true;
//                }
//                else
//                {
//                    return;
//                }
//            }
//        }
//    }
//}