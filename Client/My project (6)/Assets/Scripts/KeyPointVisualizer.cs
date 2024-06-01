using System.Collections.Generic;
using UnityEngine;

public class KeyPointVisualizer : MonoBehaviour
{
    public Camera mainCamera; // Assign this in the inspector to your main camera, for HoloLens this would be your AR camera
    public GameObject keypointPrefab; // Assign a prefab in the inspector for visualizing keypoints. This should be a simple visual element like a sphere or dot.
    public LineRenderer lineRendererPrefab;
    private List<GameObject> currentKeypoints = new List<GameObject>();
    private List<LineRenderer> currentLines = new List<LineRenderer>();

    private GameObject sternumInstance; // Store sternum GameObject
    private GameObject stomachInstance; // Store stomach GameObject

    void Start()
    {
        // Example call to ProcessAndVisualizeKeypoints - you might want to call this whenever you receive new keypoints data
        //ProcessAndVisualizeKeypoints();
    }

    void ClearKeypoints()
    {
        // Destroy all current keypoints
        foreach (GameObject go in currentKeypoints)
        {
            Destroy(go);
        }
        currentKeypoints.Clear(); // Clear the list
    }

    void DrawEdges(List<GameObject> keypoints)
    {
        int[,] edges = new int[,] {
            {0, 1}, {0, 2}, {1, 3}, {2, 4}, {5, 7}, {7, 9}, {6, 8}, {8, 10},
            {5, 6}, {5, 11}, {6, 12}, {11, 12}, {11, 13}, {13, 15}, {12, 14}, {14, 16}
        };

        // Clear existing lines
        foreach (var line in currentLines)
        {
            Destroy(line.gameObject);
        }
        currentLines.Clear();

        // Draw new lines
        for (int i = 0; i < edges.GetLength(0); i++)
        {
            int startIdx = edges[i, 0];
            int endIdx = edges[i, 1];

            if (startIdx < keypoints.Count && endIdx < keypoints.Count)
            {
                GameObject startPoint = keypoints[startIdx];
                GameObject endPoint = keypoints[endIdx];

                if (startPoint != null && endPoint != null) // Ensure both points are valid
                {
                    LineRenderer lineRenderer = Instantiate(lineRendererPrefab);
                    lineRenderer.SetPosition(0, startPoint.transform.position);
                    lineRenderer.SetPosition(1, endPoint.transform.position);

                    currentLines.Add(lineRenderer);
                }
            }
        }

        // Connect sternum and stomach points if they exist
        if (sternumInstance != null && stomachInstance != null)
        {
            LineRenderer lineRenderer = Instantiate(lineRendererPrefab);
            lineRenderer.SetPosition(0, sternumInstance.transform.position);
            lineRenderer.SetPosition(1, stomachInstance.transform.position);

            currentLines.Add(lineRenderer);
        }
    }

    public void ProcessAndVisualizeKeypoints(List<VideotoServer.Keypoint> keypointsData)
    {
        ClearKeypoints();

        // Check that necessary components are assigned
        if (mainCamera == null)
        {
            Debug.LogError("Camera not assigned in KeypointVisualizer.");
            return;
        }
        if (keypointPrefab == null)
        {
            Debug.LogError("Keypoint Prefab is null");
            return;
        }

        if (keypointsData.Count <= System.Math.Max(5, 6))  // Assuming indices 5 and 6 are used for shoulders
        {
            Debug.LogError("Insufficient keypoints data");
            return;
        }

        VideotoServer.Keypoint leftShoulder = keypointsData[5];
        VideotoServer.Keypoint rightShoulder = keypointsData[6];

        // Instantiate all keypoints with sufficient confidence
        foreach (VideotoServer.Keypoint keypoint in keypointsData)
        {
            if (keypoint.Confidence > 0.3)
            {
                GameObject keypointInstance = InstantiateKeypoint(keypoint);
                currentKeypoints.Add(keypointInstance);
            }
        }

        // Draw edges between keypoints
        DrawEdges(currentKeypoints);

        // Calculate and display the sternum point if both shoulders are recognized
        if (leftShoulder != null && rightShoulder != null)
        {
            VideotoServer.Keypoint sternum = CalculateAndDisplaySternumPoint(leftShoulder, rightShoulder);
            if (sternum != null)
            {
                CalculateAndDisplayStomachPoint(sternum);
            }
        }
        else
        {
            if (leftShoulder == null) Debug.Log("Left Shoulder is not recognized");
            if (rightShoulder == null) Debug.Log("Right Shoulder is not recognized");
        }
    }

    private VideotoServer.Keypoint CalculateAndDisplaySternumPoint(VideotoServer.Keypoint leftShoulder, VideotoServer.Keypoint rightShoulder)
    {
        float midpointX = (leftShoulder.X + rightShoulder.X) / 2;
        float midpointY = (leftShoulder.Y + rightShoulder.Y) / 2;
        float confidence = (leftShoulder.Confidence + rightShoulder.Confidence) / 2;

        VideotoServer.Keypoint sternum = new VideotoServer.Keypoint(midpointX, midpointY, confidence, "sternum");
        sternumInstance = InstantiateKeypoint(sternum);
        currentKeypoints.Add(sternumInstance);

        return sternum;
    }

    private void CalculateAndDisplayStomachPoint(VideotoServer.Keypoint sternum)
    {
        float stomachX = sternum.X;
        float stomachY = sternum.Y + 0.1f; // Adjust the offset as needed
        float confidence = sternum.Confidence;

        VideotoServer.Keypoint stomach = new VideotoServer.Keypoint(stomachX, stomachY, confidence, "stomach");
        stomachInstance = InstantiateKeypoint(stomach);
        currentKeypoints.Add(stomachInstance);
    }

    private GameObject InstantiateKeypoint(VideotoServer.Keypoint keypoint)
    {
        float invertedY = 1f - keypoint.Y;
        Vector3 viewportPosition = new Vector3(keypoint.X, invertedY, 0f);
        float depth = 2.0f; // Adjust as needed
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, depth));
        return Instantiate(keypointPrefab, worldPosition, Quaternion.identity);
    }
}
