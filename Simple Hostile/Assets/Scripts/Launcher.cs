using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Com.Kawaiisun.SimpleHostile
{
    public class ProfileData
    {
        public string username;
        public int level;
        public int xp;
    }

    public class Launcher : MonoBehaviourPunCallbacks
    {
        public InputField usernameField;
        public static ProfileData myProfile = new ProfileData();

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("CONNECTED!");

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
            PhotonNetwork.CreateRoom("");
        }

        public void StartGame ()
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.username = "RANDOM_USER_" + Random.Range(100, 1000);
            }
            else
            {
                myProfile.username = usernameField.text;
            }

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LoadLevel(1);
            }
        }
    }
}