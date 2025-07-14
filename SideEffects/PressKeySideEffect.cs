using System;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Windows.Forms;
using ExileCore;
using ReAgent.State;

namespace ReAgent.SideEffects;

/// <summary>
/// Presses a key with optional modifiers.
/// Now uses InputCoordinator for better plugin coordination.
/// </summary>
[DynamicLinqType]
[Api]
public record PressKeySideEffect(Keys Key, Keys[]? Modifiers = null) : ISideEffect
{
    public SideEffectApplicationResult Apply(RuleState state)
    {
        try
        {
            // Try to get input control via InputCoordinator
            var gameController = state.GetGameController();
            var requestControl = gameController?.GetType()
                .GetProperty("PluginBridge")?
                .GetValue(gameController)?
                .GetType()
                .GetMethod("GetMethod")?
                .MakeGenericMethod(typeof(Func<string, int, bool>))?
                .Invoke(gameController.GetType().GetProperty("PluginBridge").GetValue(gameController), 
                    new object[] { "InputCoordinator.RequestControl" }) as Func<string, int, bool>;

            bool hasControl = requestControl?.Invoke("ReAgent", 300) ?? true; // Default to true if no coordinator

            if (!hasControl)
            {
                // Another plugin has control, skip this action
                return SideEffectApplicationResult.ConditionalFailure;
            }

            // Press modifiers first
            if (Modifiers != null)
            {
                foreach (var modifier in Modifiers)
                {
                    Input.KeyDown(modifier);
                }
            }

            // Press the main key
            Input.KeyDown(Key);
            Input.KeyUp(Key);

            // Release modifiers
            if (Modifiers != null)
            {
                foreach (var modifier in Modifiers.Reverse())
                {
                    Input.KeyUp(modifier);
                }
            }

            return SideEffectApplicationResult.Success;
        }
        catch (Exception ex)
        {
            // Log error via shared logger if available
            var gameController = state.GetGameController();
            var logError = gameController?.GetType()
                .GetProperty("PluginBridge")?
                .GetValue(gameController)?
                .GetType()
                .GetMethod("GetMethod")?
                .MakeGenericMethod(typeof(Action<string>))?
                .Invoke(gameController.GetType().GetProperty("PluginBridge").GetValue(gameController), 
                    new object[] { "PluginLogger.LogError" }) as Action<string>;

            logError?.Invoke($"[ReAgent] PressKeySideEffect failed: {ex.Message}");

            return SideEffectApplicationResult.Failure;
        }
    }
}

[DynamicLinqType]
[Api]
public record StartKeyHoldSideEffect(HotkeyNodeValue Key) : ISideEffect
{
    public StartKeyHoldSideEffect(Keys key) : this(new HotkeyNodeValue(key))
    {
    }

    public SideEffectApplicationResult Apply(RuleState state)
    {
        if (state.InternalState.KeysToHoldDown.Contains(Key))
        {
            return SideEffectApplicationResult.AppliedDuplicate;
        }

        state.InternalState.KeysToHoldDown.Add(Key);
        return SideEffectApplicationResult.AppliedUnique;
    }

    public override string ToString() => $"Start holding key {Key}";
}

[DynamicLinqType]
[Api]
public record ReleaseKeyHoldSideEffect(HotkeyNodeValue Key) : ISideEffect
{
    public ReleaseKeyHoldSideEffect(Keys key) : this(new HotkeyNodeValue(key))
    {
    }

    public SideEffectApplicationResult Apply(RuleState state)
    {
        if (state.InternalState.KeysToRelease.Contains(Key))
        {
            return SideEffectApplicationResult.AppliedDuplicate;
        }

        state.InternalState.KeysToRelease.Add(Key);
        return SideEffectApplicationResult.AppliedUnique;
    }

    public override string ToString() => $"Release key {Key}";
}