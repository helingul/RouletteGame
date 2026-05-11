//////////////////////////////////////////////////////////////////////////
//  COMMAND PATTERN – Every bet-table action is a command.
//  Supports Undo (stack-based), Repeat, Clear.
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Interface
public interface IBetCommand
{
    Result Execute();
    void Undo();
}

// PlaceChipCommand
// Places a chip on a bet spot. Undo removes it.
public class PlaceChipCommand : IBetCommand
{
    private readonly Chip chip;
    private readonly BetSpot spot;

    public PlaceChipCommand(Chip chip, BetSpot spot)
    {
        this.chip = chip;
        this.spot = spot;
    }

    public Result Execute()
    {
        return spot.PlaceChip(chip);
    }

    public void Undo()
    {
        chip.ReturnToTray();         
    }
}

// RemoveChipCommand 
// Removes a chip from a spot. Undo places it back.
public class RemoveChipCommand : IBetCommand
{
    private readonly Chip chip;
    private readonly BetSpot spot;

    public RemoveChipCommand(Chip chip, BetSpot spot)
    {
        this.chip = chip;
        this.spot = spot;
    }

    public Result Execute()
    {
        chip.ReturnToTray();
        return Result.Success;
    }

    public void Undo()
    {
        spot.PlaceChip(chip);
    }
}

// ClearTableCommand
// Clears all bets. Undo restores the snapshot.
public class ClearTableCommand : IBetCommand
{
    private readonly RouletteTableLayout layout;

    // Snapshot for undo: spot ? chip values
    private readonly List<(BetSpot spot, int value)> snapshot
        = new List<(BetSpot, int)>();

    private readonly ChipTray chipTray;

    public ClearTableCommand(RouletteTableLayout layout, ChipTray chipTray)
    {
        this.layout = layout;
        this.chipTray = chipTray;
    }

    public Result Execute()
    {
        snapshot.Clear();
        foreach (var spot in layout.AllSpots)
        {
            foreach (var chip in spot.GetPlacedChips())
                snapshot.Add((spot, chip.Value));
        }

        layout.ClearAllBets();
        
        return Result.Success;
    }

    public void Undo()
    {
        foreach (var (spot, value) in snapshot)
        {
            Chip chip = chipTray.GetChipFromTray(value);
            if (chip != null) spot.PlaceChip(chip);
        }
    }
}

// RepeatBetsCommand 
// Re-places last-round bets. No meaningful Undo beyond ClearTable.
public class RepeatBetsCommand : IBetCommand
{
    private readonly List<(BetSpot spot, int value)> lastBets;
    private readonly ChipPool pool;

    public RepeatBetsCommand(List<(BetSpot, int)> lastBets, ChipPool pool)
    {
        this.lastBets = lastBets;
        this.pool = pool;
    }

    public Result Execute()
    {
        foreach (var (spot, value) in lastBets)
        {
            Chip chip = pool.Get(value);
            if (chip != null)
            {
                if(spot.PlaceChip(chip) == Result.Failure)
                {
                    return Result.Failure;
                }
            }
        }

        return Result.Success;
    }

    public void Undo()
    {
        // Simply clears freshly placed chips
        foreach (var (spot, _) in lastBets)
            spot.ClearAllChips();
    }
}

// BetCommandInvoker 
// Manages execute / undo stack.
// Call Execute(cmd) for every player action.
// Call Undo() to reverse the last action.
public class BetCommandInvoker
{
    private readonly Stack<IBetCommand> history = new Stack<IBetCommand>();

    public Result Execute(IBetCommand command)
    {
        Result result = command.Execute();
        history.Push(command);
        
        Debug.Log($"[BetCommandInvoker] Executed: {command.GetType().Name}  stack={history.Count}");
        
        return result;
    }

    public void Undo()
    {
        if (history.Count == 0)
        {
            Debug.Log("[BetCommandInvoker] Nothing to undo.");
            return;
        }

        IBetCommand cmd = history.Pop();
        cmd.Undo();
        Debug.Log($"[BetCommandInvoker] Undone: {cmd.GetType().Name}  stack={history.Count}");
    }

    public void ClearHistory() => history.Clear();
}