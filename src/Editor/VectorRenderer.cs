﻿//
//  This is Class is from the kOS addon. The License is GPL Version 3
//   https://github.com/KSP-KOS/KOS/
//
//
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace KerbalKonstructs.UI
{
    public class VectorRenderer
    {
        public Vector3d Vector { get; set; }
        public Color Color { get; set; }
        public Vector3d Start { get; set; }
        public double Scale { get; set; }
        public double Width { get; set; }

        private LineRenderer line;
        private LineRenderer hat;
        private bool enable;
        private GameObject lineObj;
        private GameObject hatObj;
        private GameObject labelObj;
        private GUIText label;
        private string labelStr = "";
        private Vector3 labelLocation;

        // These could probably be moved somewhere where they are updated
        // more globally just once per Update() rather than once per
        // VecterRenderer object per Update().  In future if we start
        // allowing more types of drawing primitives like this, then
        // it might be worth the work to move these, and their associated
        // updater methods, to a new class with one global instance for the whole
        // mod.  Until then it's not that much of an extra cost:
        private Vector3d shipCenterCoords;

        private Vector3d camPos;         // camera coordinates.
        private Vector3 camLookVec;     // vector from camera to ship position.
        private Vector3 prevCamLookVec;
        private Quaternion camRot;
        private Quaternion prevCamRot;
        private const int MAP_LAYER = 10; // found through trial-and-error
        private const int FLIGHT_LAYER = 15; // Supposedly the layer for UI effects in flight camera.

        public VectorRenderer()
        {
            Vector = new Vector3d(0, 0, 0);
            Color = new Color(1, 1, 1);
            Start = new Vector3d(0, 0, 0);
            Scale = 1.0;
            Width = 0;
        }


        /// <summary>
        /// Move the origin point of the vector drawings to move with the
        /// current ship, whichever ship that happens to be at the moment,
        /// and move to wherever that ship is within its local XYZ world (which
        /// isn't always at (0,0,0), as it turns out.):
        /// </summary>
        public void draw()
        {
            if (line == null || hat == null || !enable)
                return;

            GetCamData();
            GetShipCenterCoords();
            PutAtShipRelativeCoords();

            SetLayer(FLIGHT_LAYER);

            var magnitudeChange = prevCamLookVec.magnitude != camLookVec.magnitude;
            if (magnitudeChange)
            {
                RenderPointCoords();
                LabelPlacement();
            }
            else if (prevCamRot != camRot)
            {
                LabelPlacement();
            }
        }

        /// <summary>
        /// Update _shipCenterCoords, abstracting the different ways to do
        /// it depending on view mode:
        /// </summary>
        private void GetShipCenterCoords()
        {
            shipCenterCoords = FlightGlobals.ActiveVessel.CoMD;
        }

        /// <summary>
        /// Update camera data, abstracting the different ways KSP does it
        /// depending on view mode:
        /// </summary>
        private void GetCamData()
        {
            prevCamLookVec = camLookVec;
            prevCamRot = camRot;

            var cam = FlightCamera.fetch.mainCamera;
            camPos = cam.transform.localPosition;

            // the Distance coming from MapView.MapCamera.Distance
            // doesn't seem to work - calculating it myself below:
            // _camdist = pc.Distance();
            // camRot = cam.GetCameraTransform().rotation;
            camRot = cam.transform.rotation;

            camLookVec = camPos - shipCenterCoords;
        }

        /// <summary>
        /// Position the origins of the objects that make up the arrow
        /// such that they anchor relative to current ship position.
        /// </summary>
        private void PutAtShipRelativeCoords()
        {
            line.transform.localPosition = shipCenterCoords;
            hat.transform.localPosition = shipCenterCoords;
        }

        public bool GetShow()
        {
            return enable;
        }

        public void SetShow(bool newShowVal)
        {
            if (newShowVal)
            {
                if (line == null || hat == null)
                {
                    lineObj = new GameObject("vecdrawLine");
                    hatObj = new GameObject("vecdrawHat");
                    labelObj = new GameObject("vecdrawLabel", typeof(GUIText));

                    line = lineObj.AddComponent<LineRenderer>();
                    hat = hatObj.AddComponent<LineRenderer>();
                    //TODO: 1.1 TODO
                    label = labelObj.GetComponent<GUIText>();

                    line.useWorldSpace = false;
                    hat.useWorldSpace = false;

                    GetShipCenterCoords();

                    line.material = new Material(Shader.Find("Particles/Additive"));
                    hat.material = new Material(Shader.Find("Particles/Additive"));

                    // This is how font loading would work if other fonts were available in KSP:
                    // Font lblFont = (Font)Resources.Load( "Arial", typeof(Font) );
                    // SafeHouse.Logger.Log( "lblFont is " + (lblFont == null ? "null" : "not null") );
                    // _label.font = lblFont;

                    label.text = labelStr;
                    label.anchor = TextAnchor.MiddleCenter;

                    PutAtShipRelativeCoords();
                    RenderValues();
                }
                line.enabled = true;
                hat.enabled = true;
                label.enabled = true;
            }
            else
            {
                if (label != null)
                {
                    label.enabled = false;
                    label = null;
                }
                if (hat != null)
                {
                    hat.enabled = false;
                    hat = null;
                }
                if (line != null)
                {
                    line.enabled = false;
                    line = null;
                }
                labelObj = null;
                hatObj = null;
                lineObj = null;
            }


            enable = newShowVal;
        }

        public void SetLayer(int newVal)
        {
            if (lineObj != null) lineObj.layer = newVal;
            if (hatObj != null) hatObj.layer = newVal;
            if (labelObj != null) labelObj.layer = newVal;
        }

        public void SetLabel(string newVal)
        {
            labelStr = newVal;
            if (label != null) label.text = labelStr;
            RenderPointCoords();
        }

        public void RenderValues()
        {
            RenderPointCoords();
            RenderColor();
            GetCamData();
            LabelPlacement();
        }

        /// <summary>
        /// Assign the arrow and label's positions in space.  Call
        /// whenever :VEC, :START, or :SCALE change, or when the
        /// game switches between flight view and map view, as they
        /// don't use the same size scale.
        /// </summary>
        public void RenderPointCoords()
        {
            if (line != null && hat != null)
            {
                double mapLengthMult = 1.0; // for scaling when on map view.
                double mapWidthMult = 1.0; // for scaling when on map view.
                float useWidth;

                Vector3d point1 = mapLengthMult * Start;
                Vector3d point2 = mapLengthMult * (Start + (Scale * 0.95 * Vector));
                Vector3d point3 = mapLengthMult * (Start + (Scale * Vector));

                label.fontSize = (int)(12.0 * (Width / 0.2) * Scale);

                useWidth = (float)(Width * Scale * mapWidthMult);

                // Position the arrow line:
                line.SetVertexCount(2);
                line.SetWidth(useWidth, useWidth);
                line.SetPosition(0, point1);
                line.SetPosition(1, point2);

                // Position the arrow hat:
                hat.SetVertexCount(2);
                hat.SetWidth(useWidth * 3.5f, 0.0F);
                hat.SetPosition(0, point2);
                hat.SetPosition(1, point3);

                // Put the label at the midpoint of the arrow:
                labelLocation = (point1 + point3) / 2;

                PutAtShipRelativeCoords();
            }
        }

        /// <summary>
        /// Calculates colors and applies transparency fade effect.
        /// Only needs to be called when the color changes.
        /// </summary>
        public void RenderColor()
        {
            Color c1 = Color;
            Color c2 = Color;
            c1.a = c1.a * (float)0.25;
            Color lCol = UnityEngine.Color.Lerp(c2, UnityEngine.Color.white, 0.7f); // "whiten" the label color a lot.

            if (line != null && hat != null)
            {
                line.SetColors(c1, c2); // The line has the fade effect
                hat.SetColors(c2, c2);  // The hat does not have the fade effect.
                label.color = lCol;     // The label does not have the fade effect.
            }
        }

        private Vector3 GetViewportPosFor(Vector3 v)
        {
            var cam = FlightCamera.fetch.mainCamera;
            return cam.WorldToViewportPoint(v);
        }

        /// <summary>
        /// Place the 2D label at the correct projected spot on
        /// the screen from its location in 3D space:
        /// </summary>
        private void LabelPlacement()
        {
            Vector3 screenPos = GetViewportPosFor(shipCenterCoords + labelLocation);

            // If the projected location is on-screen:
            if (screenPos.z > 0
                 && screenPos.x >= 0 && screenPos.x <= 1
                 && screenPos.y >= 0 && screenPos.y <= 1)
            {
                label.enabled = true;
                label.transform.position = screenPos;
            }
            else
            {
                label.enabled = false;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} VectorRenderer", base.ToString());
        }

    }
}
