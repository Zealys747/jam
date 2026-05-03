using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;

    
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private string playerTag = "Player";
    
    
    
    private bool _triggered = false;
    bool _playerInRange = false;
    
    public void ResetTrigger() => _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
        }
    }

    private void Update()
    {
      
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("жмется кайфы");
            Trigger();
        }
    }

    public void Trigger()
    {
        Debug.Log("триггеред");
        Debug.Log("Dialogue manager: "+ DialogueManager.instance);
        if (triggerOnlyOnce && _triggered) return;

        if (DialogueManager.instance == null) return;

        _triggered = true;
        DialogueManager.instance.StartDialogue(dialogueData);
    }
    
    
}
