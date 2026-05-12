using UnityEngine;

namespace RouletteGame.Core
{
    //////////////////////////////////////////////////////////////////////////
    // Implements the State Pattern for the roulette game flow.
    // Each game phase (betting, spinning, results, payouts) is a
    // separate state class responsible for its own behavior.
    //////////////////////////////////////////////////////////////////////////

    // Game State Interface
    public interface IGameState
    {
        string Name { get; }
        void Enter(RouletteGameManager context);
        void Exit(RouletteGameManager context);

        // Optional per-frame update hook.
        void Tick(RouletteGameManager context);
    }

    //////////////////////////////////////////////////////////////////////////
    // WaitingToStart
    public class WaitingToStartState : IGameState
    {
        public string Name => "WaitingToStart";

        public void Enter(RouletteGameManager context)
        {
            RouletteEventBus.RaiseGameStateChanged(Name);
            Debug.Log("[State] WaitingToStart");
        }
        public void Exit(RouletteGameManager context) { }
        public void Tick(RouletteGameManager context) { }
    }

    //////////////////////////////////////////////////////////////////////////
    // PlacingBets
    public class PlacingBetsState : IGameState
    {
        public string Name => "PlacingBets";

        public void Enter(RouletteGameManager context)
        {
            RouletteEventBus.RaiseGameStateChanged(Name);
            RouletteEventBus.RaiseBettingStarted();
            Debug.Log("[State] PlacingBets");
        }
        public void Exit(RouletteGameManager context)
        {
            RouletteEventBus.RaiseBettingEnded();
        }
        public void Tick(RouletteGameManager context) { }
    }

    //////////////////////////////////////////////////////////////////////////
    // SpinningWheel
    public class SpinningWheelState : IGameState
    {
        public string Name => "SpinningWheel";

        public void Enter(RouletteGameManager context)
        {
            RouletteEventBus.RaiseGameStateChanged(Name);
            RouletteEventBus.RaiseSpinStarted();
            Debug.Log("[State] SpinningWheel");

            // Trigger wheel spin with pre-determined winning number.
            int winning = context.PendingWinningNumber;
            context.RouletteController?.Spin(winning);
        }
        public void Exit(RouletteGameManager context) { }
        public void Tick(RouletteGameManager context) { }
    }

    //////////////////////////////////////////////////////////////////////////
    // ShowingResult
    public class ShowingResultState : IGameState
    {
        public string Name => "ShowingResult";

        public void Enter(RouletteGameManager context)
        {
            RouletteEventBus.RaiseGameStateChanged(Name);
            RouletteEventBus.RaiseWinningNumber(context.PendingWinningNumber);
            Debug.Log($"[State] ShowingResult – winner: {context.PendingWinningNumber}");
        }
        public void Exit(RouletteGameManager context) { }
        public void Tick(RouletteGameManager context) { }
    }

    //////////////////////////////////////////////////////////////////////////
    // Payouts
    public class PayoutsState : IGameState
    {
        public string Name => "Payouts";

        public void Enter(RouletteGameManager context)
        {
            RouletteEventBus.RaiseGameStateChanged(Name);
            Debug.Log("[State] Payouts");
            context.ProcessPayouts();
        }
        public void Exit(RouletteGameManager context) { }
        public void Tick(RouletteGameManager context) { }
    }
}