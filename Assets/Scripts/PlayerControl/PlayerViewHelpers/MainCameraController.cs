using System;
using UnityEditor.Rendering;
using UnityEngine;
[System.Serializable]
public class CameraController
{
    [Header("Controlled Objects")]
    private Camera main_cam;
    private Transform MainCameraHolder;
    private RectTransform player_screen;

    [Header("Camera Render Data")]
    [SerializeField] float camera_zoom_time = 0.5f;
    [SerializeField] float base_zoom_level = 1.5f; // MUST BE ATLEAST 1!!!!
    [SerializeField] float zoom_factor = 0.25f;
    float lerp_amount = 1;
    float zoom_diff; // set current zoom
    float curr_zoom; // set current zoom
    [field: SerializeField] public float target_zoom {get; private set;} // set current zoom
    [SerializeField] float player_range; // player look range (based on weapons and base stats)
    float curr_zoom_time;
    [SerializeField] public const float c_max_zoom = 5;

    [Header("Camera Positioning")]
    Vector2 source_position;
    Vector2 target_position;
    [Header("Camera Recoil")]
    readonly Vector2 player_screen_pos;
    float max_recoil_time = 1;
    float curr_recoil_time;
    [SerializeField] float max_recoil_drift_distance = 1.5f; // basically the recoil intensity
    [SerializeField] float max_recovery_speed = 5;
    [SerializeField] float min_recovery_speed = 0.5f;
    float curr_recovery_speed = 2;
    Vector2 recoil_direction;
    Vector2 curr_recoil_direction;
    Action CamMovement;
    //[Header("Classmates")]
    // private PlayerViewController player_view_controller;

    #region Setup & Reset
    public CameraController(Transform CameraHolder, Camera main_cam, RectTransform player_screen)
    {
        this.main_cam = main_cam;
        this.player_screen = player_screen;
        MainCameraHolder = CameraHolder;

        // initialize values
        target_zoom = 1;
        curr_zoom = (int)target_zoom;
        zoom_diff = 0;
        target_zoom = base_zoom_level;

        player_screen_pos = player_screen.gameObject.transform.position;

        CamMovement = SetCameraBetweenPositions;
    }

    #endregion

    #region Updates
    // update the data for the camera every frame
    public void UpdateCamData(Vector2 source_position, Vector2 target_position)
    {
        this.source_position = source_position;
        this.target_position = target_position;
    }

    // every late update (called by player controller), use the data to update how the camera looks
    public void UpdateCamRender()
    {
        // adjust zoom if necessary
        if (curr_zoom_time < camera_zoom_time)
        {
            curr_zoom_time += Time.deltaTime;
            if (curr_zoom_time >= camera_zoom_time)
            {
                curr_zoom_time = camera_zoom_time;
                player_screen.localScale = new Vector3(target_zoom, target_zoom, 0);
            }
        }
        
        if (curr_zoom_time > 0)
        {
            float scale_Val = curr_zoom + zoom_diff * curr_zoom_time/camera_zoom_time; 
            player_screen.localScale = new Vector3(scale_Val, scale_Val, 0);
        }
        // adjust position with SetCameraBetweenPositions()
        UpdateCameraRecoil();
        CamMovement();
    }
    #endregion

    #region Camera Modifiers  
    public void SetCameraZoom(int zoom_scalar)
    {
        player_range = zoom_scalar;
        curr_zoom = player_screen.localScale.x;
        curr_zoom_time = 0;
        target_zoom = base_zoom_level + (c_max_zoom - zoom_scalar) * zoom_factor;
        zoom_diff = target_zoom - player_screen.localScale.x;
    }
    public void ApplyCameraRecoil(Vector2 direction, float recoil_amount)
    {
        curr_recoil_time += max_recoil_time * recoil_amount;
        curr_recoil_time = Mathf.Min(curr_recoil_time, max_recoil_time);

        
        recoil_direction = (recoil_direction + direction).normalized * max_recoil_drift_distance * curr_recoil_time/max_recoil_time;
        curr_recoil_direction = recoil_direction;
    }
    public void UpdateCameraRecoil()
    {
        curr_recoil_time = Mathf.Max(0, curr_recoil_time - Time.deltaTime * curr_recovery_speed);
        curr_recoil_direction = Vector2.Lerp(Vector2.zero, recoil_direction, curr_recoil_time/max_recoil_time);
        curr_recovery_speed = Mathf.Lerp(min_recovery_speed, max_recovery_speed, curr_recoil_time/max_recoil_time);

        player_screen.gameObject.transform.position = player_screen_pos + curr_recoil_direction;
    }

    public void SetCameraBetweenPositions()
    {
        // update cam position to move to look position within circular bounds
        float cam_range = 5 + 1.5f * player_range;
        Vector2 offset = (target_position - source_position) * 0.1f;
        if (offset.sqrMagnitude > cam_range * cam_range)
        {
            offset = offset.normalized * cam_range;
        }

        Vector3 cam_pos = source_position + offset;
        cam_pos.z = -10;

        main_cam.transform.position = cam_pos;
    }

    public void SetCameraAtPosition()
    {

        Vector3 cam_pos = source_position;
        cam_pos.z = -10;
        main_cam.transform.position = cam_pos;
    }
    #endregion
}