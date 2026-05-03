using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image sprite;
    [SerializeField] private GameObject spriteContainer;

    [SerializeField] private float panelFadeDuration = 0.25f;
    [SerializeField] private float panelSlideOffset = 40f;
    [SerializeField] private Ease panelEase = Ease.OutCubic;

    [SerializeField] private float spriteFadeDuration = 0.25f;

    private Coroutine _typewriterCoroutine;
    private Coroutine _transitionCoroutine;
    private bool _isTyping;
    private string _fullText;
    private Vector2 _panelStartPos;
    
    public System.Action onAutoAdvance;
    private DialogueLine _currentLine;

    public bool IsTyping => _isTyping;

    private void Awake()
    {
        _panelStartPos = dialoguePanel.anchoredPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }


    public void ShowPanel()
    {
        dialogueText.text = string.Empty;
        speakerName.text = string.Empty;
        
        
        gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        dialoguePanel.anchoredPosition = _panelStartPos + Vector2.down * panelSlideOffset;

        DOTween.Kill(canvasGroup);
        DOTween.Kill(dialoguePanel);

        canvasGroup.DOFade(1f, panelFadeDuration).SetEase(panelEase);
        dialoguePanel.DOAnchorPos(_panelStartPos, panelFadeDuration).SetEase(panelEase);
    }

    public void HidePanel(System.Action onComplete = null)
    {
        DOTween.Kill(canvasGroup);
        DOTween.Kill(dialoguePanel);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.DOFade(0f, panelFadeDuration).SetEase(panelEase);
        dialoguePanel
            .DOAnchorPos(_panelStartPos + Vector2.down * panelSlideOffset, panelFadeDuration)
            .SetEase(panelEase)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

  

    public void ShowLine(DialogueLine line)
    {
        StopTypewriter();

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        _transitionCoroutine = StartCoroutine(TransitionToLine(line));
    }

    private IEnumerator TransitionToLine(DialogueLine line)
    {
        if (line.delayBefore > 0f)
            yield return new WaitForSeconds(line.delayBefore);

        ApplyLine(line);

        if (line.typewriterSpeed > 0f)
            _typewriterCoroutine = StartCoroutine(Typewriter(line.CharacterLine, line.typewriterSpeed));
        else
            dialogueText.text = line.CharacterLine;
    }

    private void ApplyLine(DialogueLine line)
    {
        _currentLine = line;
        if (speakerName != null)
        {
            speakerName.text = line.CharacterName;
            speakerName.gameObject.SetActive(!string.IsNullOrEmpty(line.CharacterName));
        }

       
        if (sprite != null && spriteContainer != null)
        {
            bool hasSprite = line.sprite != null;
            spriteContainer.SetActive(hasSprite);

            if (hasSprite && sprite.sprite != line.sprite)
            {
                DOTween.Kill(sprite);
                sprite.DOFade(0f, 0f);
                sprite.sprite = line.sprite;
                sprite.DOFade(1f, spriteFadeDuration);
            }
        }

        dialogueText.text = string.Empty;
        _fullText = line.CharacterLine;
    }

    

    private IEnumerator Typewriter(string text, float speed)
    {
        _isTyping = true;
        dialogueText.text = string.Empty;

        float interval = 1f / speed;
        for (int i = 0; i <= text.Length; i++)
        {
            dialogueText.text = text[..i];
            yield return new WaitForSeconds(interval);
        }

        _isTyping = false;

        if (_currentLine.autoAdvanceDelay > 0f)
        {
            yield return new WaitForSeconds(_currentLine.autoAdvanceDelay);
            onAutoAdvance?.Invoke();
        }
    }

  
    public bool SkipOrConfirm()
    {
        if (_isTyping)
        {
            StopTypewriter();
            dialogueText.text = _fullText;
            return true;
        }

        return false;
    }

    private void StopTypewriter()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        _isTyping = false;
    }

    private void OnDestroy()
    {
        DOTween.Kill(canvasGroup);
        DOTween.Kill(dialoguePanel);
    }
}