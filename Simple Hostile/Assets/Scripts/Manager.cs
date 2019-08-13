using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Manager : MonoBehaviour
    {
        public string player_prefab_string;
        public GameObject player_prefab;
        public Transform[] spawn_points;

        private void Start()
        {
            Spawn();
        }

        public void Spawn ()
        {
            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Instantiate(player_prefab_string, t_spawn.position, t_spawn.rotation);
            }

            else
            {
                Debug.Log("WORKING");
                GameObject newPlayer = Instantiate(player_prefab, t_spawn.position, t_spawn.rotation) as GameObject;
            }
        }
    }
}