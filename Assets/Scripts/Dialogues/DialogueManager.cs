using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance {get; private set;}
    
    [SerializeField] private DialogueUI dialogueUI;

    [SerializeField]private bool advanceOnClick = true;
    [SerializeField] private bool advanceOnSpace = true;
    [SerializeField] private float panelFadeDuration = 0.25f;
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
    public UnityEvent<int> onLineChanged; // сюда айди текущей реплики идет

    private DialogueData _currentData;
    private int _currentLineIndex;
    private bool _inputLocked;
    private bool _isActive;

    [SerializeField] private float advanceCooldown = 1f;
    
    
    
    
    public bool IsActive() => _isActive;
    public int CurrentLineIndex() => _currentLineIndex;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        dialogueUI.onAutoAdvance = Advance;
    }

    private void Update()
    {
        if (!_isActive || _inputLocked) return;
        bool advance = false;

        if (advanceOnSpace && Keyboard.current != null)
        {
            advance =advance || Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        if (advanceOnClick && Mouse.current != null)
        {
            advance = advance || Mouse.current.leftButton.wasPressedThisFrame;
        }

        if (advance)
        {
            bool skipped = dialogueUI.SkipOrConfirm();
            if (skipped)
            {
                
            }
            else
            {
                Advance();
                LockInput();
            }
        }
    }

    private void LockInput()
    {
        _inputLocked = true;
        DOVirtual.DelayedCall(panelFadeDuration + 0.5f, () => _inputLocked = false);
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.Log("А ТЕКСТ ГДЕ?");
            return;
        }
        
        _currentData = data;
        _currentLineIndex = 0;
        _isActive = true;
       
        LockInput();

        dialogueUI.ShowPanel();
        ShowCurrentLine();

       
        onDialogueStart?.Invoke();
    }

    public void Advance()
    {
        if (!_isActive) return;

        
        
        _currentLineIndex++;

        if (_currentLineIndex >= _currentData.lines.Length)
        {
            EndDialogue();
            return;
        }
        
        ShowCurrentLine();
    }

    public void EndDialogue()
    {
        _isActive = false; 
        _currentData = null;
        
        dialogueUI.HidePanel(() => onDialogueEnd?.Invoke());
    }

    public void JumpToLine(int index)
    {
        if (_currentData == null || index < 0 || index >= _currentData.lines.Length) return;
        
        _currentLineIndex = index;
        
        ShowCurrentLine();
    }
    
    
    private void ShowCurrentLine()
    {
        dialogueUI.ShowLine(_currentData.lines[_currentLineIndex]);
        onLineChanged?.Invoke(_currentLineIndex);
    }
}
