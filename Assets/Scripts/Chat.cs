using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public NetworkManager network;
    public InputField inputField;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        network = GameObject.FindGameObjectWithTag("Net").GetComponent<NetworkManager>();
        network.ReceivedMessage += ReceiveMessage;
    }

    public void SendMessage()
    {
        text.text += "\n" + inputField.text;
        network.Send(inputField.text);
        inputField.text = "";
    }
    public void ReceiveMessage(string message)
    {
        text.text += "\n>" + message;
    }

}
