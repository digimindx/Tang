using System;
using System.Threading;
using System.Threading.Tasks;
using SharpHook;
using SharpHook.Data;
using TangApp.Core.Events;
using TangApp.Core.Interfaces;

namespace TangApp.Core.Services;

public sealed class EnviroSetter : IEnvironmentManager, IDisposable
{
    private readonly IGlobalHook _globalHook;

    private Task? _hookTask;
    private int _disposed;

    private bool _leftCtrlPressed;
    private bool _rightCtrlPressed;
    private bool _leftShiftPressed;
    private bool _rightShiftPressed;
    private bool _actionTriggeredForCurrentDrag;

    private int _currentMouseX;
    private int _currentMouseY;

    public event EventHandler<HotkeyTriggeredEventArgs>? ActionTriggered;

    private bool IsCtrlPressed => _leftCtrlPressed || _rightCtrlPressed;
    private bool IsShiftPressed => _leftShiftPressed || _rightShiftPressed;
    private bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    public EnviroSetter()
    {
        _globalHook = new SimpleGlobalHook();

        _globalHook.KeyPressed += OnKeyPressed;
        _globalHook.KeyReleased += OnKeyReleased;
        _globalHook.MousePressed += OnMousePressed;
        _globalHook.MouseMoved += OnMouseMoved;
        _globalHook.MouseDragged += OnMouseDragged;
    }

    public void Initialize()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(EnviroSetter));
        }

        if (_hookTask is not null)
        {
            return;
        }

        try
        {
            _hookTask = _globalHook.RunAsync();
            _ = ObserveHookCompletionAsync(_hookTask);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to start global hooks: {ex}");
        }
    }

    private async Task ObserveHookCompletionAsync(Task hookTask)
    {
        try
        {
            await hookTask.ConfigureAwait(false);

            if (!IsDisposed)
            {
                Console.Error.WriteLine("Global hook stopped unexpectedly.");
            }
        }
        catch (Exception ex) when (!IsDisposed)
        {
            Console.Error.WriteLine($"Global hook stopped unexpectedly: {ex}");
        }
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e) =>
        UpdateModifierState(e.Data.KeyCode, true);

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e) =>
        UpdateModifierState(e.Data.KeyCode, false);

    private void UpdateModifierState(KeyCode keyCode, bool isPressed)
    {
        switch (keyCode)
        {
            case KeyCode.VcLeftControl:
                _leftCtrlPressed = isPressed;
                break;

            case KeyCode.VcRightControl:
                _rightCtrlPressed = isPressed;
                break;

            case KeyCode.VcLeftShift:
                _leftShiftPressed = isPressed;
                break;

            case KeyCode.VcRightShift:
                _rightShiftPressed = isPressed;
                break;
        }
    }

    private void OnMousePressed(object? sender, MouseHookEventArgs e)
    {
        _currentMouseX = e.Data.X;
        _currentMouseY = e.Data.Y;
        _actionTriggeredForCurrentDrag = false;
    }

    private void OnMouseMoved(object? sender, MouseHookEventArgs e)
    {
        _currentMouseX = e.Data.X;
        _currentMouseY = e.Data.Y;
    }

    private void OnMouseDragged(object? sender, MouseHookEventArgs e)
    {
        _currentMouseX = e.Data.X;
        _currentMouseY = e.Data.Y;

        if (_actionTriggeredForCurrentDrag || !IsCtrlPressed || !IsShiftPressed)
        {
            return;
        }

        _actionTriggeredForCurrentDrag = true;
        ActionTriggered?.Invoke(
            this,
            new HotkeyTriggeredEventArgs(_currentMouseX, _currentMouseY));
    }

    public void Shutdown()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _globalHook.KeyPressed -= OnKeyPressed;
        _globalHook.KeyReleased -= OnKeyReleased;
        _globalHook.MousePressed -= OnMousePressed;
        _globalHook.MouseMoved -= OnMouseMoved;
        _globalHook.MouseDragged -= OnMouseDragged;

        _globalHook.Dispose();
    }

    public void Dispose()
    {
        Shutdown();
        GC.SuppressFinalize(this);
    }
}