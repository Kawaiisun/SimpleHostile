using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region Variables

        public float speed;
        public float sprintModifier;
        public float crouchModifier;
        public float slideModifier;
        public float jumpForce;
        public float jetForce;
        public float jetWait;
        public float jetRecovery;
        public float lengthOfSlide;
        public int max_health;
        public float max_fuel;
        public Camera normalCam;
        public Camera weaponCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;

        public float slideAmount;
        public float crouchAmount;
        public GameObject standingCollider;
        public GameObject crouchingCollider;

        private Transform ui_healthbar;
        private Transform ui_fuelbar;
        private Text ui_ammo;

        private Rigidbody rig;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;
        private Vector3 weaponParentCurrentPos;

        private float movementCounter;
        private float idleCounter;

        private float baseFOV;
        private float sprintFOVModifier = 1.2f;
        private Vector3 origin;

        private int current_health;
        private float current_fuel;
        private float current_recovery;

        private Manager manager;
        private Weapon weapon;

        private bool crouched;

        private bool sliding;
        private float slide_time;
        private Vector3 slide_dir;

        private bool isAiming;
        private bool canJet;

        private float aimAngle;

        #endregion
        
        
        
        #region Photon Callbacks

        public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
        {
            if (p_stream.IsWriting)
            {
                p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
            }
            else
            {
                aimAngle = (int)p_stream.ReceiveNext() / 100f;
            }
        }

        #endregion



        #region MonoBehaviour Callbacks

        private void Start()
        {
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            current_health = max_health;
            current_fuel = max_fuel;

            cameraParent.SetActive(photonView.IsMine);

            if (!photonView.IsMine)
            {
                gameObject.layer = 11;
                standingCollider.layer = 11;
                crouchingCollider.layer = 11;
            }

            baseFOV = normalCam.fieldOfView;
            origin = normalCam.transform.localPosition;

            if(Camera.main) Camera.main.enabled = false;

            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;
            weaponParentCurrentPos = weaponParentOrigin;


            if (photonView.IsMine)
            {
                ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;
                ui_fuelbar = GameObject.Find("HUD/Fuel/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                RefreshHealthBar();
            }
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                RefreshMultiplayerState();
                return;
            }

            //Axles
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");


            //Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool crouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
            bool pause = Input.GetKeyDown(KeyCode.Escape);


            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.15f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;
            bool isCrouching = crouch && !isSprinting && !isJumping && isGrounded;


            //Pause
            if(pause)
            {
                GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
            }

            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                crouch = false;
                pause = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
                isCrouching = false;
            }


            //Crouching
            if (isCrouching)
            {
                photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
            }


            //Jumping
            if (isJumping)
            {
                if(crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                rig.AddForce(Vector3.up * jumpForce);
                current_recovery = 0f;
            }

            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(100);


            //Head Bob
            if (sliding)
            {
                //sliding
                HeadBob(movementCounter, 0.15f, 0.075f);
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }
            else if (t_hmove == 0 && t_vmove == 0)
            {
                //idling
                HeadBob(idleCounter, 0.01f, 0.01f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (!isSprinting && !crouched)
            {
                //walking
                HeadBob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * 6f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else if(crouched)
            {
                //crouching
                HeadBob(movementCounter, 0.02f, 0.02f);
                movementCounter += Time.deltaTime * 4f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                //sprinting
                HeadBob(movementCounter, 0.15f, 0.055f);
                movementCounter += Time.deltaTime * 13.5f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }

            //UI Refreshes
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_ammo);
        }

        void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            //Axles
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");


            //Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool slide = Input.GetKey(KeyCode.LeftControl);
            bool aim = Input.GetMouseButton(1);
            bool jet = Input.GetKey(KeyCode.Space);


            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;
            bool isSliding = isSprinting && slide && !sliding;
            isAiming = aim && !isSliding && !isSprinting;


            //Pause
            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
                isSliding = false;
                isAiming = false;
            }


            //Movement
            Vector3 t_direction = Vector3.zero;
            float t_adjustedSpeed = speed;

            if (!sliding)
            {
                t_direction = new Vector3(t_hmove, 0, t_vmove);
                t_direction.Normalize();
                t_direction = transform.TransformDirection(t_direction);

                if (isSprinting)
                {
                    if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                    t_adjustedSpeed *= sprintModifier;
                }
                else if (crouched)
                {
                    t_adjustedSpeed *= crouchModifier;
                }
            }
            else
            {
                t_direction = slide_dir;
                t_adjustedSpeed *= slideModifier;
                slide_time -= Time.deltaTime;
                if(slide_time <= 0)
                {
                    sliding = false;
                    weaponParentCurrentPos -= Vector3.down * (slideAmount - crouchAmount);
                }
            }

            Vector3 t_targetVelocity = t_direction * t_adjustedSpeed * Time.deltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


            //Sliding
            if (isSliding)
            {
                sliding = true;
                slide_dir = t_direction;
                slide_time = lengthOfSlide;
                weaponParentCurrentPos += Vector3.down * (slideAmount - crouchAmount);
                if (!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true);
            }


            //Jetting
            if (jump && !isGrounded)
                canJet = true;
            if (isGrounded)
                canJet = false;

            if (canJet && jet && current_fuel > 0)
            {
                rig.AddForce(Vector3.up * jetForce * Time.fixedDeltaTime, ForceMode.Acceleration);
                current_fuel = Mathf.Max(0, current_fuel - Time.fixedDeltaTime);
            }

            if(isGrounded)
            {
                if (current_recovery < jetWait)
                    current_recovery = Mathf.Min(jetWait, current_recovery + Time.fixedDeltaTime);
                else
                    current_fuel = Mathf.Min(max_fuel, current_fuel + Time.fixedDeltaTime * jetRecovery);
            }

            ui_fuelbar.localScale = new Vector3(current_fuel / max_fuel, 1, 1);

            
            //Aiming
            weapon.Aim(isAiming);


            //Camera Stuff
            if (sliding)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.15f, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.15f, Time.deltaTime * 8f);

                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin + Vector3.down * slideAmount, Time.deltaTime * 6f);
                weaponCam.transform.localPosition = Vector3.Lerp(weaponCam.transform.localPosition, origin + Vector3.down * slideAmount, Time.deltaTime * 6f);
            }
            else
            {
                if (isSprinting)
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
                    weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
                }
                else if (isAiming)
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * weapon.currentGunData.mainFOV, Time.deltaTime * 8f);
                    weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * weapon.currentGunData.weaponFOV, Time.deltaTime * 8f);
                }
                else
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
                    weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
                }

                if (crouched)
                {
                    normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                    weaponCam.transform.localPosition = Vector3.Lerp(weaponCam.transform.localPosition, origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                }
                else
                {
                    normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin, Time.deltaTime * 6f);
                    weaponCam.transform.localPosition = Vector3.Lerp(weaponCam.transform.localPosition, origin, Time.deltaTime * 6f);
                }
            }
        }

        #endregion

        #region Private Methods

        void RefreshMultiplayerState ()
        {
            float cacheEulY = weaponParent.localEulerAngles.y;

            Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
            weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

            Vector3 finalRotation = weaponParent.localEulerAngles;
            finalRotation.y = cacheEulY;

            weaponParent.localEulerAngles = finalRotation;
        }

        void HeadBob (float p_z, float p_x_intensity, float p_y_intensity)
        {
            float t_aim_adjust = 1f;
            if (isAiming) t_aim_adjust = 0.1f;
            targetWeaponBobPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z) * p_x_intensity * t_aim_adjust, Mathf.Sin(p_z * 2) * p_y_intensity * t_aim_adjust, 0);
        }

        void RefreshHealthBar ()
        {
            float t_health_ratio = (float)current_health / (float)max_health;
            ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }

        [PunRPC]
        void SetCrouch (bool p_state)
        {
            if (crouched == p_state) return;

            crouched = p_state;

            if (crouched)
            {
                standingCollider.SetActive(false);
                crouchingCollider.SetActive(true);
                weaponParentCurrentPos += Vector3.down * crouchAmount;
            }

            else
            {
                standingCollider.SetActive(true);
                crouchingCollider.SetActive(false);
                weaponParentCurrentPos -= Vector3.down * crouchAmount;
            }
        }

        #endregion

        #region Public Methods
        
        public void TakeDamage (int p_damage)
        {
            if (photonView.IsMine)
            {
                current_health -= p_damage;
                RefreshHealthBar();

                if(current_health <= 0)
                {
                    manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        #endregion
    }
}
