using TMPro;
using UnityEngine;

public class MessageController : MonoBehaviour
{
    [SerializeField] TMP_Text messageText = default;
    [SerializeField] Animator textAnimator = default;
    [SerializeField] public Vector3 location;
    private void Start()
    {
        //messageText = this.GetComponentInChildren<TMP_Text>();
        //textAnimator = this.GetComponentInChildren<Animator>();
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        textAnimator.SetTrigger("Message");
    }
}
