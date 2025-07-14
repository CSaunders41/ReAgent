# ReAgent

**ReAgent** is a powerful rule-based automation plugin for ExileCore, designed to provide intelligent, context-aware automation for Path of Exile through a flexible C# scripting system.

## Features

### 🎯 Rule-Based Automation
- **Dynamic C# Scripting**: Write custom rules using full C# expressions with compile-time safety
- **Context-Aware Execution**: Rules can be enabled/disabled based on game context (town, hideout, maps)
- **Real-time Compilation**: Rules are compiled on-the-fly with comprehensive error handling

### 🎮 Game State Access
- **Player Information**: Access to vitals, skills, buffs, animations, and status effects
- **Monster Detection**: Nearby monster information, counts, and filtering by rarity
- **UI State Monitoring**: Track panel visibility, chat state, and game interface status
- **Area Detection**: Automatic detection of towns, hideouts, maps, and peaceful areas

### ⚡ Side Effects System
- **Key Automation**: Press keys, hold/release key combinations
- **Visual Overlays**: Display text, graphics, and progress bars on screen
- **State Management**: Timers, flags, and numeric variables with persistence
- **Plugin Integration**: Bridge communication with other ExileCore plugins
- **Emergency Controls**: Disconnect functionality for emergency situations

### 📋 Profile Management
- **Multiple Profiles**: Organize rules into different profiles for different builds/scenarios
- **Import/Export**: Share profiles with other users via encoded strings
- **Rule Groups**: Organize rules into logical groups with individual context controls

## Installation

1. Ensure you have ExileCore installed and configured
2. Place the ReAgent plugin files in your ExileCore plugins directory
3. Enable the plugin in ExileCore settings
4. Configure your first profile and rules

## Quick Start

### Creating Your First Rule

1. **Open ReAgent Settings**: Access through ExileCore plugin menu
2. **Create a Profile**: Click the "+" button to create a new profile
3. **Add a Rule Group**: Groups help organize related rules
4. **Write Your First Rule**: 

```csharp
// Example: Use health potion when health is below 50%
Vitals.Life.Percent < 50 && Flasks.Life.HasChargesLeft
```

### Rule Types

#### Key Press Rules
Automatically press a key when conditions are met:
```csharp
// Press '1' key when health is low
Vitals.Life.Percent < 50
```

#### Single Side Effect Rules
Execute one action when conditions are met:
```csharp
// Display warning text when health is critical
Vitals.Life.Percent < 20 ? new DisplayTextSideEffect("LOW HEALTH!", new Vector2(100, 100), "Red") : null
```

#### Multiple Side Effects Rules
Execute multiple actions simultaneously:
```csharp
// Multiple actions for emergency situations
Vitals.Life.Percent < 10 ? new ISideEffect[] {
    new DisplayTextSideEffect("EMERGENCY!", new Vector2(100, 100), "Red"),
    new PressKeySideEffect(Keys.D1),
    new SetFlagSideEffect("emergency_mode")
} : null
```

## Available Side Effects

### Key Control
- `PressKeySideEffect(Keys.F1)` - Press a key once
- `StartKeyHoldSideEffect(Keys.F1)` - Hold a key down
- `ReleaseKeyHoldSideEffect(Keys.F1)` - Release a held key

### Visual Overlays
- `DisplayTextSideEffect(text, position, color)` - Show text on screen
- `DisplayGraphicSideEffect(path, position, size, tint)` - Display image
- `ProgressBarSideEffect(text, position, size, fraction, color, bgColor, textColor)` - Show progress bar

### State Management
- `SetFlagSideEffect(name)` / `ResetFlagSideEffect(name)` - Boolean flags
- `SetNumberSideEffect(name, value)` / `ResetNumberSideEffect(name)` - Numeric variables
- `StartTimerSideEffect(name)` / `StopTimerSideEffect(name)` / `ResetTimerSideEffect(name)` - Timer controls

### Advanced
- `DelayedSideEffect(seconds, actions)` - Execute actions after delay
- `DisconnectSideEffect()` - Emergency disconnect from game
- `PluginBridgeSideEffect<T>(methodName, action)` - Call other plugin methods

## Game State API

