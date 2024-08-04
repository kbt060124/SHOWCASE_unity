using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerFixed : MonoBehaviour
{
    float TouchZoomSpeed = 15.0f;
    float ZoomMinBound = 0.1f;
    float ZoomMaxBound = 179.9f;
    private Camera cam;
    public FixedJoystick inputMove; //左画面JoyStick
    public FixedJoystick inputRotate; //右画面JoyStick
    float moveSpeed = 5.0f; //移動する速度
    float rotateSpeed = 0.5f;  //回転する速度

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on this GameObject");
        }
    }
    void Update()
    {
        if (Input.GetKeyDown("g"))
        {
            Debug.Log("G key was pressed.");
        }
        if (Input.GetKeyDown("h"))
        {
            Debug.Log("H key was pressed.");
        }
        CameraZoom();

        //左スティックでの縦移動
        this.transform.position += this.transform.forward * inputMove.Vertical * moveSpeed * Time.deltaTime;
        //左スティックでの横移動
        this.transform.position += this.transform.right * inputMove.Horizontal * moveSpeed * Time.deltaTime;
        //右スティックでの回転（水平および垂直）
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-rotateSpeed * inputRotate.Vertical * 0.1f, rotateSpeed * inputRotate.Horizontal * 0.1f, 0));
    }

    void CameraZoom()
    {
        if (Input.touchCount == 2)
        {
            // get current touch positions
            // Touch tZero = Input.GetTouch(0);
            // Touch tOne = Input.GetTouch(1);
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);
            // get touch position from the previous frame
            Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
            Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

            float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
            float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

            // get offset value
            float deltaDistance = oldTouchDistance - currentTouchDistance;
            Zoom(deltaDistance, TouchZoomSpeed);
        }
        else if (Input.GetKeyDown("g")) // 'g' キーを押すとズームイン
        {
            Zoom(-1f, TouchZoomSpeed); // 負の値でズームイン
        }
        else if (Input.GetKeyDown("h")) // 'h' キーを押すとズームアウト
        {
            Zoom(1f, TouchZoomSpeed); // 正の値でズームアウト
        }
    }

    void Zoom(float deltaMagnitudeDiff, float speed)
    {
        cam.fieldOfView += deltaMagnitudeDiff * speed;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, ZoomMinBound, ZoomMaxBound);
    }
}