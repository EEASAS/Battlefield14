using Content.Shared.GameTicking.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;

namespace Content.Server.GameTicking;

public sealed class ShiftTimeLockSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShiftTimeLockComponent, GettingInteractedWithAttemptEvent>(OnGettingInteractedWithAttempt);
    }

    private void OnGettingInteractedWithAttempt(EntityUid uid, ShiftTimeLockComponent component, ref GettingInteractedWithAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var currentShift = _gameTicker.RoundDuration();
        if (currentShift >= component.ShiftTime)
        {
            RemComp<ShiftTimeLockComponent>(uid);
            return;
        }

        _popup.PopupClient(Loc.GetString("shift-time-lock-blocked"), args.Uid);
        args.Cancelled = true;
    }
}