### Player Information
```csharp
// Vitals
Vitals.Life.Current         // Current life
Vitals.Life.Max             // Maximum life
Vitals.Life.Percent         // Life percentage
Vitals.Mana.Current         // Current mana
Vitals.EnergyShield.Current // Current energy shield

// Skills and Flasks
Skills.RightClick.CanUse    // Can use right-click skill
Flasks.Life.HasChargesLeft  // Health flask has charges
Flasks.Mana.HasChargesLeft  // Mana flask has charges

// Status
IsMoving                    // Player is moving
Animation                   // Current animation
Buffs.HasBuff("buff_name")  // Has specific buff
Ailments.Contains("Frozen") // Has specific ailment
```

### Monster Detection
```csharp
MonsterCount()              // Total nearby monsters
MonsterCount(50)            // Monsters within 50 units
MonsterCount(100, MonsterRarity.Rare) // Rare monsters within 100 units
Monsters(50).Count()        // Alternative syntax
```

### UI State
```csharp
IsChatOpen                  // Chat window is open
IsLeftPanelOpen            // Left panel is visible
IsRightPanelOpen           // Right panel is visible
IsAnyFullscreenPanelOpen   // Any fullscreen panel is open
```

### Area Information
```csharp
IsInTown                    // Player is in town
IsInHideout                 // Player is in hideout
IsInPeacefulArea           // Player is in peaceful area
AreaName                    // Current area name
```

### Custom State Management
```csharp
// Flags (boolean values)
IsFlagSet("my_flag")        // Check if flag is set
SetFlagSideEffect("my_flag") // Set flag to true

// Numbers (float values)
GetNumberValue("my_number") // Get numeric value
SetNumberSideEffect("my_number", 42.5f) // Set numeric value

// Timers
GetTimerValue("my_timer")   // Get timer elapsed seconds
IsTimerRunning("my_timer")  // Check if timer is running
SinceLastActivation(5.0)    // Seconds since rule last activated
```

## Configuration

### Context Controls
Each rule group can be configured for different contexts:
- **Enable**: Master enable/disable for the group
- **Enable in town**: Allow execution in town areas
- **Enable in hideout**: Allow execution in hideout
- **Enable in peaceful areas**: Allow execution in peaceful areas
- **Enable in maps**: Allow execution in maps and other areas

### Global Settings
- **Global Key Press Cooldown**: Minimum time between key presses (default: 200ms)
- **Maximum Monster Range**: Maximum range for monster detection (default: 200 units)
- **History Seconds**: How long to keep action history (default: 60 seconds)
- **Image Directory**: Directory for custom graphics (default: "textures/ReAgent")

## Custom Ailments

ReAgent includes a comprehensive ailment detection system with grouped ailments:

- **Bleeding**: Various bleeding effects
- **Burning**: Fire damage and ignite effects  
- **Chilled/Frozen**: Cold-based debuffs
- **Cursed**: All curse types
- **Poisoned**: Poison and chaos damage effects
- **Shocked**: Lightning-based debuffs
- **Exposed**: Resistance reduction effects

## Advanced Features

### Rule Compilation
ReAgent supports two syntax versions:
- **Version 1**: Dynamic LINQ expressions (legacy)
- **Version 2**: Full C# scripting with Roslyn compiler (recommended)

### Debug Mode
Enable debug mode to:
- View real-time rule evaluation
- Monitor action history
- Inspect game state
- Debug rule compilation errors

### Plugin Integration
ReAgent can communicate with other ExileCore plugins through the plugin bridge system, allowing for complex automation scenarios.

## Safety Features

- **Game State Validation**: Rules only execute when game state is valid
- **Key Press Throttling**: Configurable cooldown between key presses
- **Context Awareness**: Rules respect game context (chat open, panels visible, etc.)
- **Emergency Disconnect**: Built-in disconnect capability for emergency situations
- **Grace Period Detection**: Respects game's grace period mechanics

## Contributing

ReAgent is designed to be extensible. New side effects can be added by implementing the `ISideEffect` interface, and new game state properties can be exposed through the `RuleState` class.

## Support

If you like ReAgent, you can donate via:

**BTC**: `bc1qke67907s6d5k3cm7lx7m020chyjp9e8ysfwtuz`

**ETH**: `0x3A37B3f57453555C2ceabb1a2A4f55E0eB969105`

---

*ReAgent - Intelligent automation for Path of Exile* 
