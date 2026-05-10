//////////////////////////////////////////////////////////////////////////
//  STATE PATTERN – Each game phase is its own class.
//  RouletteGameManager holds the current IGameState and
//  delegates Enter / Exit / Tick to it.
//////////////////////////////////////////////////////////////////////////

using UnityEngine;

// Game State Interface
public interface IGameState
{
    string Name { get; }
    void Enter(RouletteGameManager ctx);
    void Exit(RouletteGameManager ctx);
    void Tick(RouletteGameManager ctx);          // called in Update if needed
}

// WaitingToStart
public class WaitingToStartState : IGameState
{
    public string Name => "WaitingToStart";

    public void Enter(RouletteGameManager ctx)
    {
        RouletteEventBus.RaiseGameStateChanged(Name);
        Debug.Log("[State] WaitingToStart");
    }
    public void Exit(RouletteGameManager ctx) { }
    public void Tick(RouletteGameManager ctx) { }
}

// PlacingBets
public class PlacingBetsState : IGameState
{
    public string Name => "PlacingBets";

    public void Enter(RouletteGameManager ctx)
    {
        RouletteEventBus.RaiseGameStateChanged(Name);
        RouletteEventBus.RaiseBettingStarted();
        Debug.Log("[State] PlacingBets");
    }
    public void Exit(RouletteGameManager ctx)
    {
        RouletteEventBus.RaiseBettingEnded();
    }
    public void Tick(RouletteGameManager ctx) { }
}

// SpinningWheel
public class SpinningWheelState : IGameState
{
    public string Name => "SpinningWheel";

    public void Enter(RouletteGameManager ctx)
    {
        RouletteEventBus.RaiseGameStateChanged(Name);
        RouletteEventBus.RaiseSpinStarted();
        Debug.Log("[State] SpinningWheel");

        int winning = ctx.PendingWinningNumber;
        ctx.RouletteController?.Spin(winning);
    }
    public void Exit(RouletteGameManager ctx) { }
    public void Tick(RouletteGameManager ctx) { }
}

// ShowingResult
public class ShowingResultState : IGameState
{
    public string Name => "ShowingResult";

    public void Enter(RouletteGameManager ctx)
    {
        RouletteEventBus.RaiseGameStateChanged(Name);
        RouletteEventBus.RaiseWinningNumber(ctx.PendingWinningNumber);
        Debug.Log($"[State] ShowingResult – winner: {ctx.PendingWinningNumber}");
    }
    public void Exit(RouletteGameManager ctx) { }
    public void Tick(RouletteGameManager ctx) { }
}

// Payouts
public class PayoutsState : IGameState
{
    public string Name => "Payouts";

    public void Enter(RouletteGameManager ctx)
    {
        RouletteEventBus.RaiseGameStateChanged(Name);
        Debug.Log("[State] Payouts");
        ctx.ProcessPayouts();
    }
    public void Exit(RouletteGameManager ctx) { }
    public void Tick(RouletteGameManager ctx) { }
}