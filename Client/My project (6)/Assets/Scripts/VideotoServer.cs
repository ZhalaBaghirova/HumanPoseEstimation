using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class VideotoServer : MonoBehaviour
{
    //public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;
    public KeyPointVisualizer visualizer;


    void Start()
    {

        // Set the VideoPlayer to API Only mode
        Debug.Log("VideoToServer: Start called");
        // videoPlayer.sendFrameReadyEvents = true;
        //videoPlayer.frameReady += FrameReady;
        //videoPlayer.prepareCompleted += PrepareCompleted;

        //videoPlayer.Prepare();
        //Debug.Log("prepare");

    }
    /*
    void PrepareCompleted(VideoPlayer vp)
    {
        Debug.Log("VideoToServer: Video prepared, starting playback");
        // Start the video
        vp.Play();
    
        Debug.Log("3");
    }
    void FrameReady(VideoPlayer vp, long frame)
    {
        Debug.Log("frame ready ishledi");
        // Capture the current frame into a Texture2D
        Texture2D frameTexture = ToTexture2D(renderTexture);

        // Convert the texture to a byte array
        byte[] imageBytes = frameTexture.EncodeToJPG();
        // Send the byte array to the server as a coroutine
        StartCoroutine(SendFrameToServer(imageBytes));
    }
    
    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Debug.Log("texture  ishledi");

        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }

  */
    public IEnumerator SendFrameToServer(byte[] imageBytes)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("frame", imageBytes, "frame.jpg", "image/jpeg"));

        UnityWebRequest www = UnityWebRequest.Post("https://127.0.0.1:8000/process_frame/", formData);
        www.certificateHandler = new BypassCertificateHandler();

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("Network or HTTP Error: " + www.error);
            yield break; // Stop execution on error
        }

        Debug.Log("Frame sent successfully: " + www.downloadHandler.text);

        string jsonResponse = www.downloadHandler.text;
        Debug.Log("JSON Response: " + jsonResponse);

        if (string.IsNullOrEmpty(jsonResponse))
        {
            Debug.LogError("Error: JSON response is null or empty.");
            yield break; // Stop execution on bad response
        }

        // Try to deserialize the JSON response into the ApiResponse object and handle any exceptions
        try
        {
            ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
            Debug.Log("Deserialized ApiResponse object: " + response);
            Debug.Log("Keypoints accessed successfully."); // Log success message after successful deserialization

            if (response == null || response.Keypoints == null)
            {
                Debug.LogError("Deserialization failed or keypoints are null.");
                yield break; // Stop execution if deserialization fails or keypoints are null
            }

            List<Keypoint> keyPointsList = new List<Keypoint>();
            Debug.Log("Keypoints initialized. Total persons detected: " + response.Keypoints.Length);

            foreach (var person in response.Keypoints)
            {
                foreach (var pose in person)
                {
                    foreach (var keypointArray in pose)
                    {
                        if (keypointArray.Length >= 3)
                        {
                            keyPointsList.Add(new Keypoint(keypointArray[1], keypointArray[0], keypointArray[2]));
                            Debug.Log($"Keypoint - X: {keypointArray[1]}, Y: {keypointArray[0]}, Confidence: {keypointArray[2]}");
                        }
                    }
                }
            }

            if (visualizer != null && keyPointsList.Count > 0)
            {
                visualizer.ProcessAndVisualizeKeypoints(keyPointsList);
            }
            else
            {
                Debug.LogError("Visualizer not assigned or no keypoints to visualize.");
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError("JSON Exception: " + ex.Message);
            Debug.LogError("JSON Response: " + jsonResponse);  // Log the raw JSON response on exception
        }
        finally
        {
            www.certificateHandler.Dispose();
        }
    }

    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // Override the SSL certificate verification
            return true; // WARNING: Use this for testing only; it's insecure for production.
        }
    }

    public class ApiResponse
    {
        public float[][][][] Keypoints { get; set; }
    }
    public class Keypoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Confidence { get; set; }

        public Keypoint(float x, float y, float confidence, string description = "")
        {
            X = x;
            Y = y;
            Confidence = confidence;
        }
    }



}
