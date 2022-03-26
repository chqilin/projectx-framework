using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class FollowTarget : MonoBehaviour
    {
        public Camera overrideCamera;

        public float normalizedOffset = 0.5f;
        public float speed = 3f;
        public float maxAngle = 20f;

        public Vector3 screenOffset = Vector3.zero;

        public Transform targetBone;
        public Transform referenceForward;

        public bool adjustUpDirection = false;

        private Quaternion currentTargetBoneRotation;
        private Quaternion lastFinalRotation;

        // froward in local space
        private Vector3 initialTargetBoneForward;
        private Vector3 lastTargetBoneUp;
        private Vector3 animatedForward;

        private bool isTouched = false;
        private bool isFollowing = false;
        private bool isLocked = false;

        private bool isToLookAtTarget = false;
        private Transform target;
        public float modifyAngle = 0;
        private Vector3 modifyVec = Vector3.zero;

        // for rotate back:
        private bool startRotateBack = false;
        private float accT = 0;
        private float rotateSpeed = 1.25f;

        private float delay = 0.2f;

        public bool IsLookingAtTarget
        {
            get { return this.isToLookAtTarget; }
        }

        public Transform LookingTarget
        {
            get { return this.target; }
        }

        public void StartLookingAtTarget(Transform newTarget)
        {
            isToLookAtTarget = true;
            target = newTarget;

            startRotateBack = false;
        }

        public void StopLookingAtTarget()
        {
            isToLookAtTarget = false;
            target = null;

            isLocked = true;
            startRotateBack = true;
            accT = 0;
        }

        void Awake()
        {
            currentTargetBoneRotation = targetBone.localRotation;
            lastFinalRotation = targetBone.localRotation;
            initialTargetBoneForward = targetBone.parent.InverseTransformDirection(this.targetBone.forward);

            lastTargetBoneUp = targetBone.up;

            if (referenceForward != null)
            {
                modifyAngle = Vector3.Angle(targetBone.forward, referenceForward.forward) / 180.0f * Mathf.PI;
                modifyVec = targetBone.InverseTransformDirection(targetBone.forward) * Mathf.Cos(modifyAngle) - targetBone.InverseTransformDirection(referenceForward.forward);
            }
        }

        void Start()
        {
            InputDetection.instance.onPressAnyWhere.Attach(this.OnTouchAnywhere);
            InputDetection.instance.onReleaseAnyWhere.Attach(this.OnTouchReleaseAnywhere);
        }

        void OnDestroy()
        {
            InputDetection.instance.onPressAnyWhere.Detach(this.OnTouchAnywhere);
            InputDetection.instance.onReleaseAnyWhere.Detach(this.OnTouchReleaseAnywhere);
        }

        private Camera GetCamera()
        {
            bool isOverridedCamera = this.overrideCamera != null;
            Camera targetCamera = isOverridedCamera ? this.overrideCamera : Camera.main;

            return targetCamera;
        }

        public void OnTouchAnywhere(InputTouch touch)
        {
            isTouched = true;
            if (!this.isLocked && touch.fingerId == 0 && !isToLookAtTarget)
            {
                Invoke("StartFollowFinger", delay);
            }
        }

        public void SetFollowFingerDelay(float delay)
        {
            this.delay = delay;
        }

        public void StartFollowFinger()
        {
            isFollowing = true;
            lastFinalRotation = targetBone.localRotation;
            startRotateBack = false;
        }

        public void OnTouchReleaseAnywhere(InputTouch touch)
        {
            isTouched = false;
            if (!this.isLocked && touch.fingerId == 0 && !isToLookAtTarget)
            {
                this.isFollowing = false;
                RotateBoneBack();
            }

            CancelInvoke("StartFollowFinger");
        }

        public void SetLock(bool isToLock)
        {
            ToggleBoneLock(isToLock);

            if (isToLock)
            {
                isFollowing = false;
                ResetBonePosition();
            }
            else
            {
                if (isTouched) isFollowing = true;
            }
        }

        private void ToggleBoneLock(bool isLocked)
        {
            this.isLocked = isLocked;
        }

        public bool IsLocked()
        {
            return this.isLocked;
        }

        public bool IsFollowing()
        {
            return this.isFollowing;
        }

        public void MyLateUpdate()
        {
            animatedForward = targetBone.forward;
            // get local rotation right after TargetBone is animated by animtion system. 
            // this is not accurate since TargetBone may be animated.
            //initialTargetBoneRotation = targetBone.localRotation;

            if (startRotateBack)
            {
                currentTargetBoneRotation = targetBone.localRotation;

                targetBone.localRotation = lastFinalRotation;
                accT += Time.deltaTime * rotateSpeed;
                if (accT >= 1.0f)
                {
                    accT = 1.0f;

                    startRotateBack = false;
                }
                targetBone.localRotation = Quaternion.Slerp(lastFinalRotation, currentTargetBoneRotation, accT);
            }
            else
            {
                if (!this.isLocked && (isFollowing || isToLookAtTarget))
                {
                    DoFollowTarget();
                }
            }

            lastFinalRotation = targetBone.localRotation;

            lastTargetBoneUp = targetBone.up;
        }

        private void ResetBonePosition()
        {
            RotateBoneBack();
        }

        private void RotateBoneBack()
        {
            startRotateBack = true;
            accT = 0;
        }

        private void DoFollowTarget()
        {
            Vector3 target = this.GetTargetPoint();
            this.DoFollowTarget(target);
        }

        private void DoFollowTarget(Vector3 target)
        {
            targetBone.localRotation = lastFinalRotation;
            // modify target
            if (referenceForward != null)
            {
                Vector3 worldModifyVec = targetBone.TransformDirection(modifyVec);
                Vector3 tempForward = target - targetBone.position;
                worldModifyVec = tempForward.magnitude * Mathf.Tan(modifyAngle) * worldModifyVec.normalized;

                target = target + worldModifyVec;
            }
            // test: show target
            //		worldPoint.position = target;

            LookBoneAt(target);
        }

        private void LookBoneAt(Vector3 target)
        {
            LookAt(this.targetBone, initialTargetBoneForward, target);
        }

        private void LookAt(Transform bone, Vector3 boneOriginalForward, Vector3 target)
        {
            Vector3 originalForward = bone.parent.TransformDirection(boneOriginalForward);
            Vector3 boneDirection = GetBoneRotateDirection(bone, target).normalized;

            float angle = Vector3.Angle(boneDirection.normalized, originalForward);
            if (angle > maxAngle)
            {
                Vector3 targetForward = Vector3.Slerp(originalForward, boneDirection, maxAngle / angle);
                boneDirection = Vector3.RotateTowards(boneDirection, targetForward, speed * Time.deltaTime * 50, 0);
            }

            if (adjustUpDirection)
            {
                bone.LookAt(bone.position + boneDirection * 1.0f, lastTargetBoneUp);
            }
            else
            {
                //bone.forward = boneDirection;
                bone.LookAt(bone.position + boneDirection * 10f, Vector3.up);
            }
        }

        private float CalculateCameraOffset(Camera curCamera)
        {
            Vector3 cameraPosition = curCamera.transform.position;

            Vector3 targetPos = this.targetBone.position;
            float curNormalizedOffset = normalizedOffset;

            float meanOffset = Vector3.Distance(cameraPosition, targetPos);
            float offset = meanOffset * curNormalizedOffset;

            return offset;
        }

        private Vector3 GetBoneRotateDirection(Transform bone, Vector3 boneTarget)
        {
            float step = this.speed * Time.deltaTime;
            Vector3 target = boneTarget - bone.position;

            Vector3 direction = Vector3.RotateTowards(bone.forward, target.normalized, step, 0.0f);

            return direction;
        }

        private Vector3 GetTargetPoint()
        {
            if (isToLookAtTarget && target != null)
            {
                return target.position;
            }

            Vector3 mousePosition = Vector3.zero;
            Camera curCamera = GetCamera();

            if (curCamera != null)
            {
                float offset = CalculateCameraOffset(curCamera);
                Vector3 inputPosition = GetInputPosition();
                inputPosition.z = offset;

                mousePosition = curCamera.ScreenToWorldPoint(inputPosition);
            }

            return mousePosition;
        }

        private Vector3 GetInputPosition()
        {
            if (Application.isEditor)
            {
                return Input.mousePosition + new Vector3(screenOffset.x * Screen.width / 2f, screenOffset.y * Screen.height / 2f, 0f);
            }
            else
            {
                if (Input.touches.Length <= 0) return Vector3.zero;

                return (Vector3)Input.touches[0].position + new Vector3(screenOffset.x * Screen.width / 2f, screenOffset.y * Screen.height / 2f, 0f);
            }
        }
    }
}
