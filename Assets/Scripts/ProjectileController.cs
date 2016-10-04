﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

namespace Filibusters
{
    public class ProjectileController : MonoBehaviour 
    {
        private Vector3 mDirection;
        public Vector3 NewDirection
        {
            set
            {
                mDirection = value;
                mDirX = value.x;
                mDirY = value.y;
            }
        }
        // TODO: Let PlayerAttack set mVel based on the bullet type
        [SerializeField]
        private float mVel;
        private float mDirX;
        private float mDirY;

        private Quaternion mCurRotation;
        private Quaternion mPrevRotation;

        private PhotonView mPhotonView;

        [SerializeField]
        private LayerMask mColLayers;
        private Vector2 mSize = Vector2.zero;
        private Vector2 mOffset = Vector2.zero;

        void Start()
        {
            mCurRotation = transform.rotation;
            mPrevRotation = mCurRotation;

            mPhotonView = GetComponent<PhotonView>();

            Vector3 scale = transform.localScale;
            BoxCollider2D bCol = GetComponent<BoxCollider2D>();
            Assert.IsNotNull(bCol, "BoxCollider2D component missing");

            mSize = new Vector2(bCol.size.x * scale.x, bCol.size.y * scale.y);
            //mOffset = new Vector2(bCol.offset.x * scale.x, bCol.offset.y * scale.y);
        }
        
        void Update() 
        {
            mCurRotation = transform.rotation;
            if (mCurRotation != mPrevRotation)
            {
                float angle = mCurRotation.eulerAngles.z - mPrevRotation.eulerAngles.z;
                NewDirection = Quaternion.AngleAxis(angle, Vector3.forward) * mDirection;
            }
            mPrevRotation = mCurRotation;

            float movement = mVel * Time.deltaTime;
            transform.Translate(movement, 0f, 0f);

            // Currently only the master can resolve collisions
            if (PhotonNetwork.isMasterClient)
            {
                DetectCollisions();
            }
        }

        void DetectCollisions()
        {
            float x = transform.position.x;
            float y = transform.position.y;
            float castDistance = mSize.x / 2f;

            Vector2 origin = new Vector2(x, y);
            Vector2 direction = new Vector2(mDirX, mDirY);

            Ray2D ray = new Ray2D(origin, direction);
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan);

            RaycastHit2D hit;
            if (hit = Physics2D.Raycast(ray.origin, ray.direction, castDistance, mColLayers))
            {
                GameObject obj = hit.transform.gameObject;
                if (obj.tag == "Player")
                {
                    // TODO: Ensure bullet doesn't hit player who created it
                    if (obj.GetComponent<PhotonView>().owner.ID != mPhotonView.owner.ID)
                    {
                        Debug.Log("Hit player");
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
