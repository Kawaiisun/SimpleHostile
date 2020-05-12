using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace Com.Kawaiisun.SimpleHostile
{
    [System.Serializable]
    public class ProfileData
    {
        public string username;
        public int level;
        public int xp;

        public ProfileData()
        {
            this.username = "";
            this.level = 1;
            this.xp = 0;
        }

        public ProfileData(string u, int l, int x)
        {
            this.username = u;
            this.level = l;
            this.xp = x;
        }
    }

    [System.Serializable]
    public class MapData
    {
        public string name;
        public int scene;
    }

    public class Launcher : MonoBehaviourPunCallbacks
    {
        public InputField usernameField;
        public InputField roomnameField;
        public Text mapValue;
        public Text modeValue;
        public Slider maxPlayersSlider; 
        public Text maxPlayersValue; 
        public static ProfileData myProfile = new ProfileData();

        public GameObject tabMain;
        public GameObject tabRooms;
        public GameObject tabCreate; 

        public GameObject buttonRoom;

        public MapData[] maps;
        private int currentmap = 0;

        private List<RoomInfo> roomList;

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            myProfile = Data.LoadProfile();
            if (!string.IsNullOrEmpty(myProfile.username))
            {
                usernameField.text = myProfile.username;
            }

            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("CONNECTED!");

            PhotonNetwork.JoinLobby();
            base.OnConnectedToMaster();
        }

        public override void OnJoinedRoom()
        {
            StartGame();

            base.OnJoinedRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();

            base.OnJoinRandomFailed(returnCode, message);
        }

        public void Connect ()
        {
            Debug.Log("Trying to Connect...");
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();
        } 

        public void Join ()
        {
            PhotonNetwork.JoinRandomRoom();
        } 

        public void Create ()
        {
            RoomOptions options = new RoomOptions(); 
            options.MaxPlayers = (byte) maxPlayersSlider.value;

            options.CustomRoomPropertiesForLobby = new string[] { "map", "mode" };

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable(); 
            properties.Add("map", currentmap);
            properties.Add("mode", (int)GameSettings.GameMode);
            options.CustomRoomProperties = properties; 

            PhotonNetwork.CreateRoom(roomnameField.text, options); 
        }

        public void ChangeMap ()
        {
            currentmap++;
            if (currentmap >= maps.Length) currentmap = 0;
            mapValue.text = "MAP: " + maps[currentmap].name.ToUpper();
        }

        public void ChangeMode()
        {
            int newMode = (int)GameSettings.GameMode + 1;
            if (newMode >= System.Enum.GetValues(typeof(GameMode)).Length) newMode = 0;
            GameSettings.GameMode = (GameMode)newMode;
            modeValue.text = "MODE: " + System.Enum.GetName(typeof(GameMode), newMode);
        }

        public void ChangeMaxPlayersSlider (float t_value) 
        {
            maxPlayersValue.text = Mathf.RoundToInt(t_value).ToString();
        }

        public void TabCloseAll()
        {
            tabMain.SetActive(false);
            tabRooms.SetActive(false);
            tabCreate.SetActive(false); 
        }

        public void TabOpenMain ()
        {
            TabCloseAll();
            tabMain.SetActive(true);
        }

        public void TabOpenRooms ()
        {
            TabCloseAll();
            tabRooms.SetActive(true);
        }

        public void TabOpenCreate ()
        {
            TabCloseAll();
            tabCreate.SetActive(true);

            roomnameField.text = "";

            currentmap = 0;
            mapValue.text = "MAP: " + maps[currentmap].name.ToUpper();

            GameSettings.GameMode = (GameMode)0;
            modeValue.text = "MODE: " + System.Enum.GetName(typeof(GameMode), (GameMode)0);

            maxPlayersSlider.value = maxPlayersSlider.maxValue;
            maxPlayersValue.text = Mathf.RoundToInt(maxPlayersSlider.value).ToString();
        }

        private void ClearRoomList ()
        {
            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
            foreach (Transform a in content) Destroy(a.gameObject);
        }

        private void VerifyUsername ()
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.username = "RANDOM_USER_" + Random.Range(100, 1000);
            }
            else
            {
                myProfile.username = usernameField.text;
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> p_list)
        {
            roomList = p_list;
            ClearRoomList();

            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");

            foreach (RoomInfo a in roomList)
            {
                GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;

                newRoomButton.transform.Find("Name").GetComponent<Text>().text = a.Name;
                newRoomButton.transform.Find("Players").GetComponent<Text>().text = a.PlayerCount + " / " + a.MaxPlayers;

                if (a.CustomProperties.ContainsKey("map"))
                    newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = maps[(int)a.CustomProperties["map"]].name;
                else
                    newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = "-----";

                newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
            }

            base.OnRoomListUpdate(roomList);
        }

        public void JoinRoom (Transform p_button)
        {
            string t_roomName = p_button.Find("Name").GetComponent<Text>().text;

            VerifyUsername();

            RoomInfo roomInfo = null;
            Transform buttonParent = p_button.parent;
            for (int i = 0; i < buttonParent.childCount; i++)
            {
                if (buttonParent.GetChild(i).Equals(p_button))
                {
                    roomInfo = roomList[i];
                    break;
                }
            }

            if (roomInfo != null)
            {
                LoadGameSettings(roomInfo);
                PhotonNetwork.JoinRoom(t_roomName);
            }
        }

        public void LoadGameSettings (RoomInfo roomInfo)
        {
            GameSettings.GameMode = (GameMode)roomInfo.CustomProperties["mode"];
        }

        public void StartGame ()
        {
            VerifyUsername();

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Data.SaveProfile(myProfile);
                PhotonNetwork.LoadLevel(maps[currentmap].scene);
            }
        }
    }
}