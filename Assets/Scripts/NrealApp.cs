using NRKernal;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NrealApp : MonoBehaviour
{
    [SerializeField] private string _websocketUrl;
    [SerializeField] private GameObject _centerAnchor;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _imageTrackedButton;

    private List<NRTrackableImage> _markers;

    private WebSocketManager _wsManager;
    private Pose _markerPose;

    private enum AppState
    {
        None,
        MarkerTracking,
        Stage
    }
    private AppState _state;

    private void Start()
    {
        _markers = new List<NRTrackableImage>();
        _state = AppState.None;
        _wsManager = new WebSocketManager(_websocketUrl);
        _wsManager.OnOpen += _wsManager_OnOpen;
        _wsManager.Connect();

        // Debug
        _imageTrackedButton.onClick.AddListener(() =>
        {
            _markerPose = new Pose
            {
                position = new Vector3(0f, 0f, -3f),
                rotation = Quaternion.identity
            };
            _wsManager.SendPose(WebSocketManager.MessageType.MarkerPose, _markerPose.position, _markerPose.rotation);
            _state = AppState.Stage;
        });
        _resetButton.onClick.AddListener(() =>
        {
            _wsManager.SendAppReset();
            _state = AppState.MarkerTracking;
        });
    }

    private void Update()
    {
        if(_state == AppState.MarkerTracking)
        {
            ImageTrack();
        }
        if(_state == AppState.Stage)
        {
            _wsManager.SendPose(WebSocketManager.MessageType.PlayerPose, _centerAnchor.transform.position, _centerAnchor.transform.rotation);
        }
    }

    private void OnApplicationQuit()
    {
        _wsManager.Close();
    }

    private void _wsManager_OnOpen()
    {
        Debug.Log($"_wsManager_OnOpen");
        EnableImageTracking();
        _wsManager.SendAppReset();
    }

    public void EnableImageTracking()
    {
        Debug.Log($"1");
        _state = AppState.MarkerTracking;
        Debug.Log($"2");
        var config = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig;
        Debug.Log($"3");
        config.ImageTrackingMode = TrackableImageFindingMode.ENABLE;
        Debug.Log($"4");
        NRSessionManager.Instance.SetConfiguration(config);
        Debug.Log($"5");
        Debug.Log($"EnableImageTracking");
    }

    public void DisableImageTracking()
    {
        Debug.Log($"DisableImageTracking");
        _state = AppState.Stage;
        var config = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig;
        config.ImageTrackingMode = TrackableImageFindingMode.DISABLE;
        NRSessionManager.Instance.SetConfiguration(config);
        _wsManager.SendPose(WebSocketManager.MessageType.MarkerPose, _markerPose.position, _markerPose.rotation);
    }

    private void ImageTrack()
    {
        Debug.Log($"ImageTrack");
#if !UNITY_EDITOR
            // Check that motion tracking is tracking.
            if (NRFrame.SessionStatus != SessionState.Running)
            {
                return;
            }
#endif
        NRFrame.GetTrackables(_markers, NRTrackableQueryFilter.New);

        foreach (var image in _markers)
        {
            Debug.Log(image.GetTrackingState());
            if (image.GetTrackingState() == TrackingState.Tracking)
            {
                _markerPose = image.GetCenterPose();
                DisableImageTracking();
                return;
            }
        }
    }
}
