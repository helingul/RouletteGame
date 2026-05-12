# Roulette Game

## Gameplay

[![Demo Video](https://img.youtube.com/vi/z4DP5hNVo_g/0.jpg)](https://www.youtube.com/watch?v=z4DP5hNVo_g)

In this roulette game, the chips shown in the bottom bar can be placed onto the table. Adding chips to the table does not immediately reduce the player's balance. Clicking a chip creates a new chip instance on the table. The table also includes a chip limit system to prevent excessive chip stacking.

When dragging a chip across the table, all valid betting spots are highlighted. Once released, the chip automatically snaps to the nearest available betting area.

The game supports all standard roulette betting types, including:

### Inside Bets
- Straight
- Split
- Street
- Corner
- Six Line

### Outside Bets
- Red / Black
- Even / Odd
- High / Low
- Dozens
- Columns

When a chip is successfully placed onto a betting area, the chip value is deducted from the player's balance.

To start the game, press the **Spin** button located in the bottom-right corner of the screen.

Before spinning, you can also press the **Select Winning Number** button to manually choose the winning number. If no number is selected, the game automatically generates a random winning number.

The project does not include a save/load system. However, the following gameplay statistics are tracked and displayed:
- Total number of spins played
- Current player balance
- Total winnings since the game started

---

# Design Patterns Used

## Singleton Pattern
`RouletteGameManager` acts as the main controller responsible for managing the overall game flow.

Since this project is primarily a demo/prototype, a singleton architecture was chosen for faster iteration and easier global access. However, the core gameplay systems are intentionally designed with minimal dependency on the singleton to reduce tight coupling.

---

## State Pattern
Game flow and transitions are managed using the State Pattern architecture.

`RouletteGameManager` acts as the central state controller and handles transitions between gameplay states such as betting, spinning, and result evaluation.

---

## Strategy Pattern
Bet evaluation and payout calculation are implemented using the Strategy Pattern.

Currently, the project uses a single payout strategy because roulette payouts are based on fixed multipliers. However, the system is designed to be easily extendable for alternative roulette rulesets or payout systems in the future.

---

## Command Pattern
Undo functionality is implemented using the Command Pattern.

This allows gameplay actions such as:
- Placing chips
- Removing chips
- Returning chips back to the table

to be reverted cleanly through the UI undo system.

---

## Observer Pattern / Event Bus
A centralized event-driven communication system was implemented using `RouletteEventBus`.

This architecture allows gameplay systems and UI components to communicate with minimal direct dependencies, improving modularity and maintainability.

---

## Factory Pattern + Object Pooling
Chip creation is handled through a Factory Pattern architecture.

Since chips are spawned frequently during gameplay, object pooling is also used to improve runtime performance and reduce unnecessary memory allocations and instantiations.

---

# General Notes

To simplify scene setup and reduce repetitive manual work, the project includes procedural systems for automatically generating roulette wheel slots and betting spots.

---

## SlotMarkerGenerator

`SlotMarkerGenerator` is responsible for generating the roulette wheel slot markers automatically.

By pressing the **CreateRouletteSlots** button from the Tools section, all wheel slot markers are procedurally created and aligned with the roulette wheel mesh.

This approach eliminates the need to manually position each slot marker individually.

For simplicity, the generated values are currently embedded directly into the game. In a production-ready version, these values would be exposed through editable tools or custom editor interfaces.

---

## RouletteTableLayout

`RouletteTableLayout` automatically generates all betting spots on the roulette table.

The system uses configuration data stored inside the `RouletteTableData` ScriptableObject.

To generate the table layout:
1. Attach `RouletteTableLayout` to a GameObject
2. Press the **Generate All Bet Spots** button from the Inspector

All betting areas will then be generated automatically.