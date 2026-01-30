using Chunks;
using UnityEngine;

namespace Common
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float zoomSpeed, moveSpeed, rotateSpeed;
        [SerializeField] private Transform trs;
        [SerializeField] private Camera cam;
        [SerializeField] private VoxelManager voxelManager;

        private float zoom = 1f;
        private Vector3 origin;

        void Start()
        {
            origin = new Vector3(voxelManager.size * 0.5f, 0, voxelManager.size * 0.5f);
            trs.forward = (origin - trs.position).normalized;
        }

        void Update()
        {
            float dz = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(dz) > 0.00001f)
            {
                float delta = zoomSpeed * Time.deltaTime * Mathf.Sign(dz);
                cam.orthographicSize -= delta;
                zoom += delta * 0.1f;
            }

            Vector2 mouseDelta = new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")).normalized;

            if (Input.GetButton("Fire2") && mouseDelta.magnitude > 0.00001f)
            {
                Vector3 delta = moveSpeed * Time.deltaTime * (mouseDelta.x * trs.right + mouseDelta.y * trs.up) / zoom;
                trs.position += delta;
                origin += delta;
            }

            if (Input.GetButton("Fire3") && mouseDelta.magnitude > 0.00001f)
            {
                Vector3 delta = rotateSpeed * Time.deltaTime * (mouseDelta.x * trs.right + mouseDelta.y * trs.up) / (zoom * 0.5f);
                trs.position = origin + (trs.position + delta - origin).normalized * (origin - trs.position).magnitude;
                trs.forward = (origin - trs.position).normalized;
            }
        }
    }
}