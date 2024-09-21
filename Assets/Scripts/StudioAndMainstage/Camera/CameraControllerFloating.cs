using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerFloating : MonoBehaviour
{
    float TouchZoomSpeed = 15.0f;
    float ZoomMinBound = 0.1f;
    float ZoomMaxBound = 179.9f;
    private Camera cam;
    public FloatingJoystick inputMove; //左画面JoyStick
    public FloatingJoystick inputRotate; //右画面JoyStick
    float moveSpeed = 5.0f; //移動する速度
    float rotateSpeed = 0.5f;  //回転する速度

    private GameObject room;
    private Vector3 roomMin;
    private Vector3 roomMax;

    private GameObject floor;
    private GameObject wallBack;
    private GameObject wallFront;
    private GameObject ceiling;
    private GameObject wallLeft;
    private GameObject wallRight;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on this GameObject");
        }

        room = GameObject.Find("Room");
        if (room == null)
        {
            Debug.LogError("Room object not found in the scene");
        }
        else
        {
            FindRoomComponents();
            CalculateRoomBounds();
        }
    }

    void FindRoomComponents()
    {
        floor = room.transform.Find("Floor")?.gameObject;
        wallBack = room.transform.Find("WallBack")?.gameObject;
        wallFront = room.transform.Find("WallFront")?.gameObject;
        ceiling = room.transform.Find("Ceiling")?.gameObject;
        wallLeft = room.transform.Find("WallLeft")?.gameObject;
        wallRight = room.transform.Find("WallRight")?.gameObject;

        if (floor == null || wallBack == null || wallFront == null || ceiling == null || wallLeft == null || wallRight == null)
        {
            Debug.LogError("One or more room components are missing");
        }
    }

    void CalculateRoomBounds()
    {
        if (floor == null || wallBack == null || wallFront == null || ceiling == null || wallLeft == null || wallRight == null)
        {
            Debug.LogError("Cannot calculate room bounds due to missing components");
            return;
        }

        Bounds roomBounds = new Bounds(room.transform.position, Vector3.zero);

        // 各壁のBoundsを計算
        Bounds floorBounds = floor.GetComponent<Renderer>().bounds;
        Bounds wallBackBounds = wallBack.GetComponent<Renderer>().bounds;
        Bounds wallFrontBounds = wallFront.GetComponent<Renderer>().bounds;
        Bounds ceilingBounds = ceiling.GetComponent<Renderer>().bounds;
        Bounds wallLeftBounds = wallLeft.GetComponent<Renderer>().bounds;
        Bounds wallRightBounds = wallRight.GetComponent<Renderer>().bounds;

        // 壁の厚さを計算
        float floorThickness = floorBounds.size.y;
        float wallBackThickness = wallBackBounds.size.z;
        float wallFrontThickness = wallFrontBounds.size.z;
        float ceilingThickness = ceilingBounds.size.y;
        float wallLeftThickness = wallLeftBounds.size.x;
        float wallRightThickness = wallRightBounds.size.x;

        // roomMinとroomMaxを計算
        roomMin = new Vector3(
            wallLeftBounds.max.x + wallLeftThickness,
            floorBounds.max.y + floorThickness,
            wallFrontBounds.max.z + wallFrontThickness
        );

        roomMax = new Vector3(
            wallRightBounds.min.x - wallRightThickness,
            ceilingBounds.min.y - ceilingThickness,
            wallBackBounds.min.z - wallBackThickness
        );

        // Debug.Log($"Room bounds calculated: Min {roomMin}, Max {roomMax}");
    }

    void LateUpdate()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, roomMin.x, roomMax.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, roomMin.y, roomMax.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, roomMin.z, roomMax.z);
        transform.position = clampedPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown("g"))
        {
            // Debug.Log("G key was pressed.");
        }
        if (Input.GetKeyDown("h"))
        {
            // Debug.Log("H key was pressed.");
        }
        CameraZoom();

        Vector3 movement = Vector3.zero;
        //左スティックでの縦移動
        movement += this.transform.forward * inputMove.Vertical * moveSpeed * Time.deltaTime;
        //左スティックでの横移動
        movement += this.transform.right * inputMove.Horizontal * moveSpeed * Time.deltaTime;

        // 移動を適用する前に境界チェックを行う
        Vector3 newPosition = transform.position + movement;
        newPosition.x = Mathf.Clamp(newPosition.x, roomMin.x, roomMax.x);
        newPosition.y = Mathf.Clamp(newPosition.y, roomMin.y, roomMax.y);
        newPosition.z = Mathf.Clamp(newPosition.z, roomMin.z, roomMax.z);
        transform.position = newPosition;

        //右スティックでの回転（水平および垂直）
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-rotateSpeed * inputRotate.Vertical * 1f, rotateSpeed * inputRotate.Horizontal * 1f, 0));
    }

    void CameraZoom()
    {
        return;

        // if (Input.touchCount == 2)
        // {
        //     // get current touch positions
        //     // Touch tZero = Input.GetTouch(0);
        //     // Touch tOne = Input.GetTouch(1);
        //     Touch tZero = Input.GetTouch(0);
        //     Touch tOne = Input.GetTouch(1);
        //     // get touch position from the previous frame
        //     Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
        //     Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

        //     float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
        //     float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

        //     // get offset value
        //     float deltaDistance = oldTouchDistance - currentTouchDistance;
        //     Zoom(deltaDistance, TouchZoomSpeed);
        // }
        // else if (Input.GetKeyDown("g")) // 'g' キーを押すとズームイン
        // {
        //     Zoom(-1f, TouchZoomSpeed); // 負の値でズームイン
        // }
        // else if (Input.GetKeyDown("h")) // 'h' キーを押すとズームアウト
        // {
        //     Zoom(1f, TouchZoomSpeed); // 正の値でズームアウト
        // }
    }

    void Zoom(float deltaMagnitudeDiff, float speed)
    {
        cam.fieldOfView += deltaMagnitudeDiff * speed;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, ZoomMinBound, ZoomMaxBound);
    }
}

