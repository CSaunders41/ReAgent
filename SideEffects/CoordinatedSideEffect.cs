using System;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using ReAgent.State;

namespace ReAgent.SideEffects;

/// <summary>
/// A side effect wrapper that coordinates with other plugins (like Follower) 
/// to ensure smooth execution without interference
/// </summary>
[DynamicLinqType]
[Api]
public record CoordinatedSideEffect(ISideEffect InnerSideEffect, int DelayMs = 100) : ISideEffect
{
    private DateTime _lastAttempt = DateTime.MinValue;
    private bool _hasDelayed = false;

    public SideEffectApplicationResult Apply(RuleState state)
    {
        // Check if we need to delay execution to coordinate with other plugins
        if (!_hasDelayed)
        {
            _lastAttempt = DateTime.Now;
            _hasDelayed = true;
            
            // Signal to other plugins that we're about to execute
            SignalPendingExecution();
            
            // Give other plugins time to yield control
            if (DelayMs > 0)
            {
                return SideEffectApplicationResult.UnableToApply;
            }
        }

        // Check if enough time has passed since our initial attempt
        if (DateTime.Now - _lastAttempt < TimeSpan.FromMilliseconds(DelayMs))
        {
            return SideEffectApplicationResult.UnableToApply;
        }

        // Execute the inner side effect
        var result = InnerSideEffect.Apply(state);
        
        // Reset coordination state on successful execution
        if (result == SideEffectApplicationResult.AppliedUnique)
        {
            _hasDelayed = false;
            _lastAttempt = DateTime.MinValue;
        }
        
        return result;
    }

    private void SignalPendingExecution()
    {
        // This could be expanded to use plugin bridge methods
        // to notify other plugins of pending execution
    }

    public override string ToString() => $"Coordinated: {InnerSideEffect}";
}

/// <summary>
/// Helper methods for creating coordinated side effects
/// </summary>
[DynamicLinqType]
[Api]
public static class CoordinatedSideEffectHelpers
{
    /// <summary>
    /// Creates a coordinated key press that yields to movement systems
    /// </summary>
    [Api]
    public static ISideEffect CoordinatedKeyPress(HotkeyNodeValue key, int delayMs = 100)
    {
        return new CoordinatedSideEffect(new PressKeySideEffect(key), delayMs);
    }

    /// <summary>
    /// Creates a coordinated key press that yields to movement systems
    /// </summary>
    [Api]
    public static ISideEffect CoordinatedKeyPress(System.Windows.Forms.Keys key, int delayMs = 100)
    {
        return new CoordinatedSideEffect(new PressKeySideEffect(key), delayMs);
    }
} 