using UnityEngine;
using UnityEngine.InputSystem;

public class BurnerPlayerStabilizer : MonoBehaviour
{
    [Header("Стабилизация игроком (WASD)")]
    [SerializeField] private bool enablePlayerStabilization = true;
    [SerializeField] private float playerTiltStrength = 9f;
    [SerializeField] private float playerTiltResponse = 30f;
    
    [Tooltip("Если W/S наклоняют не в ту сторону, включи")]
    [SerializeField] private bool invertForwardBack = false;
    [Tooltip("Если A/D наклоняют не в ту сторону, включи")]
    [SerializeField] private bool invertLeftRight = false;

    private Vector2 _current;
    private bool _init;

    public void Init() => _init = true;

    public Vector2 CalculateTilt(float dt)
    {
        if (!_init || !enablePlayerStabilization) return Vector2.zero;

        Vector2 input = ReadInput();
        float fwdSign = invertForwardBack ? -1f : 1f;
        float sideSign = invertLeftRight ? -1f : 1f;

        Vector2 target = new Vector2(input.y * fwdSign, -input.x * sideSign) * Mathf.Max(0f, playerTiltStrength);

        float resp = Mathf.Max(0f, playerTiltResponse);
        _current = resp <= 0f ? target : Vector2.MoveTowards(_current, target, resp * dt);
        return _current;
    }

    private static Vector2 ReadInput()
    {
        Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null)
        {
            float x = (k.aKey.isPressed || k.leftArrowKey.isPressed ? -1f : 0f) + (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f);
            float y = (k.sKey.isPressed || k.downArrowKey.isPressed ? -1f : 0f) + (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f);
            v = new Vector2(x, y);
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (v == Vector2.zero) v = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        return Vector2.ClampMagnitude(v, 1f);
    }
}