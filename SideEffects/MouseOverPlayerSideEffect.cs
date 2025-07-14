using System;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Numerics;
using ReAgent.State;
using SharpDX;

namespace ReAgent.SideEffects;

/// <summary>
/// Moves the mouse cursor over a specific player to highlight their name
/// This ensures skill targeting works correctly for player-targeted skills like Soul Link
/// </summary>
[DynamicLinqType]
[Api]
public record MouseOverPlayerSideEffect(string PlayerName, int HoverDurationMs = 100) : ISideEffect
{
    private DateTime _lastHoverStart = DateTime.MinValue;
    private bool _hasStartedHover = false;

    public SideEffectApplicationResult Apply(RuleState state)
    {
        try
        {
            // Find the player by name
            var targetPlayer = state.PlayerByName(PlayerName);
            if (targetPlayer == null)
            {
                return SideEffectApplicationResult.UnableToApply; // Player not found
            }

            // Check if player is within a reasonable distance
            if (targetPlayer.Distance > 100) // Within screen range
            {
                return SideEffectApplicationResult.UnableToApply; // Player too far
            }

            // Start hovering if we haven't already
            if (!_hasStartedHover)
            {
                _lastHoverStart = DateTime.Now;
                _hasStartedHover = true;
                
                // Convert player world position to screen position
                var screenPos = WorldToScreenPosition(targetPlayer.Position, state);
                if (screenPos == null)
                {
                    return SideEffectApplicationResult.UnableToApply; // Off screen
                }

                // Move mouse to player position
                MoveMouseToPosition(screenPos.Value);
                
                // Need to wait for hover duration
                if (HoverDurationMs > 0)
                {
                    return SideEffectApplicationResult.UnableToApply;
                }
            }

            // Check if we've hovered long enough
            if (DateTime.Now - _lastHoverStart < TimeSpan.FromMilliseconds(HoverDurationMs))
            {
                return SideEffectApplicationResult.UnableToApply; // Still hovering
            }

            // Hover complete, reset state
            _hasStartedHover = false;
            _lastHoverStart = DateTime.MinValue;
            
            return SideEffectApplicationResult.AppliedUnique;
        }
        catch (Exception)
        {
            // Reset state on error
            _hasStartedHover = false;
            _lastHoverStart = DateTime.MinValue;
            return SideEffectApplicationResult.UnableToApply;
        }
    }

    private Vector2? WorldToScreenPosition(Vector3 worldPos, RuleState state)
    {
        try
        {
            // Access the game controller through reflection to get camera and window info
            var gameController = GetGameController(state);
            if (gameController == null) return null;

            var camera = gameController.Game?.IngameState?.Camera;
            if (camera == null) return null;

            var windowRect = gameController.Window?.GetWindowRectangle();
            if (windowRect == null) return null;

            // Convert world position to screen position
            var screenPos = camera.WorldToScreen(worldPos);
            var finalPos = screenPos + windowRect.Value.Location;

            // Check if position is within game window bounds
            if (finalPos.X < windowRect.Value.Left || finalPos.X > windowRect.Value.Right ||
                finalPos.Y < windowRect.Value.Top || finalPos.Y > windowRect.Value.Bottom)
            {
                return null; // Off screen
            }

            return finalPos;
        }
        catch
        {
            return null;
        }
    }

    private void MoveMouseToPosition(Vector2 screenPos)
    {
        try
        {
            // Use Win32 API to set cursor position
            SetCursorPos((int)screenPos.X, (int)screenPos.Y);
        }
        catch
        {
            // Ignore mouse movement errors
        }
    }

    private object GetGameController(RuleState state)
    {
        try
        {
            // Use the internal method to access GameController
            return state.GetGameController();
        }
        catch
        {
            // Ignore access errors
        }
        return null;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    public override string ToString() => $"Mouse over player '{PlayerName}' for {HoverDurationMs}ms";
}

/// <summary>
/// Combined side effect that first hovers over a player, then casts a skill
/// Perfect for player-targeted skills like Soul Link
/// </summary>
[DynamicLinqType]
[Api]
public record MouseOverAndCastSideEffect(string PlayerName, HotkeyNodeValue SkillKey, int HoverDurationMs = 100) : ISideEffect
{
    private bool _mouseOverComplete = false;
    private readonly MouseOverPlayerSideEffect _mouseOverEffect = new(PlayerName, HoverDurationMs);
    private readonly PressKeySideEffect _castEffect = new(SkillKey);

    public SideEffectApplicationResult Apply(RuleState state)
    {
        if (!_mouseOverComplete)
        {
            var mouseOverResult = _mouseOverEffect.Apply(state);
            if (mouseOverResult == SideEffectApplicationResult.AppliedUnique)
            {
                _mouseOverComplete = true;
                // Mouse over complete, now cast the skill
                return _castEffect.Apply(state);
            }
            return mouseOverResult; // Still hovering or failed
        }
        else
        {
            // Mouse over was already complete, just cast
            var result = _castEffect.Apply(state);
            if (result == SideEffectApplicationResult.AppliedUnique)
            {
                _mouseOverComplete = false; // Reset for next use
            }
            return result;
        }
    }

    public override string ToString() => $"Mouse over '{PlayerName}' and cast {SkillKey}";
}

/// <summary>
/// Helper methods for creating mouse-over side effects
/// </summary>
[DynamicLinqType]
[Api]
public static class MouseOverHelpers
{
    /// <summary>
    /// Creates a mouse-over and cast side effect for player targeting
    /// </summary>
    [Api]
    public static ISideEffect MouseOverAndCast(string playerName, HotkeyNodeValue skillKey, int hoverMs = 100)
    {
        return new MouseOverAndCastSideEffect(playerName, skillKey, hoverMs);
    }

    /// <summary>
    /// Creates a mouse-over and cast side effect for player targeting
    /// </summary>
    [Api]
    public static ISideEffect MouseOverAndCast(string playerName, System.Windows.Forms.Keys skillKey, int hoverMs = 100)
    {
        return new MouseOverAndCastSideEffect(playerName, new HotkeyNodeValue(skillKey), hoverMs);
    }

    /// <summary>
    /// Creates a simple mouse-over side effect
    /// </summary>
    [Api]
    public static ISideEffect MouseOverPlayer(string playerName, int hoverMs = 100)
    {
        return new MouseOverPlayerSideEffect(playerName, hoverMs);
    }
} 