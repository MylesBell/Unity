using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GraniteNetworkManager : NetworkManager {
    public InputField IPAddressInput;
    public InputField PortNumberInput;
    public InputField ScreenNumberInput;
    public InputField NumberOfScreensLeftInputField;
    public InputField NumberOfScreensRightInputField;
    public InputField GameCodeInputField;
    
    public Dropdown ClientLaneDropdown;
    
    public static int screeNumber = -1;
    public static bool isServer = false;
    public static ComputerLane lane = 0;
    
    public static int numberOfScreens_left = -1;
    public static int numberOfScreens_right = -1;
    
    public static string game_code;
    

    public void Start() {
        //reset 
        PlayerPrefs.DeleteAll();
        //check for CLI
        string[] args = System.Environment.GetCommandLineArgs();
        bool hasType = false, hasIP = false, hasPort = false, hasNumberOfScreensLeft = false, hasNumberOfScreensRight = false, hasScreenNumber = false, hasGameCode = false, hasLane = false;
        string type = "", IP = "", port = "", numberOfScreensLeft = "0", numberOfScreensRight = "0", screenNumber = "", gameCode = "", lane = "";
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
            } else if (flag.Contains("--lane")) {
                splitFlag = flag.Split('=');
                lane = splitFlag[1];
                hasLane = true;
            }
        }
        if (hasType) {
            switch (type) {
                case "host":
                    if (hasIP && hasPort && (hasNumberOfScreensLeft || hasNumberOfScreensRight) && hasGameCode) StartupHost(IP, port, numberOfScreensLeft, numberOfScreensRight, gameCode);
                    break;
                case "client":
                    if (hasIP && hasPort && hasScreenNumber && (hasNumberOfScreensLeft || hasNumberOfScreensRight) && hasLane) JoinScreen(IP, port, numberOfScreensLeft, numberOfScreensRight, screenNumber, lane);
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
        SetGameCode();
        screeNumber = 0;
        isServer = true;
        lane = numberOfScreens_left > numberOfScreens_right ? ComputerLane.LEFT : ComputerLane.RIGHT;
        NetworkManager.singleton.maxConnections = numberOfScreens_left + numberOfScreens_right + 1;
        NetworkManager.singleton.StartHost();
    }

    public void StartupHost(string IPAddress, string portNumber, string numberOfScreensLeft, string numberOfScreensRight, string gameCode) {
        SetIPAddress(IPAddress);
        SetPort(portNumber);
        SetNumberOfScreens(numberOfScreensLeft, true);
        SetNumberOfScreens(numberOfScreensRight, false);
        screeNumber = 0;
        isServer = true;
        SetGameCode(gameCode);
        NetworkManager.singleton.maxConnections = numberOfScreens_left + numberOfScreens_right + 1;
        NetworkManager.singleton.StartHost();
    }

    public void JoinScreen() {
        SetIPAddress();
        SetPort();
        SetScreen();
        SetNumberOfScreens();
        isServer = false;
        SetLane();
        NetworkManager.singleton.StartClient();
    }

    public void JoinScreen(string IPAddress, string portNumber, string numberOfScreensLeft, string numberOfScreensRight, string screenNumber, string string_lane) {
        SetIPAddress(IPAddress);
        SetPort(portNumber);
        SetScreen(screenNumber);
        SetNumberOfScreens(numberOfScreensLeft, true);
        SetNumberOfScreens(numberOfScreensRight, false);
        isServer = false;
        lane = string_lane.Equals("right", StringComparison.OrdinalIgnoreCase) ? ComputerLane.RIGHT : ComputerLane.LEFT;
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

    public void SetScreen() {
        SetScreen(ScreenNumberInput.text);
    }

    public void SetScreen(string screenNumber) {
        Debug.Log("Screen Number is: " + screenNumber);
        int screen = -1;
        int.TryParse(screenNumber, out screen);
        screeNumber = screen;
    }

    public void SetGameCode() {
        SetGameCode(GameCodeInputField.text);
    }

    public void SetGameCode(string gameCode) {
        Debug.Log("Game code is: " + gameCode);
        game_code = gameCode;
    }

    public void SetNumberOfScreens() {
        SetNumberOfScreens(NumberOfScreensLeftInputField.text, true);
        SetNumberOfScreens(NumberOfScreensRightInputField.text, false);
    }

    public void SetNumberOfScreens(string numberOfScreens, bool isLeft) {
        // Debug.Log("Number of screens, side " + side + " is: " + numberOfScreens);
        int screens = 0;
        int.TryParse(numberOfScreens, out screens);
        if(isLeft) numberOfScreens_left = screens;
        else       numberOfScreens_right = screens;
    }
    
    public void SetLane(){
        //0 is left and 1 is right
        int selectedLane = ClientLaneDropdown.value;
        lane = selectedLane == 0 ? ComputerLane.LEFT : ComputerLane.RIGHT;
    }
}
