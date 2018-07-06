using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using Windows.Kinect;
using System.Text;
using UnityEngine.Video;

public class KinectManager : MonoBehaviour {

    // Kinect
    private KinectSensor kinectSensor;
    private BodyFrameReader bodyFrameReader;
    private int bodyCount;
    private Body[] bodies;

    private string WaveRightGestureName = "WaveGesture_Right";
    private string WaveLeftGestureName = "WaveGesture_Left";

    //Video Player
    public VideoPlayer[] video;

    //Gesture
    private List<GestureDetector> gestureDetectorList = null;
    
	// Use this for initialization
	void Start () {
        foreach (var vid in video)
        {
            vid.Stop();
        }
        this.kinectSensor = KinectSensor.GetDefault();
        if (this.kinectSensor!=null)
        {
            this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            //body data
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            //body frame to use
            this.bodies = new Body[this.bodyCount];

            // Inisialisasi gesture
            this.gestureDetectorList = new List<GestureDetector>();
            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
            {
                this.gestureDetectorList.Add(new GestureDetector(this.kinectSensor));
            }
            this.kinectSensor.Open();
        }
	}
	
	// Update is called once per frame
	void Update () {
        bool newBodyData = false;
        using (BodyFrame bodyFrame = this.bodyFrameReader.AcquireLatestFrame())
        {
            if (bodyFrame!=null)
            {
                bodyFrame.GetAndRefreshBodyData(this.bodies);
                newBodyData = true;
            }
        }
        if (newBodyData)
        {
            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
            {
                var body = this.bodies[bodyIndex];
                if (body!=null)
                {
                    var trackingId = body.TrackingId;

                    //if the current body TrackingId changed, update the corresponding gesture detector
                    if (trackingId!= this.gestureDetectorList[bodyIndex].TrackingId)
                    {
                        this.gestureDetectorList[bodyIndex].TrackingId = trackingId;
                        //unpause its detector to get VisualGestureBuilderFrame
                        this.gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);

                        //pause when body not tracked
                        this.gestureDetectorList[bodyIndex].OnGestureDetected += CreateOnGestureHandler(bodyIndex);
                    }
                }
            }
        }
	}

    private EventHandler<GestureEventArgs> CreateOnGestureHandler(int bodyIndex)
    {
        return (object sender, GestureEventArgs e) => OnGestureDetected(sender, e, bodyIndex);
    }

    private void OnGestureDetected(object sender, GestureEventArgs e, int bodyIndex)
    {
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;

        if (e.GestureID == WaveLeftGestureName)
        {
            if (e.DetectionConfidence > 0.65f)
            {
                video[0].Play();
                video[1].Stop();
            }
        }
        if (e.GestureID == WaveRightGestureName)
        {
            if (e.DetectionConfidence > 0.65f)
            {
                video[0].Stop();
                video[1].Play();
            }
        }
    }
}
