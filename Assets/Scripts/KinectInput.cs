﻿using System.Collections.Generic;
using Windows.Kinect;
using UnityEngine;
using Kinect = Windows.Kinect;

public class KinectInput : MonoBehaviour
{
    private ulong player1ID, player2ID;
    private bool p1ReadyForID = true, p2ReadyForID = true;
    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();
    private KinectSensor kinectSensor;
    private BodyFrameReader bodyFrameReader;
    private Body[] bodyData = null;

    public GameManager gameManager;
    public GameObject player1Paddle, player2Paddle;

    private GameObject p1Select, p2Select;
    public GameObject selectObject;

    public float maxDistanceForPlayer = 6f;

    public float burgerMOE = 0.50f;
    public float texanMOE = 0.50f;
    public float handshakeMOE = 0.15f;

    private Kinect.Joint leftHandJointP1, rightHandJointP1;
    private Vector3 leftJointPositionP1, rightJointPositionP1;
    private Kinect.Joint leftHandJointP2, rightHandJointP2;
    private Vector3 leftJointPositionP2, rightJointPositionP2;

    private Vector3 selectHeadPosition1, selectHeadPosition2;

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

    // 1 unit is about 3 feet.
    private bool IsBodyWithinRange(Body body)
    {
        return body.Joints[Kinect.JointType.SpineMid].Position.Z < (maxDistanceForPlayer / 3f);
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

                // refresh the body data based on current frame
                frame.GetAndRefreshBodyData(bodyData);
                frame.Dispose();
                frame = null;
            }
        }

        if (bodyData == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in bodyData)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked && IsBodyWithinRange(body))
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

            if (body.IsTracked && IsBodyWithinRange(body))
            {
                if (!bodies.ContainsKey(body.TrackingId))
                {
                    bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                    // to lock bodies to players
                    if (body.TrackingId != player2ID && p1ReadyForID)
                    {
                        Debug.Log("tracking player 1");
                        player1ID = body.TrackingId;
                        p1ReadyForID = false;
                    }

                    else if (body.TrackingId != player1ID && p2ReadyForID)
                    {
                        Debug.Log("tracking player 2");
                        player2ID = body.TrackingId;
                        p2ReadyForID = false;
                    }
                }

                if (gameManager.GetCurrentState() == GameState.PlayerSelect)
                {
                    DetectPlayerSelect(body, bodies[body.TrackingId]);
                }
                else if (gameManager.GetCurrentState() == GameState.Handshake)
                {
                    DetectHandshake(body, bodies[body.TrackingId]);
                }
                else if (gameManager.GetCurrentState() == GameState.Instructions)
                {
                    // wait for instructions to be over
                }
                else if (gameManager.GetCurrentState() == GameState.GamePlay)
                {
                    switch (gameManager.GetCurrentRule())
                    {
                        case Rules.Pong:
                            DetectNormalPong(body, bodies[body.TrackingId]);
                            break;
                        case Rules.Burger:
                            DetectHamburger(body, bodies[body.TrackingId]);
                            break;
                        case Rules.Texan:
                            DetectTexan(body, bodies[body.TrackingId]);
                            break;
                    }
                }
            }
        }
    }

    private bool NewPositionWithinBounds(Vector3 position)
    {
        return position.y <= 2.75f && position.y >= -2.75f;
    }

    private void DetectPlayerSelect(Kinect.Body body, GameObject bodyObject)
    {
        if (body.TrackingId == player1ID)
        {
            Kinect.Joint headJoint = body.Joints[Kinect.JointType.Head];
            selectHeadPosition1 = GetVector3FromJoint(headJoint);
        }

        if (body.TrackingId == player2ID)
        {
            Kinect.Joint headJoint = body.Joints[Kinect.JointType.Head];
            selectHeadPosition2 = GetVector3FromJoint(headJoint);
        }

        if (selectHeadPosition1 != Vector3.zero && selectHeadPosition2 != Vector3.zero)
        {
            if (selectHeadPosition1.x > selectHeadPosition2.x)
            {
                // players inversed, need to swap
                ulong temp = player1ID;
                player1ID = player2ID;
                player2ID = temp;
            }
            gameManager.PlayersSelected();
        }
    }

    private void DetectHamburger(Kinect.Body body, GameObject bodyObject)
    {
        if (body.TrackingId == player1ID)
        {
            Kinect.Joint leftHandJoint = body.Joints[Kinect.JointType.HandLeft];
            Kinect.Joint rightHandJoint = body.Joints[Kinect.JointType.HandRight];
            Vector3 leftJointPosition = GetVector3FromJoint(leftHandJoint);
            Vector3 rightJointPosition = GetVector3FromJoint(rightHandJoint);
            if (leftJointPosition.y - rightJointPosition.y <= handshakeMOE &&
                leftJointPosition.y - rightJointPosition.y >= -handshakeMOE)
            {
                if (NewPositionWithinBounds(leftJointPosition))
                    player1Paddle.transform.position = new Vector3(player1Paddle.transform.position.x,
                        leftJointPosition.y, 0);
            }
        }

        if (body.TrackingId == player2ID)
        {
            Kinect.Joint leftHandJoint = body.Joints[Kinect.JointType.HandLeft];
            Kinect.Joint rightHandJoint = body.Joints[Kinect.JointType.HandRight];
            Vector3 leftJointPosition = GetVector3FromJoint(leftHandJoint);
            Vector3 rightJointPosition = GetVector3FromJoint(rightHandJoint);
            if (leftJointPosition.y - rightJointPosition.y <= burgerMOE &&
                leftJointPosition.y - rightJointPosition.y >= -burgerMOE)
            {
                if (NewPositionWithinBounds(leftJointPosition))
                    player2Paddle.transform.position = new Vector3(player2Paddle.transform.position.x,
                        leftJointPosition.y, 0);
            }
        }
    }

    private void DetectTexan(Kinect.Body body, GameObject bodyObject)
    {
        if (body.TrackingId == player1ID)
        {
            leftHandJointP1 = body.Joints[Kinect.JointType.HandLeft];
            rightHandJointP1 = body.Joints[Kinect.JointType.HandRight];
            leftJointPositionP1 = GetVector3FromJoint(leftHandJointP1);
            rightJointPositionP1 = GetVector3FromJoint(rightHandJointP1);
            float handDiff = Mathf.Abs(rightJointPositionP1.y - leftJointPositionP1.y);
            if (rightJointPositionP1.y > leftJointPositionP1.y)
            {
                Vector3 newPos = new Vector3(player1Paddle.transform.position.x,
                    player1Paddle.transform.position.y + handDiff*.05f, 0);
                if (NewPositionWithinBounds(newPos))
                {
                    player1Paddle.transform.position = newPos;
                }
            }

            if (rightJointPositionP1.y < leftJointPositionP1.y)
            {
                Vector3 newPos = new Vector3(player1Paddle.transform.position.x,
                    player1Paddle.transform.position.y - handDiff*.05f, 0);
                if (NewPositionWithinBounds(newPos))
                {
                    player1Paddle.transform.position = newPos;
                }
            }
        }

        if (body.TrackingId == player2ID)
        {
            Kinect.Joint leftHandJoint = body.Joints[Kinect.JointType.HandLeft];
            Kinect.Joint rightHandJoint = body.Joints[Kinect.JointType.HandRight];
            Vector3 leftJointPosition = GetVector3FromJoint(leftHandJoint);
            Vector3 rightJointPosition = GetVector3FromJoint(rightHandJoint);
            float handDiff = Mathf.Abs(rightJointPosition.y - leftJointPosition.y);
            if (rightJointPosition.y > leftJointPosition.y)
            {
                Vector3 newPos = new Vector3(player2Paddle.transform.position.x,
                    player2Paddle.transform.position.y + handDiff*.1f, 0);
                if (NewPositionWithinBounds(newPos))
                {
                    player2Paddle.transform.position = newPos;
                }
            }

            if (rightJointPosition.y < leftJointPosition.y)
            {
                Vector3 newPos = new Vector3(player2Paddle.transform.position.x,
                    player2Paddle.transform.position.y - handDiff*.1f, 0);
                if (NewPositionWithinBounds(newPos))
                {
                    player2Paddle.transform.position = newPos;
                }
            }
        }
    }

    public void DetectNormalPong(Kinect.Body body, GameObject bodyObject)
    {
        if (body.TrackingId == player1ID)
        {
            Kinect.Joint leftHandJoint = body.Joints[Kinect.JointType.HandLeft];
            Vector3 leftJointPosition = GetVector3FromJoint(leftHandJoint);

            if (NewPositionWithinBounds(leftJointPosition))
            {
                player1Paddle.transform.position = new Vector3(player1Paddle.transform.position.x,
                    leftJointPosition.y, 0);
            }
        }

        if (body.TrackingId == player2ID)
        {
            Kinect.Joint rightHandJoint = body.Joints[Kinect.JointType.HandRight];
            Vector3 rightJointPosition = GetVector3FromJoint(rightHandJoint);

            if (NewPositionWithinBounds(rightJointPosition))
            {
                player2Paddle.transform.position = new Vector3(player2Paddle.transform.position.x,
                    rightJointPosition.y, 0);
            }
        }
    }

    public void DetectHandshake(Kinect.Body body, GameObject bodyObject)
    {
        if (body.TrackingId == player1ID)
        {
            leftHandJointP1 = body.Joints[Kinect.JointType.HandLeft];
            rightHandJointP1 = body.Joints[Kinect.JointType.HandRight];
            leftJointPositionP1 = GetVector3FromJoint(leftHandJointP1);
            rightJointPositionP1 = GetVector3FromJoint(rightHandJointP1);
        }

        if (body.TrackingId == player2ID)
        {
            leftHandJointP2 = body.Joints[Kinect.JointType.HandLeft];
            rightHandJointP2 = body.Joints[Kinect.JointType.HandRight];
            leftJointPositionP2 = GetVector3FromJoint(leftHandJointP2);
            rightJointPositionP2 = GetVector3FromJoint(rightHandJointP2);
        }

        if (Mathf.Abs(rightJointPositionP2.x - rightJointPositionP1.x) <= handshakeMOE ||
            Mathf.Abs(leftJointPositionP2.x - leftJointPositionP1.x) <= handshakeMOE)
        {
            gameManager.HandsShook();
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        return new GameObject("Body:" + id);
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X*5, joint.Position.Y*5, joint.Position.Z*5);
    }
}