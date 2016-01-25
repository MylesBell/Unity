using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GraniteNetworkManager : NetworkManager {
    public InputField IPAddressInput;
    public InputField PortNumberInput;
    public InputField ScreenNumberInput;
    public InputField NumberOfScreensInputField;

    public void Start() {
        //check for CLI
        string[] args = System.Environment.GetCommandLineArgs();
        bool hasType = false, hasIP = false, hasPort = false, hasNumberOfScreens = false, hasScreenNumber = false, hasGameCode = false;
        string type = "", IP = "", port = "", numberOfScreens = "", screenNumber = "", gameCode = "";
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
            } else if (flag.Contains("--number-of-screens")) {
                splitFlag = flag.Split('=');
                numberOfScreens = splitFlag[1];
                hasNumberOfScreens = true;
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
                    if (hasIP && hasPort && hasNumberOfScreens && hasGameCode) StartupHost(IP, port, numberOfScreens, gameCode);
                    break;
                case "client":
                    if (hasIP && hasPort && hasScreenNumber && hasNumberOfScreens) JoinScreen(IP, port, screenNumber, numberOfScreens);
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
        NetworkManager.singleton.StartHost();
    }

    public void StartupHost(string IPAddress, string portNumber, string numberOfScreens, string gameCode) {
        SetIPAddress(IPAddress);
        SetPort(portNumber);
        SetNumberOfScreens(numberOfScreens);
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

    public void JoinScreen(string IPAddress, string portNumber, string screenNumber, string numberOfScreens) {
        SetIPAddress(IPAddress);
        SetPort(portNumber);
        SetScreen(screenNumber);
        SetLane();
        SetNumberOfScreens(numberOfScreens);
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
        SetNumberOfScreens(NumberOfScreensInputField.text);
    }

    public void SetNumberOfScreens(string numberOfScreens) {
        Debug.Log("Number of screens is: " + numberOfScreens);
        int screens = 2;
        int.TryParse(numberOfScreens, out screens);
        PlayerPrefs.SetInt("numberofscreens", screens);
    }
}
