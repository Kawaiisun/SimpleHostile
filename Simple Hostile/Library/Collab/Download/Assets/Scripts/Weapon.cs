using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Weapon : MonoBehaviour
    {
        #region Variables

        public Gun[] loadout;
        public Transform weaponParent;
        public LayerMask canBeShot;
        public GameObject bullethole;

        private int currentIndex;
        private GameObject currentWeapon;

        #endregion

        #region MonoBehaviour Callbacks

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);

            if (currentWeapon != null)
            {
                Aim(Input.GetMouseButton(1));

                if(Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
            }
        }

        #endregion

        #region Private Methods

        void Equip(int p_ind)
        {
            if (currentWeapon != null) Destroy(currentWeapon);

            currentIndex = p_ind;

            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;

            currentWeapon = t_newWeapon;
        }

        void Aim (bool p_isAiming)
        {
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");

            if (p_isAiming)
            {
                //aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
            else
            {
                //hip
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
        }

        void Shoot ()
        {
            Transform t_camera = transform.Find("Cameras/Normal Camera");

            RaycastHit t_hit = new RaycastHit();
            if(Physics.Raycast(t_camera.transform.position, t_camera.transform.forward, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newbullethole = Instantiate(bullethole, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity);
                t_newbullethole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newbullethole, 5f);
            }
        }

        #endregion
    }
}
