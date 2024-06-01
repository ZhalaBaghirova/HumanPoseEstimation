using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Linq;

public class CameraCapture : MonoBehaviour
{
    public float captureIntervalSeconds = 0.033f; // Approximately 30 fps
    private bool isCapturing = false;

    public VideotoServer sendToServer; // Assign this in the Inspector

    private PhotoCapture photoCaptureObject = null;

    void Start()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;
        Debug.Log("Photo Capture Object Created");
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).First();
        CameraParameters cameraParameters = new CameraParameters
        {
            hologramOpacity = 0.0f,
            cameraResolutionWidth = cameraResolution.width,
            cameraResolutionHeight = cameraResolution.height,
            pixelFormat = CapturePixelFormat.BGRA32
        };

        captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            isCapturing = true;
            InvokeRepeating("CapturePhoto", 0f, captureIntervalSeconds);
            Debug.Log("Photo mode started successfully.");
        }
        else
        {
            Debug.LogError("Unable to start photo mode! Error Code: " + result.hResult.ToString("X"));
        }
    }

    void CapturePhoto()
    {
        if (!isCapturing || photoCaptureObject == null)
        {
            Debug.LogError("CapturePhoto called but photo capturing is not ready.");
            return;
        }

        photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (!result.success || photoCaptureFrame == null)
        {
            Debug.LogError("Failed to capture photo: " + result.hResult.ToString("X"));
            return;
        }

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).First();
        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        byte[] imageBytes = targetTexture.EncodeToJPG();

        StartCoroutine(sendToServer.SendFrameToServer(imageBytes));
        Destroy(targetTexture);  // Consider reusing the texture instead of destroying and recreating every frame
    }

    void StopCapture()
    {
        if (isCapturing)
        {
            isCapturing = false;
            CancelInvoke("CapturePhoto");
            photoCaptureObject?.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Photo mode stopped successfully.");
        }
        else
        {
            Debug.LogError("Failed to stop photo mode: " + result.hResult.ToString("X"));
        }
        photoCaptureObject?.Dispose();
        photoCaptureObject = null;
    }
}
