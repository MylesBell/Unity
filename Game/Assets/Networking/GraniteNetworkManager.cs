using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GraniteNetworkManager : NetworkManager {
    public InputField IPAddressInput;
    public InputField PortNumberInput;
    public InputField ScreenNumberInput;
    public InputField NumberOfScreensInputField;

    public void Start() {
        //reset 
        PlayerPrefs.DeleteAll();
        //check for CLI
        string[] args = System.Environment.GetCommandLineArgs();
        bool hasType = false, hasIP = false, hasPort = false, hasNumberOfScreensLeft = false, hasNumberOfScreensRight = false, hasScreenNumber = false, hasGameCode = false;
        string type = "", IP = "", port = "", numberOfScreensLeft = "0", numberOfScreensRight = "0", screenNumber = "", gameCode = "";
        foreach (string flag in args) {
            string[] splitFlag;
            if (flag.Contains("--type")) {
                splitFlag = flag.Split('=');
                type = splitFlag[1];
                hasType = true;
            } else if (flag.Contains("--ip")) {
                splitFlag = flag.Split('=');
                IP = splitFlag[1];
                hasIP = true;
            } else if (flag.Contains("--port")) {
                splitFlag = flag.Split('=');
                port = splitFlag[1];
                hasPort = true;
            } else if (flag.Contains("--number-of-screens-left")) {
                splitFlag = flag.Split('=');
                numberOfScreensLeft = splitFlag[1];
                hasNumberOfScreensLeft = true;
            } else if (flag.Contains("--number-of-screens-right")) {
                splitFlag = flag.Split('=');
                numberOfScreensRight = splitFlag[1];
                hasNumberOfScreensRight = true;
            } else if (flag.Contains("--screen-number")) {
                splitFlag = flag.Split('=');
                screenNumber = splitFlag[1];
                hasScreenNumber = true;
            } else if (flag.Contains("--game-code")) {
                splitFlag = flag.Split('=');
                gameCode = splitFlag[1];
                hasGameCode = true;
            }
        }
        if (hasType) {
            switch (type) {
                case "host":
                    if (hasIP && hasPort && (hasNumberOfScreensLeft || hasNumberOfScreensRight) && hasGameCode) StartupHost(IP, port, numberOfScreensLeft, numberOfScreensRight, gameCode);
                    break;
                case "client":
                    if (hasIP && hasPort && hasScreenNumber && (hasNumberOfScreensLeft || hasNumberOfScreensRight)) JoinScreen(IP, port, numberOfScreensLeft, numberOfScreensRight, screenNumber);
                    break;
                default:
                    Debug.Log("Running GUI\n");
                    break;
            }
        }
    }

    public void StartupHost() {
        SetIPAddress();
        SetPort();
        SetNumberOfScreens();
        PlayerPrefs.SetInt("screen", 0);
        PlayerPrefs.SetInt("isServer", 1);
        PlayerPrefs.SetString("gameCode", "ABCD");
        NetworkManager.singleton.StartHost();
    }

    public void StartupHost(string IPAddress, string portNumber, string numberOfScreensLeft, string numberOfScreensRight, string gameCode) {
        SetIPAddress(IPAddress);
        SetPort(portNumber);
        SetNumberOfScreens(numberOfScreensLeft, "left");
        SetNumberOfScreens(numberOfScreensRight, "right");
        PlayerPrefs.SetInt("screen", 0);
        PlayerPrefs.SetInt("isServer", 1);
        PlayerPrefs.SetString("gameCode", gameCode);
        NetworkManager.singleton.StartHost();
    }

    public void JoinScreen() {
        SetIPAddress();
        SetPort();
        SetScreen();
        SetLane();
        SetNumberOfScreens();
        PlayerPrefs.SetInt("isServer", 0);
        NetworkManager.singleton.StartClient();
    }

    public void JoinScreen(string IPAddress, string portNumber, string numberOfScreensLeft, string numberOfScreensRight, string screenNumber) {
        SetIPAddress(IPAddress);
        SetPort(portNumber);
        SetScreen(screenNumber);
        SetLane();
        SetNumberOfScreens(numberOfScreensLeft, "left");
        SetNumberOfScreens(numberOfScreensRight, "right");
        PlayerPrefs.SetInt("isServer", 0);
        NetworkManager.singleton.StartClient();
    }

    public void SetIPAddress() {
        SetIPAddress(IPAddressInput.text);
    }

    public void SetIPAddress(string ipAddress) {
        Debug.Log("IP Address is: " + ipAddress);
        NetworkManager.singleton.networkAddress = ipAddress;
    }

    public void SetPort() {
        SetPort(PortNumberInput.text);
    }

    public void SetPort(string portNumber) {
        int port = -1;
        int.TryParse(portNumber, out port);
        Debug.Log("Port Number is: " + port);
        NetworkManager.singleton.networkPort = port;
    }
    public void SetLane() {

    }

    public void SetScreen() {
        SetScreen(ScreenNumberInput.text);
    }

    public void SetScreen(string screenNumber) {
        Debug.Log("Screen Number is: " + screenNumber);
        int screen = -1;
        int.TryParse(screenNumber, out screen);
        PlayerPrefs.SetInt("screen", screen);
    }

    public void SetNumberOfScreens() {
        SetNumberOfScreens(NumberOfScreensInputField.text, "left");
    }

    public void SetNumberOfScreens(string numberOfScreens, string side) {
        Debug.Log("Number of screens, side " + side + " is: " + numberOfScreens);
        int screens = 0;
        int.TryParse(numberOfScreens, out screens);
        PlayerPrefs.SetInt("numberofscreens-" + side, screens);
    }
}
