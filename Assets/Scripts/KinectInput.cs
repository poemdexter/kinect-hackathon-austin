﻿using System.Collections.Generic;
using Windows.Kinect;
using UnityEngine;
using Kinect = Windows.Kinect;

public class KinectInput : MonoBehaviour
{
    private ulong player1ID, player2ID;
    private bool p1ReadyForID, p2ReadyForID;
    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();
    private KinectSensor kinectSensor;
    private BodyFrameReader bodyFrameReader;
    private Body[] bodyData = null;
    

    #region boneMap definition
    private Dictionary<Kinect.JointType, Kinect.JointType> boneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        {Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft},
        {Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft},
        {Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft},
        {Kinect.JointType.HipLeft, Kinect.JointType.SpineBase},
        {Kinect.JointType.FootRight, Kinect.JointType.AnkleRight},
        {Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight},
        {Kinect.JointType.KneeRight, Kinect.JointType.HipRight},
        {Kinect.JointType.HipRight, Kinect.JointType.SpineBase},
        {Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft},
        {Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft},
        {Kinect.JointType.HandLeft, Kinect.JointType.WristLeft},
        {Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft},
        {Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft},
        {Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder},
        {Kinect.JointType.HandTipRight, Kinect.JointType.HandRight},
        {Kinect.JointType.ThumbRight, Kinect.JointType.HandRight},
        {Kinect.JointType.HandRight, Kinect.JointType.WristRight},
        {Kinect.JointType.WristRight, Kinect.JointType.ElbowRight},
        {Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight},
        {Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder},
        {Kinect.JointType.SpineBase, Kinect.JointType.SpineMid},
        {Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder},
        {Kinect.JointType.SpineShoulder, Kinect.JointType.Neck},
        {Kinect.JointType.Neck, Kinect.JointType.Head},
    };
    #endregion

    private void Start()
    {
        // fire up the kinect sensor and open a reader
        kinectSensor = KinectSensor.GetDefault();
        if (kinectSensor != null)
        {
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            if (!kinectSensor.IsOpen)
            {
                kinectSensor.Open();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (bodyFrameReader != null)
        {
            bodyFrameReader.Dispose();
            bodyFrameReader = null;
        }
        if (kinectSensor != null && kinectSensor.IsOpen)
        {
            kinectSensor.Close();
        }
        kinectSensor = null;
    }

    private void Update()
    {
        // initiate the reader if we haven't already and grab bodies that we see
        if (bodyFrameReader != null)
        {
            var frame = bodyFrameReader.AcquireLatestFrame();
            if (frame != null)
            {
                if (bodyData == null)
                {
                    bodyData = new Body[kinectSensor.BodyFrameSource.BodyCount];
                }
            }

            // refresh the body data based on current frame
            frame.GetAndRefreshBodyData(bodyData);
            frame.Dispose();
            frame = null;
        }

        if (bodyData == null)
        {
            return;
        }

        // find which body is player 1 and which is player 2
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in bodyData)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(bodies[trackingId]);
                bodies.Remove(trackingId);

                // if removing a player, set them up to accept a body
                if (trackingId == player1ID)
                {
                    p1ReadyForID = true;
                }
                if (trackingId == player2ID)
                {
                    p2ReadyForID = true;
                }
            }
        }

        foreach (var body in bodyData)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!bodies.ContainsKey(body.TrackingId))
                {
                    bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                    // to lock bodies to players
                    if (body.TrackingId != player2ID && p1ReadyForID)
                    {
                        player1ID = body.TrackingId;
                        p1ReadyForID = false;
                    }

                    else if (body.TrackingId != player1ID && p2ReadyForID)
                    {
                        player2ID = body.TrackingId;
                        p2ReadyForID = false;
                    }
                }

                RefreshBodyObject(body, bodies[body.TrackingId]);
            }
        }
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        //if (body.TrackingId == player1ID)
        //{
        //    if (body.HandLeftConfidence == TrackingConfidence.High)
        //    {
        //        if (body.HandLeftState == HandState.Closed)
        //        {
        //            p1BlockingL = false;
        //        }
        //        else if (body.HandLeftState == HandState.Open)
        //        {
        //            p1BlockingL = false;
        //        }
        //        else if (body.HandLeftState == HandState.Lasso)
        //        {
        //            p1BlockingL = true;
        //        }
        //    }

        //    if (body.HandRightConfidence == TrackingConfidence.High)
        //    {
        //        if (body.HandRightState == HandState.Closed)
        //        {
        //            p1BlockingR = false;
        //        }
        //        else if (body.HandRightState == HandState.Open)
        //        {
        //            p1BlockingR = false;
        //        }
        //        else if (body.HandRightState == HandState.Lasso)
        //        {
        //            p1BlockingR = true;
        //        }
        //    }
        //    if (p1BlockingL)
        //    {
        //        player1LeftPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90f));
        //    }
        //    else
        //    {
        //        player1LeftPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        //    }

        //    if (p1BlockingR)
        //    {
        //        player1RightPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
        //    }
        //    else
        //    {
        //        player1RightPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        //    }

        //    Kinect.Joint leftHandJoint = body.Joints[Kinect.JointType.HandLeft];
        //    Vector3 leftJointPosition = GetVector3FromJoint(leftHandJoint);
        //    player1LeftPlatform.transform.position = new Vector3(leftJointPosition.x, leftJointPosition.y, 0);

        //    Kinect.Joint rightHandJoint = body.Joints[Kinect.JointType.HandRight];
        //    Vector3 rightJointPosition = GetVector3FromJoint(rightHandJoint);
        //    player1RightPlatform.transform.position = new Vector3(rightJointPosition.x, rightJointPosition.y, 0);

        //    //Kinect.JointType.SpineMid
        //    Kinect.Joint chestJoint = body.Joints[Kinect.JointType.SpineShoulder];
        //    Vector3 chestJointPosition = GetVector3FromJoint(chestJoint);
        //    p1Chest.transform.position = new Vector3(chestJointPosition.x, chestJointPosition.y, 0);

        //    //Kinect.JointType.Head
        //    Kinect.Joint headJoint = body.Joints[Kinect.JointType.Head];
        //    Vector3 headJointPosition = GetVector3FromJoint(headJoint);
        //    player1Head.transform.position = new Vector3(headJointPosition.x, headJointPosition.y, 0);

        //    //Kinect.JointType.SpineBase
        //    Kinect.Joint waistJoint = body.Joints[Kinect.JointType.SpineBase];
        //    Vector3 waistJointPosition = GetVector3FromJoint(waistJoint);
        //    player1WaistPlatform.transform.position = new Vector3(waistJointPosition.x, waistJointPosition.y, 0);
        //    p1Base.transform.position = new Vector3(waistJointPosition.x, waistJointPosition.y, 0);

        //    //Kinect.JointType.SpineMid
        //    Kinect.Joint midJoint = body.Joints[Kinect.JointType.SpineMid];
        //    Vector3 midJointPosition = GetVector3FromJoint(midJoint);
        //    p1Body.transform.position = new Vector3(midJointPosition.x, midJointPosition.y, 0);

        //    //Kinect.JointType.WristLeft
        //    Kinect.Joint p1WristLeftJoint = body.Joints[Kinect.JointType.WristLeft];
        //    Vector3 p1WristLeftJointPosition = GetVector3FromJoint(p1WristLeftJoint);
        //    p1armL3.transform.position = new Vector3(p1WristLeftJointPosition.x, p1WristLeftJointPosition.y, 0);

        //    Kinect.Joint p1WristRightJoint = body.Joints[Kinect.JointType.WristRight];
        //    Vector3 p1WristRightJointPosition = GetVector3FromJoint(p1WristRightJoint);
        //    p1armR3.transform.position = new Vector3(p1WristRightJointPosition.x, p1WristRightJointPosition.y, 0);

        //    //Kinect.JointType.ElbowLeft
        //    Kinect.Joint p1ElbowLeftJoint = body.Joints[Kinect.JointType.ElbowLeft];
        //    Vector3 p1ElbowLeftJointPosition = GetVector3FromJoint(p1ElbowLeftJoint);
        //    p1armL2.transform.position = new Vector3(p1ElbowLeftJointPosition.x, p1ElbowLeftJointPosition.y, 0);

        //    Kinect.Joint p1ElbowRightJoint = body.Joints[Kinect.JointType.ElbowRight];
        //    Vector3 p1ElbowRightJointPosition = GetVector3FromJoint(p1ElbowRightJoint);
        //    p1armR2.transform.position = new Vector3(p1ElbowRightJointPosition.x, p1ElbowRightJointPosition.y, 0);

        //    //Kinect.JointType.ShoulderLeft
        //    Kinect.Joint p1ShoulderLeftJoint = body.Joints[Kinect.JointType.ShoulderLeft];
        //    Vector3 p1ShoulderLeftJointPosition = GetVector3FromJoint(p1ShoulderLeftJoint);
        //    p1armL1.transform.position = new Vector3(p1ShoulderLeftJointPosition.x, p1ShoulderLeftJointPosition.y, 0);

        //    Kinect.Joint p1ShoulderRightJoint = body.Joints[Kinect.JointType.ShoulderRight];
        //    Vector3 p1ShoulderRightJointPosition = GetVector3FromJoint(p1ShoulderRightJoint);
        //    p1armR1.transform.position = new Vector3(p1ShoulderRightJointPosition.x, p1ShoulderRightJointPosition.y, 0);

        //}
        //else if (body.TrackingId == player2ID)
        //{
        //    if (body.HandLeftConfidence == TrackingConfidence.High)
        //    {
        //        if (body.HandLeftState == HandState.Closed)
        //        {
        //            p2BlockingL = false;
        //        }
        //        else if (body.HandLeftState == HandState.Open)
        //        {
        //            p2BlockingL = false;
        //        }
        //        else if (body.HandLeftState == HandState.Lasso)
        //        {
        //            p2BlockingL = true;
        //        }
        //    }

        //    if (body.HandRightConfidence == TrackingConfidence.High)
        //    {
        //        if (body.HandRightState == HandState.Closed)
        //        {
        //            p2BlockingR = false;
        //        }
        //        else if (body.HandRightState == HandState.Open)
        //        {
        //            p2BlockingR = false;
        //        }
        //        else if (body.HandRightState == HandState.Lasso)
        //        {
        //            p2BlockingR = true;
        //        }
        //    }

        //    if (p2BlockingL)
        //    {
        //        player2LeftPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90f));
        //    }
        //    else
        //    {
        //        player2LeftPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        //    }

        //    if (p2BlockingR)
        //    {
        //        player2RightPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
        //    }
        //    else
        //    {
        //        player2RightPlatform.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        //    }

        //    Kinect.Joint leftHandJoint = body.Joints[Kinect.JointType.HandLeft];
        //    Vector3 leftJointPosition = GetVector3FromJoint(leftHandJoint);
        //    player2LeftPlatform.transform.position = new Vector3(leftJointPosition.x, leftJointPosition.y, 0);

        //    Kinect.Joint rightHandJoint = body.Joints[Kinect.JointType.HandRight];
        //    Vector3 rightJointPosition = GetVector3FromJoint(rightHandJoint);
        //    player2RightPlatform.transform.position = new Vector3(rightJointPosition.x, rightJointPosition.y, 0);

        //    //Kinect.JointType.SpineShoulder
        //    Kinect.Joint chestJoint = body.Joints[Kinect.JointType.SpineShoulder];
        //    Vector3 chestJointPosition = GetVector3FromJoint(chestJoint);
        //    p2Chest.transform.position = new Vector3(chestJointPosition.x, chestJointPosition.y, 0);

        //    //Kinect.JointType.Head
        //    Kinect.Joint headJoint = body.Joints[Kinect.JointType.Head];
        //    Vector3 headJointPosition = GetVector3FromJoint(headJoint);
        //    player2Head.transform.position = new Vector3(headJointPosition.x, headJointPosition.y, 0);

        //    //Kinect.JointType.SpineBase
        //    Kinect.Joint waistJoint = body.Joints[Kinect.JointType.SpineBase];
        //    Vector3 waistJointPosition = GetVector3FromJoint(waistJoint);
        //    player2WaistPlatform.transform.position = new Vector3(waistJointPosition.x, waistJointPosition.y, 0);
        //    p2Base.transform.position = new Vector3(waistJointPosition.x, waistJointPosition.y, 0);

        //    //Kinect.JointType.SpineMid
        //    Kinect.Joint midJoint = body.Joints[Kinect.JointType.SpineMid];
        //    Vector3 midJointPosition = GetVector3FromJoint(midJoint);
        //    p2Body.transform.position = new Vector3(midJointPosition.x, midJointPosition.y, 0);

        //    //Kinect.JointType.WristLeft
        //    Kinect.Joint p2WristLeftJoint = body.Joints[Kinect.JointType.WristLeft];
        //    Vector3 p2WristLeftJointPosition = GetVector3FromJoint(p2WristLeftJoint);
        //    p2armL3.transform.position = new Vector3(p2WristLeftJointPosition.x, p2WristLeftJointPosition.y, 0);

        //    Kinect.Joint p2WristRightJoint = body.Joints[Kinect.JointType.WristRight];
        //    Vector3 p2WristRightJointPosition = GetVector3FromJoint(p2WristRightJoint);
        //    p2armR3.transform.position = new Vector3(p2WristRightJointPosition.x, p2WristRightJointPosition.y, 0);

        //    //Kinect.JointType.ElbowLeft
        //    Kinect.Joint p2ElbowLeftJoint = body.Joints[Kinect.JointType.ElbowLeft];
        //    Vector3 p2ElbowLeftJointPosition = GetVector3FromJoint(p2ElbowLeftJoint);
        //    p2armL2.transform.position = new Vector3(p2ElbowLeftJointPosition.x, p2ElbowLeftJointPosition.y, 0);

        //    Kinect.Joint p2ElbowRightJoint = body.Joints[Kinect.JointType.ElbowRight];
        //    Vector3 p2ElbowRightJointPosition = GetVector3FromJoint(p2ElbowRightJoint);
        //    p2armR2.transform.position = new Vector3(p2ElbowRightJointPosition.x, p2ElbowRightJointPosition.y, 0);

        //    //Kinect.JointType.ShoulderLeft
        //    Kinect.Joint p2ShoulderLeftJoint = body.Joints[Kinect.JointType.ShoulderLeft];
        //    Vector3 p2ShoulderLeftJointPosition = GetVector3FromJoint(p2ShoulderLeftJoint);
        //    p2armL1.transform.position = new Vector3(p2ShoulderLeftJointPosition.x, p2ShoulderLeftJointPosition.y, 0);

        //    Kinect.Joint p2ShoulderRightJoint = body.Joints[Kinect.JointType.ShoulderRight];
        //    Vector3 p2ShoulderRightJointPosition = GetVector3FromJoint(p2ShoulderRightJoint);
        //    p2armR1.transform.position = new Vector3(p2ShoulderRightJointPosition.x, p2ShoulderRightJointPosition.y, 0);
        //}

        //if ((p1BlockingL || p1BlockingR) && (p2BlockingL || p2BlockingR))
        //{
        //    game.Ready();
        //}
    }

    private GameObject CreateBodyObject(ulong id)
    {
        return new GameObject("Body:" + id);
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}