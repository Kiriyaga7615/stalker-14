using Content.Client._Stalker.StalkerUi.Widgets;
using Content.Client.Gameplay;
using Content.Shared._Stalker.Psyonics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._Stalker.StalkerUi;

public sealed class PsyUiController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PsyEnergyChangedMessage>(OnPsyCounterUpdate);
    }

    private void OnPsyCounterUpdate(PsyEnergyChangedMessage msg, EntitySessionEventArgs args = default!)
    {
        if (UIManager.ActiveScreen == null)
            return;
        if (UIManager.ActiveScreen.GetWidget<PsyGui>() is { } psy)
        {
            psy.UpdatePanelEntity(msg.NewPsy, msg.MaxPsy);
        }
    }

    private void OnScreenLoad()
    {
        OnPsyCounterUpdate(new PsyEnergyChangedMessage(0, 0, 0));
    }

    private void DestroyUi()
    {

    }

    public void OnStateEntered(GameplayState state)
    {
        OnScreenLoad();
    }

    public void OnStateExited(GameplayState state)
    {
        DestroyUi();
    }
}
