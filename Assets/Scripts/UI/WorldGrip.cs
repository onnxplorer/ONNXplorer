using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

//RAINY //CHECK Should it also permit rotate in a y plane?
public class WorldGrip : MonoBehaviour
{
    private Transform sceneTransform;

    public Transform leftController;
    public Transform rightController;

    // Start is called before the first frame update
    void Start()
    {
        sceneTransform = this.transform;
    }

    //// Useful
    //var p = sceneTransform.InverseTransformPoint(rightController.transform.position).toDA(); // Vector3
    //var v = sceneTransform.InverseTransformDirection(rightController.TransformDirection(v));

    private int i = 0;

    void Update() {
        { // Debugging scene transform
            //sceneTransform.Translate(new Vector3((float)Math.Sin(i / 20.0)*0.1f, 0, (float)Math.Cos(i / 20.0)*0.1f));
            //if (i > 1000) {
            //    float s = (float)Math.Sin(i / 400.0) + 1.5f;
            //    sceneTransform.localScale = new Vector3(s, s, s);
            //}
            //i++;
        }

        //RAINY This may be important at some point
        /*
        { // Controller aim line
            var x = rightController.transform.position.copy();
            var v = rightController.TransformDirection(new double[] { 0, 0, 0.2 }.toV3());
            IMDraw.Line3D(x, new Vector3(x[0] + v[0], x[1] + v[1], x[2] + v[2]), Color.green);
        }
        */

        //THINK This *might* be nice.  May not mean much, here.
        /*
        { // Ruler
            var size = 0.2 / sceneTransform.lossyScale.x;
            var t = formatLength(size);
            IMDraw.TextMesh(leftController.TransformPoint(new Vector3(0, 0, 0.09f)), leftController.rotation * Quaternion.AngleAxis(90, new Vector3(1, 0, 0)), 0.1f, Color.white, t);
            var range = getLengthRange(size);

            // 0.2m bar
            IMDraw.Line3D(leftController.TransformPoint(new Vector3(-0.1f, 0, 0.1f)), leftController.TransformPoint(new Vector3(0.1f, 0, 0.1f)), Color.white);
            IMDraw.Line3D(leftController.TransformPoint(new Vector3(-0.1f, 0, 0.1f)), leftController.TransformPoint(new Vector3(-0.1f, 0, 0.11f)), Color.white);
            IMDraw.Line3D(leftController.TransformPoint(new Vector3(0.1f, 0, 0.1f)), leftController.TransformPoint(new Vector3(0.1f, 0, 0.11f)), Color.white);

            // Middle mark
            IMDraw.Line3D(leftController.TransformPoint(new Vector3(0f, 0, 0.105f)), leftController.TransformPoint(new Vector3(0f, 0, 0.11f)), Color.white);

            // Range ticks
            var dx = sceneTransform.lossyScale.x * range.a / 2;
            IMDraw.Line3D(leftController.TransformPoint(new Vector3((float)-dx, 0, 0.1f)), leftController.TransformPoint(new Vector3((float)-dx, 0, 0.105f)), Color.white);
            IMDraw.Line3D(leftController.TransformPoint(new Vector3((float)dx, 0, 0.1f)), leftController.TransformPoint(new Vector3((float)dx, 0, 0.105f)), Color.white);
        }
        */

        { // Grip
            var lg = SteamVR_Actions.default_GrabGrip.GetState(SteamVR_Input_Sources.LeftHand);
            var rg = SteamVR_Actions.default_GrabGrip.GetState(SteamVR_Input_Sources.RightHand);
            var oldGripped = leftGripped && rightGripped;
            var newGripped = lg && rg;
            if (!oldGripped) {
                if (newGripped) {
                    // Wasn't, now is, start grip
                    prevLeftGripPos = leftController.transform.position;
                    prevRightGripPos = rightController.transform.position;
                } else {
                    // Wasn't, isn't, do nothing
                }
            } else {
                if (!newGripped) {
                    // Was, isn't, stop gripping
                } else {
                    // Was, is, keep gripping and move
                    var lgp = leftController.transform.position;
                    var rgp = rightController.transform.position;
                    var wasSize = (prevLeftGripPos - prevRightGripPos).magnitude;
                    var isSize = (lgp - rgp).magnitude;

                    // Scale
                    var scale = (isSize / wasSize);

                    // Translate
                    var prevCenter = (prevLeftGripPos + prevRightGripPos) / 2;
                    var nowCenter = (lgp + rgp) / 2;
                    var translation = (nowCenter - prevCenter) + ((scale - 1) * (sceneTransform.position - nowCenter));
                    //var translation = ((scale - 1) * sceneTransform.position)+((nowCenter * (2-scale)) - prevCenter); // Doesn't help nanoscale jitter

                    // Rotate
                    var d0 = new double[] { prevRightGripPos.x - prevLeftGripPos.x, prevRightGripPos.z - prevLeftGripPos.z };
                    var d1 = new double[] { rgp.x - lgp.x, rgp.z - lgp.z };
                    // https://stackoverflow.com/a/16544330/513038
                    var dot = d0[0] * d1[0] + d0[1] * d1[1];  // dot product between [x1, y1] and [x2, y2]
                    var det = d0[0] * d1[1] - d0[1] * d1[0];  // determinant
                    var a = Math.Atan2(det, dot); // atan2(y, x) or atan2(sin, cos)
                    var rotation = -360 * a / (2 * Math.PI);

                    //TODO Fix
                    sceneTransform.localScale *= scale;
                    sceneTransform.localPosition += translation;
                    sceneTransform.RotateAround(nowCenter, new Vector3(0, 1, 0), (float)(rotation));

                    prevLeftGripPos = leftController.transform.position;
                    prevRightGripPos = rightController.transform.position;
                }
            }
            leftGripped = lg;
            rightGripped = rg;
        }
    }

    private Vector3 prevLeftGripPos;
    private Vector3 prevRightGripPos;
    private bool leftGripped = false;
    private bool rightGripped = false;
}
