//using System.Numerics;
//using Content.Shared._RMC14.Weapons.Ranged.Prediction;
//using Content.Shared.Camera;
//using Content.Shared.Damage;
//using Content.Shared.Damage.Components;
//using Content.Shared.Damage.Systems;
//using Content.Shared.Database;
//using Content.Shared.DoAfter;
//using Content.Shared.Effects;
//using Content.Shared.Hands.EntitySystems;
//using Content.Shared.Interaction;
//using Content.Shared.Mobs.Components;
//using Content.Shared.Weapons.Ranged.Components;
//using Content.Shared.Weapons.Ranged.Systems;
//using Robust.Shared.Audio.Systems;
//using Robust.Shared.Map;
//using Robust.Shared.Network;
//using Robust.Shared.Physics;
//using Robust.Shared.Physics.Components;
//using Robust.Shared.Physics.Dynamics;
//using Robust.Shared.Physics.Events;
//using Robust.Shared.Physics.Systems;
//using Robust.Shared.Player;
//using Robust.Shared.Serialization;
//using Robust.Shared.Utility;
//using Robust.Shared.Threading;
//using System.Collections.Concurrent;
//using Robust.Shared.Timing;
//using Content.Shared._Mono;
//using Content.Shared.Tag;
//using Content.Shared.BarricadeBlock; // BF14
//using Robust.Shared.Random; // BF14
//
//namespace Content.Shared.Projectiles;
//
//public abstract partial class SharedProjectileSystem : EntitySystem
//{
//    public const string ProjectileFixture = "projectile";
//
//    [Dependency] private readonly INetManager _netManager = default!;
//    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
//    [Dependency] private readonly SharedAudioSystem _audio = default!;
//    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
//    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
//    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
//    [Dependency] private readonly SharedGunSystem _guns = default!;
//    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
//    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
//    [Dependency] private readonly SharedTransformSystem _transform = default!;
//    [Dependency] private readonly TagSystem _tag = default!;
//    [Dependency] private readonly IParallelManager _parallel = default!;
//    [Dependency] private readonly IGameTiming _gameTiming = default!;
//    [Dependency] private readonly INetManager _net = default!;
//    [Dependency] private readonly IRobustRandom _random = default!; // BF14
//
//    // Cache of projectiles waiting for collision checks
//    private readonly ConcurrentQueue<(EntityUid Uid, ProjectileComponent Component, EntityUid Target)> _pendingCollisionChecks = new();
//    private readonly HashSet<EntityUid> _processedProjectiles = new();
//    private const int MinProjectilesForParallel = 8;
//    private const int ProjectileBatchSize = 16;
//    private TimeSpan _lastBatchProcess;
//    private readonly TimeSpan _processingInterval = TimeSpan.FromMilliseconds(16); // ~60Hz
//
//    public override void Initialize()
//    {
//        base.Initialize();
//
//        SubscribeLocalEvent<ProjectileComponent, StartCollideEvent>(OnStartCollide);
//        SubscribeLocalEvent<ProjectileComponent, PreventCollideEvent>(PreventCollision);
//
//        SubscribeLocalEvent<EmbeddedContainerComponent, EntityTerminatingEvent>(OnEmbeddableTermination);
//        // Subscribe to initialize the origin grid on ProjectileGridPhaseComponent
//        SubscribeLocalEvent<ProjectileGridPhaseComponent, ComponentStartup>(OnProjectileGridPhaseStartup);
//        // Subscribe to ensure MetaDataComponent on projectile entities for networking
//        SubscribeLocalEvent<ProjectileComponent, ComponentStartup>(OnProjectileMetaStartup);
//
//        // Mono
//        SubscribeLocalEvent<ProjectileComponent, TileFrictionEvent>(OnTileFriction);
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