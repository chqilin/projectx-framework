using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class Rotate : MonoBehaviour
    {
        public Vector3 axis;
        public float rotateSpeed = 10.0f;

        void Update()
        {
            transform.Rotate(axis, rotateSpeed * Time.deltaTime);
        }
    }
}
