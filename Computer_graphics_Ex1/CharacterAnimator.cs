using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;


public class CharacterAnimator : MonoBehaviour
{
    public TextAsset BVHFile; // The BVH file that defines the animation and skeleton
    public bool animate; // Indicates whether or not the animation should be running

    private BVHData data; // BVH data of the BVHFile will be loaded here
    private int currFrame = 0; // Current frame of the animation
    private float timeCount = 0;
    private float DIAMETER = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        BVHParser parser = new BVHParser();
        data = parser.Parse(BVHFile);
        CreateJoint(data.rootJoint, Vector3.zero);
    }


    // Returns a Matrix4x4 representing a rotation aligning the up direction of an object with the given v
    Matrix4x4 RotateTowardsVector(Vector3 v)
    {
        Vector3 normV = v * (1 / v.magnitude);

        float zAngle = 90 - Mathf.Atan2(Mathf.Sqrt(normV.y * normV.y + normV.z * normV.z), normV.x) * Mathf.Rad2Deg;
        float xAngle = -1 * (90 - Mathf.Atan2(normV.y, normV.z) * Mathf.Rad2Deg);

        Matrix4x4 rotations = MatrixUtils.RotateX(xAngle).inverse * MatrixUtils.RotateZ(zAngle).inverse;

        return rotations;
    }

    // Creates a Cylinder GameObject between two given points in 3D space
    GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        Vector3 positionVector = (p1 + p2) / 2;
        Matrix4x4 translateMatrix = MatrixUtils.Translate(positionVector);
        Vector3 rotateVector = p2 - p1;
        Matrix4x4 rotationMatrix = (rotateVector == Vector3.zero) ? Matrix4x4.identity : RotateTowardsVector(rotateVector);


        float p1p2Distance = (p1 - p2).magnitude;
        Vector3 scaleVector = new Vector3(diameter, p1p2Distance / 2, diameter);
        Matrix4x4 scalingMatrix = MatrixUtils.Scale(scaleVector);

        Matrix4x4 transformMatrix = translateMatrix * rotationMatrix * scalingMatrix;

        MatrixUtils.ApplyTransform(cylinder, transformMatrix);

        return cylinder;
    }

    // Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for it's child joints
    GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
    {
        joint.gameObject = new GameObject(joint.name);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = joint.gameObject.transform;

        int scaling = (joint.name == "Head") ? 8 : 2;
        Vector3 newScaleVector = new Vector3(scaling, scaling, scaling);

        MatrixUtils.ApplyTransform(sphere, MatrixUtils.Scale(newScaleVector));

        Vector3 newPosVector = parentPosition + joint.offset;
        MatrixUtils.ApplyTransform(joint.gameObject, MatrixUtils.Translate(newPosVector));

        foreach (BVHJoint child in joint.children)
        {
            CreateJoint(child, joint.gameObject.transform.position);
            GameObject cylinder = CreateCylinderBetweenPoints(joint.gameObject.transform.position, child.gameObject.transform.position, DIAMETER);
            cylinder.transform.parent = joint.gameObject.transform;

        }

        return joint.gameObject;
    }

    // Transforms BVHJoint according to the keyframe channel data, and recursively transforms its children
    private void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform, float[] keyframe)
    {
        Matrix4x4 T = MatrixUtils.Translate(joint.offset);
        if (joint == data.rootJoint)
        {
            Vector3 rootTrans = new Vector3(keyframe[joint.positionChannels.x], keyframe[joint.positionChannels.y], keyframe[joint.positionChannels.z]);
            T = MatrixUtils.Translate(rootTrans);

        }

        Matrix4x4 Rx = MatrixUtils.RotateX(keyframe[joint.rotationChannels.x]);
        Matrix4x4 Ry = MatrixUtils.RotateY(keyframe[joint.rotationChannels.y]);
        Matrix4x4 Rz = MatrixUtils.RotateZ(keyframe[joint.rotationChannels.z]);

        Matrix4x4 first = (joint.rotationOrder.x == 0) ? Rx : (joint.rotationOrder.y == 0) ? Ry : Rz;
        Matrix4x4 sec = (joint.rotationOrder.x == 1) ? Rx : (joint.rotationOrder.y == 1) ? Ry : Rz;
        Matrix4x4 third = (joint.rotationOrder.x == 2) ? Rx : (joint.rotationOrder.y == 2) ? Ry : Rz;

        Matrix4x4 M = T * first * sec * third;

        Matrix4x4 Mtag = parentTransform * M;

        MatrixUtils.ApplyTransform(joint.gameObject, Mtag);

        foreach (BVHJoint child in joint.children)
        {
            TransformJoint(child, Mtag, keyframe);
        }
    }

    // Update is called once per frame 
    void Update()
    {
        if (animate)
        {
            timeCount += Time.deltaTime;

            if (timeCount >= (currFrame + 1) * data.frameLength)
            {
                currFrame = (int)Math.Ceiling(timeCount / data.frameLength);
                
                if (currFrame > data.numFrames)
                {
                    currFrame = 0;
                    timeCount = timeCount - data.numFrames*data.frameLength;
                }

                TransformJoint(data.rootJoint, Matrix4x4.identity, data.keyframes[currFrame]);
            }
        }
    }

}

