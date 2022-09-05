using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game_Turn_Based
{
    public class CameraMovement : MonoBehaviour
    {
        public Camera cam;
        private Vector3 dragOrigin;
        // Start is called before the first frame update
        float sizeTemp = 0;
        bool isIsometric = false;
        float rotationX;
        float maxZoom, minZoom;
        void Start()
        {
            minZoom = (GridManager.Instance.getWidth() + GridManager.Instance.getHeight()) / 50.0f;
            maxZoom = (GridManager.Instance.getWidth() + GridManager.Instance.getHeight()) / 6.5f;
            cam.orthographicSize = minZoom;
            sizeTemp = cam.orthographicSize;
            this.rotationX = this.transform.rotation.eulerAngles.x;
        }

        // Update is called once per frame
        void Update()
        {
            PanCamera();
            if (cam.orthographic)
            {
                sizeTemp -= Input.GetAxis("Mouse ScrollWheel") * 5f;
                if (sizeTemp < minZoom || sizeTemp > maxZoom)
                {
                    sizeTemp = cam.orthographicSize;
                    return;
                }
                if (sizeTemp != cam.orthographicSize)
                {
                    cam.orthographicSize = sizeTemp;
                    cam.transform.position = ClampCamera(cam.transform.position);
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (!isIsometric)
                {
                    cam.transform.rotation = Quaternion.Euler(45f, 0, 0);
                    isIsometric = true;
                }
                else
                {
                    cam.transform.rotation = Quaternion.Euler(rotationX, 0, 0);
                    isIsometric = false;
                }
                cam.transform.position = ClampCamera(cam.transform.position);
            }

        }

        private void PanCamera()
        {
            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 diff = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);

                cam.transform.position += diff;
                cam.transform.position = ClampCamera(cam.transform.position);
            }
        }



        private Vector3 ClampCamera(Vector3 targetPos)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = cam.orthographicSize * cam.aspect;

            float minX = -(GridManager.Instance.getWidth()/2) + camWidth;
            float maxX = (GridManager.Instance.getWidth()/2) - camWidth;
            float minY = -(GridManager.Instance.getHeight()/2) + camHeight;
            float maxY = (GridManager.Instance.getHeight()/2) - camHeight;

            float newX = Mathf.Clamp(targetPos.x, minX, maxX);
            float newY = Mathf.Clamp(targetPos.y, minY, maxY);

            return new Vector3(newX, newY, cam.transform.position.z);
        }
    }
}