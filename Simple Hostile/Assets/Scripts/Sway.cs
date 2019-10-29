using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Sway : MonoBehaviour
    {
        #region Variables

        public float intensity;
        public float smooth;
        public bool isMine;

        private Quaternion origin_rotation;

        #endregion



        #region MonoBehaviour Callbacks

        private void Start()
        {
            origin_rotation = transform.localRotation;
        }

        private void Update()
        {
            if (Pause.paused) return; //!
            UpdateSway();
        }

        #endregion



        #region Private Methods

        private void UpdateSway ()
        {
            //controls
            float t_x_mouse = Input.GetAxis("Mouse X");
            float t_y_mouse = Input.GetAxis("Mouse Y");

            if(!isMine)
            {
                t_x_mouse = 0;
                t_y_mouse = 0;
            }

            //calculate target rotation
            Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
            Quaternion t_y_adj = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.right);
            Quaternion target_rotation = origin_rotation * t_x_adj * t_y_adj;

            //rotate towards target rotation
            transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);
        }

        #endregion
    }
}
