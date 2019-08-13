using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variables

        public Gun[] loadout;
        public Transform weaponParent;
        public GameObject bulletholePrefab;
        public LayerMask canBeShot;
        public bool isAiming = false;

        private float currentCooldown;
        private int currentIndex;
        private GameObject currentWeapon;

        private bool isReloading;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            foreach (Gun a in loadout) a.Initialize();
            Equip(0);
        }

        void Update()
        {
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) { photonView.RPC("Equip", RpcTarget.All, 0); }
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2)) { photonView.RPC("Equip", RpcTarget.All, 1); }

            if (currentWeapon != null)
            {
                if (photonView.IsMine)
                {
                    Aim(Input.GetMouseButton(1));

                    if (loadout[currentIndex].burst != 1)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
                        {
                            if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else StartCoroutine(Reload(loadout[currentIndex].reload));
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCooldown <= 0)
                        {
                            if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else StartCoroutine(Reload(loadout[currentIndex].reload));
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload(loadout[currentIndex].reload));

                    //cooldown
                    if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
                }

                //weapon position elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            }
        }

        #endregion

        #region Private Methods

        IEnumerator Reload (float p_wait)
        {
            isReloading = true;
            currentWeapon.SetActive(false);

            yield return new WaitForSeconds(p_wait);

            loadout[currentIndex].Reload();
            currentWeapon.SetActive(true);
            isReloading = false;
        }

        [PunRPC]
        void Equip(int p_ind)
        {
            if (currentWeapon != null)
            {
                if(isReloading) StopCoroutine("Reload");
                Destroy(currentWeapon);
            }

            currentIndex = p_ind;

            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

            currentWeapon = t_newWeapon;
        }

        void Aim (bool p_isAiming)
        {
            isAiming = p_isAiming;
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

        [PunRPC]
        void Shoot ()
        {
            Transform t_spawn = transform.Find("Cameras/Normal Camera");

            //bloom
            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();

            //cooldown
            currentCooldown = loadout[currentIndex].firerate;
            
            //raycast
            RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newHole = Instantiate(bulletholePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, 5f);

                if(photonView.IsMine)
                {
                    //shooting other player on network
                    if(t_hit.collider.gameObject.layer == 11)
                    {
                        t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage); //!
                    }
                }
            }

            //gun fx
            currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;
        }

        [PunRPC]
        private void TakeDamage (int p_damage)
        {
            GetComponent<Player>().TakeDamage(p_damage);
        }

        #endregion

        #region Public Methods

        public void RefreshAmmo (Text p_text)
        {
            int t_clip = loadout[currentIndex].GetClip();
            int t_stache = loadout[currentIndex].GetStash();

            p_text.text = t_clip.ToString("D2") + " / " + t_stache.ToString("D2");
        }

        #endregion
    }
}
