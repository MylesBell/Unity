using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GraniteNetworkManager : NetworkManager {
    public InputField IPAddressInput;
    public InputField PortNumberInput;
    public InputField ScreenNumberInput;
    public InputField NumberOfScreensInputField;

    public void StartupHost()
    {
        SetIPAddress();
        SetPort();
        SetNumberOfScreens();
        PlayerPrefs.SetInt("screen", 0);
        PlayerPrefs.SetInt("isServer", 1);
        NetworkManager.singleton.StartHost();
    }

    public void JoinScreen()
    {
        SetIPAddress();
        SetPort();
        SetScreen();
        SetLane();
        SetNumberOfScreens();
        PlayerPrefs.SetInt("isServer", 0);
        NetworkManager.singleton.StartClient();
    }
    public void SetIPAddress()
    {
        string ipAddress = IPAddressInput.text;
        Debug.Log("IP Address is: " + ipAddress);
        NetworkManager.singleton.networkAddress = ipAddress;
    }

    public void SetPort()
    {
        string portNumber = PortNumberInput.text;
        int port = -1;
        int.TryParse(portNumber, out port);
        Debug.Log("Port Number is: " + port);
        NetworkManager.singleton.networkPort = port;
    }
    public void SetLane()
    {

    }
    public void SetScreen()
    {
        string screenNumber = ScreenNumberInput.text;
        Debug.Log("Screen Number is: " + screenNumber);
        int screen = -1;
        int.TryParse(screenNumber, out screen);
        PlayerPrefs.SetInt("screen", screen);
    }

    public void SetNumberOfScreens()
    {
        string screenNumber = NumberOfScreensInputField.text;
        Debug.Log("Number of screens is: " + screenNumber);
        int screen = 2;
        int.TryParse(screenNumber, out screen);
        PlayerPrefs.SetInt("numberofscreens", screen);
    }
}
