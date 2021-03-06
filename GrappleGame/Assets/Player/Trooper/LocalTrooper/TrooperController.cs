﻿using Cinemachine;
using Server;
using SharedFiles.Utility;
using UI;
using UnityEngine;

namespace Player.Trooper.LocalTrooper
{
    public class TrooperController : MonoBehaviour
    {
        public static TrooperController instance;
        
        public Trooper trooper;
        public new Rigidbody rigidbody;
        public new CapsuleCollider collider;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }
            
            trooper = GetComponent<Trooper>();
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
        }
        
        public bool grounded = false;
        public bool nearGround = false;
        public Vector3 groundNormal;

        [SerializeField] CinemachineFreeLook cinemachineFreeLook;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] float sensitivity = 100f;
        [SerializeField] float jetpackSpeed = 1f;
        [SerializeField] float magnetBootsForce = 10f;
        [SerializeField] float walkSpeed = 10f;
        [SerializeField] private int groundCheckDirections = 20;
        [SerializeField] private float groundCheckMaxDistance = 0.2f;
        [SerializeField] private float nearGroundCheckMaxDistance = 0.5f;

        private void Start()
        {
            float height = collider.height;
            groundCheckMaxDistance += height / 2;
            nearGroundCheckMaxDistance += height / 2;
        }
        
        void FixedUpdate()
        {
            Vector3 input = Vector3.zero;
            float mouseVertical = 0.0f;
            float mouseHorizontal = 0.0f;
            
            if (UIManager.instance.lastActivePanel.panelType == UIManager.instance.GetCurrentInGamePanel())
            {
                // Getting movment inputs only if in Game
                input = Vector3.zero;
                if (Input.GetKey(KeyCode.W)) { 
                    input.z -= 1;
                }
                if (Input.GetKey(KeyCode.S)) { input.z += 1; }
                if (Input.GetKey(KeyCode.A)) { input.x += 1; }
                if (Input.GetKey(KeyCode.D)) { input.x -= 1; }
                input = transform.rotation * -input;
                
                mouseVertical = -Input.GetAxis("Mouse Y");
                mouseHorizontal = Input.GetAxis("Mouse X");
            }
            
            
            // Update Grounded, nearGround and groundNormal
            grounded = false;
            nearGround = false;
            groundNormal = Vector3.zero;
            float shortestDistance = 100.0f;

            foreach (Vector3 direction in GrappleMath.GetSphereDirections(groundCheckDirections))
            {
                if (!Physics.Raycast(transform.position, direction, out RaycastHit groundHitInfo,
                    nearGroundCheckMaxDistance, groundLayers)) continue;
                
                nearGround = true;
                if (groundHitInfo.distance < groundCheckMaxDistance) { grounded = true; }
                
                if (groundHitInfo.distance >= shortestDistance) continue;
                shortestDistance = groundHitInfo.distance;
                groundNormal = groundHitInfo.normal;
            }

            // Performing Movment and Rotation
            if (grounded)
            {
                // Rotation on Ground
                var rotation = transform.rotation;
                rotation = Quaternion.FromToRotation(rotation * Vector3.up, groundNormal) * rotation;
                transform.rotation = rotation;
                transform.Rotate(0f, mouseHorizontal * sensitivity * Time.deltaTime, 0f);

                // Movment on Ground
                Vector3 speed = input * walkSpeed;
                rigidbody.velocity = speed;
            }
            else
            {
                // Rotaion in Space
                transform.Rotate(mouseVertical * sensitivity * Time.deltaTime, mouseHorizontal * sensitivity * Time.deltaTime, 0f);

                // Movment in Space
                Vector3 speed = input * jetpackSpeed;
                rigidbody.AddForce(speed);
            }

            if (nearGround)
            {
                rigidbody.AddForce(-groundNormal * magnetBootsForce);
            }
            else
            {
                cinemachineFreeLook.m_YAxis.m_InputAxisValue = 0.0f;
            }
            
            // Updating depending varibles
            
            cinemachineFreeLook.m_YAxis.m_InputAxisName = 
                grounded && 
                UIManager.instance.lastActivePanel.panelType == UIManager.instance.GetCurrentInGamePanel() ? 
                    "Mouse Y" : "";
            cinemachineFreeLook.m_YAxisRecentering.m_enabled = !nearGround;
            
            trooper.grounded = grounded;
            trooper.velocity = rigidbody.velocity;

            // Sending transform to Server
            ClientSend.TrooperTransformUpdate(trooper);
        }
    }
}
