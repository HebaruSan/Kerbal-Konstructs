﻿using System;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs;


namespace KerbalKonstructs.Core
{
    public class CameraController
    {
        public FlightCamera cam;
        public bool active = false;

        private Transform oldTarget;

        private float x = 0;
        private float y = 0;
        private float zoom = 10;

        public void enable(GameObject target)
        {

            cam = FlightCamera.fetch;
            if (cam)
            {
                active = true;
                if (KerbalKonstructs.useLegacyCamera)
                {
                    
                    InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");

                    cam.DeactivateUpdate();
                    oldTarget = cam.transform.parent;
                    //cam.updateActive = false;
                    cam.transform.parent = target.transform;
                    cam.transform.position = target.transform.position;
                }
                else
                {
                    // new camera code
                    cam.SetTargetTransform(target.transform);
                }
            }
            else
            {
                Log.UserError("FlightCamera doesn't exist!");
            }
        }

        public void disable()
        {
            if (KerbalKonstructs.useLegacyCamera)
            {
                if (oldTarget != null)
                    cam.transform.parent = oldTarget;
                cam.ActivateUpdate();

                //for legacy control
                InputLockManager.RemoveControlLock("KKCamControls");
            }
            else
            {
                cam.SetTargetVessel(FlightGlobals.ActiveVessel);
            }
            active = false;
        }


        public void updateCamera()
        {
            bool needUpdate = false; 
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * cam.orbitSensitivity * 50.0f;
                y -= Input.GetAxis("Mouse Y") * cam.orbitSensitivity * 50.0f;
                needUpdate = true;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                zoom = Mathf.Clamp(zoom - Input.GetAxis("Mouse ScrollWheel") * 100.0f, cam.minDistance, cam.maxDistance);
                needUpdate = true;
            }
            if (needUpdate)
            {
                cam.transform.localRotation = Quaternion.Euler(y, x, 0);

                cam.transform.localPosition = Vector3.Slerp(cam.transform.localPosition, Quaternion.Euler(y, x, 0) * new Vector3(0.0f, 0.0f, -zoom), Time.deltaTime * cam.sharpness);
                //cam.transform.localPosition = cam.transform.localPosition + Quaternion.Euler(y, x, 0) * new Vector3(0, 0, -zoom) * Time.deltaTime * cam.sharpness;
            }
        }


        internal static void SetSpaceCenterCam(LaunchSite currentSite)
        {
            if (KerbalKonstructs.focusLastLaunchSite && (currentSite.body.name == "Kerbin"))
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    scCam.transform.parent = currentSite.lsGameObject.transform;
                    scCam.transform.position = currentSite.lsGameObject.transform.position;
                    scCam.initialPositionTransformName = currentSite.lsGameObject.transform.name;
                    //FieldInfo pqsField = scCam.GetType().GetField("pqs", BindingFlags.Instance | BindingFlags.NonPublic);
                    //pqsField.SetValue(scCam, currentSite.body.pqsController);
                    scCam.pqsName = currentSite.body.name;
                    scCam.ResetCamera();
                }

            }
            else
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    scCam.transform.parent = SpaceCenter.Instance.transform;
                    scCam.transform.position = SpaceCenter.Instance.transform.position;
                    scCam.initialPositionTransformName = "KSC/SpaceCenter/SpaceCenterCameraPosition";
                    scCam.pqsName = "Kerbin";
                    scCam.ResetCamera();
                }
            }

            if (currentSite.LaunchSiteName == "Runway" || currentSite.LaunchSiteName == "LaunchPad")
            {
                foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
                {
                    cam.altitudeInitial = 45f;
                    cam.ResetCamera();
                    
                }
            } else {

                    PQSCity sitePQS = currentSite.staticInstance.pqsCity;

                    foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
                    {
                        if (sitePQS.repositionToSphere || sitePQS.repositionToSphereSurface)
                        {

                            double nomHeight = currentSite.body.pqsController.GetSurfaceHeight((Vector3d)sitePQS.repositionRadial.normalized) - currentSite.body.Radius;
                            if (sitePQS.repositionToSphereSurface)
                            {
                                nomHeight += sitePQS.repositionRadiusOffset;
                            }
                            cam.altitudeInitial = 0f - (float)nomHeight;
                        }
                        else
                        {
                            cam.altitudeInitial = 0f - (float)sitePQS.repositionRadiusOffset;
                        }
                        cam.ResetCamera();
                        Log.Normal("fixed the Space Center camera.");
                    }
                }
        }


        internal static void SetSpaceCenterCam2(LaunchSite currentSite)
        {
            if (KerbalKonstructs.focusLastLaunchSite)
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    scCam.transform.parent = currentSite.lsGameObject.transform;
                    scCam.transform.position = currentSite.lsGameObject.transform.position;
                    scCam.initialPositionTransformName = currentSite.lsGameObject.transform.name;
                    //FieldInfo pqsField = scCam.GetType().GetField("pqs", BindingFlags.Instance | BindingFlags.NonPublic);
                    //pqsField.SetValue(scCam, currentSite.body.pqsController);
                    scCam.pqsName = currentSite.body.name;
                    scCam.ResetCamera();
                }

            }
            else
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    scCam.transform.parent = SpaceCenter.Instance.transform;
                    scCam.transform.position = SpaceCenter.Instance.transform.position;
                    scCam.initialPositionTransformName = "KSC/SpaceCenter/SpaceCenterCameraPosition";
                    scCam.pqsName = "Kerbin";
                    scCam.ResetCamera();
                }
            }

            //PQSCity sitePQS = currentSite.lsGameObject.GetComponentInParent<PQSCity>();

            //foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
            //{
            //    if (sitePQS.repositionToSphere || sitePQS.repositionToSphereSurface)
            //    {

            //        double nomHeight = currentSite.body.pqsController.GetSurfaceHeight((Vector3d)sitePQS.repositionRadial.normalized) - currentSite.body.Radius;
            //        if (sitePQS.repositionToSphereSurface)
            //        {
            //            nomHeight += sitePQS.repositionRadiusOffset;
            //        }
            //        cam.altitudeInitial = 0f - (float)nomHeight;
            //    }
            //    else
            //    {
            //        cam.altitudeInitial = 0f - (float)sitePQS.repositionRadiusOffset;
            //    }
            //    cam.ResetCamera();
            //    Log.Normal("fixed the Space Center camera.");
            //    SetNextMorningPoint(currentSite);
            //}

        }

        static void SetNextMorningPoint(LaunchSite launchSite)
        {

            double timeOfDawn = ((launchSite.staticInstance.RefLongitude + 180) / 360);

            KSP.UI.UIWarpToNextMorning.timeOfDawn = timeOfDawn + 0.06;
            Log.Normal("Fixed the \"warp to next morning\" button");

        }

    }
}
