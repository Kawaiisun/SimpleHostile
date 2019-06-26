using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Motion : MonoBehaviourPunCallbacks
    {

        #region Variables

        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public Camera normalCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;

        private Rigidbody rig;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;

        private float movementCounter;
        private float idleCounter;

        private float baseFOV;
        private float sprintFOVModifier = 1.5f;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            cameraParent.SetActive(photonView.IsMine);

            baseFOV = normalCam.fieldOfView;

            if(Camera.main) Camera.main.enabled = false;

            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            //Axles
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");


            //Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);


            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;


            //Jumping
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce);
            }


            //Head Bob
            if (t_hmove == 0 && t_vmove == 0)
            {
                HeadBob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (!isSprinting)
            {
                HeadBob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                HeadBob(movementCounter, 0.15f, 0.075f);
                movementCounter += Time.deltaTime * 7f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }
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


            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;


            //Movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            float t_adjustedSpeed = speed;
            if (isSprinting) t_adjustedSpeed *= sprintModifier;

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.deltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


            //Field of View
            if (isSprinting) { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f); }
            else { normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f); ; }
        }

        #endregion

        #region Private Methods

        void HeadBob (float p_z, float p_x_intensity, float p_y_intensity)
        {
            targetWeaponBobPosition = weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z * 2) * p_y_intensity, 0);
        }

        #endregion
    }
}
