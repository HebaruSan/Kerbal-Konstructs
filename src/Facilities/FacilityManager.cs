﻿using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    class FacilityManager : KKWindow
    {
        private static FacilityManager _instance = null;
        internal static FacilityManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FacilityManager();
                }
                return _instance;
            }
        }

        public static Rect facilityManagerRect = new Rect(150, 75, 320, 670);

        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);

        public static LaunchSite selectedSite = null;
        public static StaticInstance selectedFacility = null;

        private float fLqFMax = 0;
        private float fOxFMax = 0;
        private float fMoFMax = 0;

        private double fAlt = 0f;

        public Boolean isOpen2 = false;
        public Boolean bChangeTargetType = false;

        public Boolean bHalfwindow = false;
        public Boolean bHalvedWindow = false;

        public Boolean bTransferOreToF = false;
        public Boolean bTransferOreToC = false;

        private string sFacilityName = "Unknown";
        private string sFacilityType = "Unknown";

        private Vector3 objectPos = new Vector3(0, 0, 0);

        private double disObjectLat = 0;
        private double disObjectLon = 0;

        private GUIStyle Yellowtext;
        private GUIStyle KKWindow;
        private GUIStyle DeadButton;
        private GUIStyle DeadButtonRed;
        private GUIStyle BoxNoBorder;
        private GUIStyle LabelInfo;
        private GUIStyle ButtonSmallText;


        public override void Close()
        {
            if (KerbalKonstructs.instance.selectedObject != null)
                KerbalKonstructs.instance.deselectObject(true, true);
            base.Close();
        }

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                if (KerbalKonstructs.instance.selectedObject != null)
                    KerbalKonstructs.instance.deselectObject(true, true);
            }

            KKWindow = new GUIStyle(GUI.skin.window);
            KKWindow.padding = new RectOffset(3, 3, 5, 5);

            if (bHalfwindow)
            {
                if (!bHalvedWindow)
                {
                    facilityManagerRect = new Rect(facilityManagerRect.xMin, facilityManagerRect.yMin, facilityManagerRect.width, facilityManagerRect.height - 200);
                    bHalvedWindow = true;
                }
            }

            if (!bHalfwindow)
            {
                if (bHalvedWindow)
                {
                    facilityManagerRect = new Rect(facilityManagerRect.xMin, facilityManagerRect.yMin, facilityManagerRect.width, facilityManagerRect.height + 200);
                    bHalvedWindow = false;
                }
            }

            facilityManagerRect = GUI.Window(0xB01B2B5, facilityManagerRect, drawFacilityManagerWindow, "", KKWindow);
        }

        void drawFacilityManagerWindow(int windowID)
        {
            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.white;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.white;
            DeadButton.focused.textColor = Color.white;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Bold;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            Yellowtext = new GUIStyle(GUI.skin.box);
            Yellowtext.normal.textColor = Color.yellow;
            Yellowtext.normal.background = null;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            ButtonSmallText = new GUIStyle(GUI.skin.button);
            ButtonSmallText.fontSize = 12;
            ButtonSmallText.fontStyle = FontStyle.Normal;

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Facility Manager", DeadButton, GUILayout.Height(16));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
                {
                    selectedFacility = null;
                    this.Close();
                    return;

                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            if (selectedFacility != null)
            {
                sFacilityType = (string)selectedFacility.FacilityType;

                if (sFacilityType == "GroundStation")
                {
                    sFacilityName = "Ground Station";
                    bHalfwindow = true;
                }
                else
                    sFacilityName = selectedFacility.model.title;

                GUILayout.Box("" + sFacilityName, Yellowtext);
                GUILayout.Space(5);

                fAlt = selectedFacility.RadiusOffset;

                objectPos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(selectedFacility.gameObject.transform.position);
                disObjectLat = KKMath.GetLatitudeInDeg(objectPos);
                disObjectLon = KKMath.GetLongitudeInDeg(objectPos);

                if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Alt. " + fAlt.ToString("#0.0") + "m", LabelInfo);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Lat. " + disObjectLat.ToString("#0.000"), LabelInfo);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Lon. " + disObjectLon.ToString("#0.000"), LabelInfo);
                    GUILayout.Space(5);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                string sPurpose = "";

                if (sFacilityType == "Hangar")
                {
                    sPurpose = "Craft can be stored in this building for launching from the base at a later date. The building has limited space.";
                    bHalfwindow = true;
                }
                else if (sFacilityType == "Barracks")
                {
                    sPurpose = "This facility provides a temporary home for base-staff. Other facilities can draw staff from the pool available at this facility.";
                    bHalfwindow = true;
                }
                else if (sFacilityType == "RadarStation")
                {
                    sPurpose = "This facility tracks craft in the planet's atmosphere at a limited range. It provides bonuses for recovery operations by the nearest open base.";
                    bHalfwindow = true;
                }
                else if (sFacilityType == "Research")
                {
                    sPurpose = "This facility carries out research and generates Science.";
                    bHalfwindow = true;
                }
                else if (sFacilityType == "Business")
                {
                    sPurpose = "This facility carries out business related to the space program in order to generate Funds.";
                    bHalfwindow = true;
                }
                else if (sFacilityType == "TrackingStation")
                {
                    sPurpose = "Thís Facility can be a GroundStation for RemoteTech/CommNet";
                    bHalfwindow = true;
                }
                else if (sFacilityType == "FuelTanks")
                {
                    sPurpose = "This facility stores fuel for craft.";
                    bHalfwindow = false;
                }

                GUILayout.Label(sPurpose, LabelInfo);
                GUILayout.Space(2);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(3);

                SharedInterfaces.OpenCloseFacility(selectedFacility);

                isOpen2 = selectedFacility.myFacilities[0].isOpen;

                GUILayout.Space(2);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(3);

                GUI.enabled = isOpen2;

                if (sFacilityType == "GroundStation")
                {
                    TrackingStationGUI.TrackingInterface(selectedFacility);
                }

                if (sFacilityType == "Hangar")
                {
                    HangarGUI.HangarInterface(selectedFacility);
                }

                if (sFacilityType == "Research" || sFacilityType == "Business" )
                {
                    ProductionGUI.ProductionInterface(selectedFacility, sFacilityType);
                }

                fLqFMax = selectedFacility.model.LqFMax;
                fOxFMax = selectedFacility.model.OxFMax;
                fMoFMax = selectedFacility.model.MoFMax;

                if (fLqFMax > 0 || fOxFMax > 0 || fMoFMax > 0 || sFacilityType == "FuelTanks")
                {
                    FuelTanksGUI.FuelTanksInterface(selectedFacility);
                }

                GUI.enabled = true;

                GUILayout.Space(2);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(2);

                GUI.enabled = isOpen2;
                StaffGUI.StaffingInterface(selectedFacility);
                GUI.enabled = true;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(3);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
