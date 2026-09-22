using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : IDisposable
{
    private static InputManager _instance;
    public static InputManager Instance => _instance ??= new InputManager();
    private readonly MainInput _inputActions;
    public MainInput Actions => _inputActions;

    private readonly Dictionary<string, IController> _registeredControllers = new(8);
    private readonly HashSet<IController> _connectedControllers = new(8);

    public InputManager()
    {
        _inputActions = new MainInput();
        _inputActions.Enable();
    }

    public void Dispose()
    {
        foreach (var controller in _connectedControllers)
        {
            controller.OnDisconnected(_inputActions);
        }

        _connectedControllers.Clear();
        _registeredControllers.Clear();
        _inputActions?.Dispose();
    }

    /// <summary>
    /// Controller의 ID값을 통한 연결 상태 변경을 위해 ID를 등록
    /// </summary>
    public void Register(IController controller)
    {
        if (controller == null) return;

        string id = controller.ID;
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Controller ID가 비어있는 객체는 등록될 수 없습니다.");
            return;
        }

        if (_registeredControllers.ContainsKey(id))
        {
            Debug.LogWarning($"동일한 ID({id})를 가진 컨트롤러가 이미 존재합니다.");
            return;
        }

        _registeredControllers.Add(id, controller);
    }

    /// <summary>
    /// 등록된 Controller를 제거
    /// </summary>
    public void Unregister(IController controller)
    {
        if (controller == null) return;

        string id = controller.ID;
        if (!_registeredControllers.ContainsKey(id)) return;

        // 연결된 상태라면 연결 해제시키기
        if (_connectedControllers.Contains(controller))
        {
            Disconnect(controller);
        }

        _registeredControllers.Remove(id);
    }

    /// <summary>
    /// Controller에 의한 Input Event 받기
    /// </summary>
    public void Connect(string id)
    {
        if (!_registeredControllers.TryGetValue(id, out var controller))
        {
            Debug.LogWarning($"[InputManager] 등록되지 않은 Controller 입니다: {id}");
            return;
        }

        Connect(controller);
    }

    /// <summary>
    /// Controller에 의한 Input Event 받기
    /// </summary>
    public void Connect(IController controller)
    {
        if (controller == null) return;

        // 중복 연결 차단
        if (!_connectedControllers.Add(controller))
        {
            return;
        }

        controller.OnConnected(_inputActions);
    }

    /// <summary>
    /// Controller에 의한 Input Event 받기 해제
    /// </summary>
    public void Disconnect(string id)
    {
        if (!_registeredControllers.TryGetValue(id, out var controller))
        {
            Debug.LogWarning($"[InputManager] 등록되지 않은 Controller 입니다.: {id}");
            return;
        }

        Disconnect(controller);
    }

    /// <summary>
    /// Controller에 의한 Input Event 받기 해제
    /// </summary>
    public void Disconnect(IController controller)
    {
        if (controller == null) return;

        // 연결되어 있지 않다면 무시
        if (!_connectedControllers.Remove(controller))
        {
            return;
        }

        controller.OnDisconnected(_inputActions);
    }

    /// <summary>
    /// 모든 입력 일시적으로 무시
    /// </summary>
    public void InputLock()
    {
        _inputActions.Disable();

        // 마우스 입력 차단
        var evtSystem = EventSystem.current;
        if (evtSystem != null) EventSystem.current.enabled = false;
    }

    /// <summary>
    /// 모든 입력 받기
    /// </summary>
    public void InputUnlock()
    {
        _inputActions.Enable();

        // 마우스 입력 받기
        var evtSystem = EventSystem.current;
        if (evtSystem != null) EventSystem.current.enabled = true;
    }
}