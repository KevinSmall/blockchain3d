// Blockchain 3D and VR Explorer: Blockchain Technology Visualization
// Copyright (C) 2018 Kevin Small email:contactweb@blockchain3d.info
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using B3d.Engine.Adaptors;
using B3d.Engine.FrontEnd;
using B3d.Tools;
using System.Collections;
using UnityEngine;

namespace B3d.Demos
{
   public class StartUpController : MonoBehaviour
   {
      [Info("This component is the link from GlobalData (set by login scene) to the FrontEndController (which handles UI requests). The login scene collects startup info like what" +
         " address to show, whether we are online or offline and stores it in GlobalData component. This StartUpController component then selects the appropriate adaptor" +
         " (it uses the adaptor selector on the IAdaptorGO game object to switch adaptor) and calls FrontEndController to display the first graph items.")]
      [Tooltip("Frontend Controller")]
      public FrontEndController FrontEndController;

      /// <summary>
      /// GameObject carrying an IAdaptor interface
      /// </summary>
      [Tooltip("GameObject carrying an IAdaptor interface")]
      public GameObject IAdaptorGO;
      /// <summary>
      /// Adaptor selector, retrieved from the game object IAdaptorGO
      /// </summary>
      private AdaptorSelector _adaptorSel;

      [Header("Default Startup")]
      /// <summary>
      /// If no address or tx provided in GlobalData, or "random" addr tx is asked for, use this and its companion below
      /// </summary>
      public string StartupTxFallBack01 = "00c3b434effcb7a9f267ccc1f3c199694fef85491c3491ef6b29dec2fb2f8592";
      /// <summary>
      /// If no address or tx provided in GlobalData, or "random" addr tx is asked for, use this and its companion above
      /// </summary>
      public string StartupTxFallBack02 = "0fbf2d16605c10dbdec919fafd6f9e5060c33e2838127d9a13af3671fef25316";

      private bool _monoscopicWithHeadTracking = false;

      private void Awake()
      {
         _adaptorSel = IAdaptorGO.GetComponent<AdaptorSelector>();
         if (_adaptorSel == null)
         {
            Msg.LogError("StartUpController cannot find AdaptorSelector");
         }

         // Switch to VR if desired
         if (GlobalData.Instance.CardboardAvailable)
         {
            // Cardboard available in Android, iOS
            if (GlobalData.Instance.CardboardRequested)
            {
               // And we've asked for it to be active
               Msg.Log("StartUpController.Awake: Attempting to switch to Cardboard");
               StartCoroutine(LoadDevice("cardboard", true));
            }
            else
            {
               // So cardboard available, but we've not asked for it, but we still load 
               // it to give us the gyroscopic camera control, essentially mono-scopic VR 
               Msg.Log("StartUpController.Awake: Attempting to switch to mono-scopic VR ( == Cardboard present but disabled)");
               StartCoroutine(LoadDevice("cardboard", false));
               //StartCoroutine(LoadDevice("None", true));
               _monoscopicWithHeadTracking = true;
            }
         }
      }

      // From https://developers.google.com/vr/develop/unity/guides/hybrid-apps
      IEnumerator LoadDevice(string newDevice, bool switchToVrModeAtEnd)
      {
         UnityEngine.XR.XRSettings.LoadDeviceByName(newDevice);
         yield return null;
#if UNITY_EDITOR
         UnityEngine.XR.XRSettings.enabled = false;
         Msg.Log("StartUpController.LoadDevice: Completed switch to " + newDevice + " but in Editor mode so not enabled");
#else
         UnityEngine.XR.XRSettings.enabled = switchToVrModeAtEnd;
         Msg.Log("StartUpController.LoadDevice: Completed switch to " + newDevice + " : enabled: " + switchToVrModeAtEnd);
#endif
      }

      // Use this for initialization
      void Start()
      {
         // Use adaptor selector to choose appropriate adaptor, according to the settings written to the control panel runtime by the login screen
         if (GlobalData.Instance.OfflineAddressDataRequested && GlobalData.Instance.OfflineTransactionDataRequested)
         {
            // We have been asked for pure offline
            Msg.Log("StartupController: Mode = Offline, demo data");

            _adaptorSel.SetChosenAdaptor(AdaptorSelector.AvailableAdaptor.AdaptorBtcOfflineFiles);

            // Use the offline values
            FrontEndController.GetTransactionData(StartupTxFallBack01, 1);
            FrontEndController.GetTransactionData(StartupTxFallBack02, 1);
         }
         else if (GlobalData.Instance.OfflineTransactionDataRequested)
         {
            // We are asked for a "random" start, and we're online
            Msg.Log("StartupController: Mode = Online, random start position");

            _adaptorSel.SetChosenAdaptor(AdaptorSelector.AvailableAdaptor.AdaptorBtcDotInfo);
            
            // Use the "random" values provided to the component
            //FrontEndController.GetAddressData("17mjNZWa3LVXnpiewKae3VkWyDCqKwt7PV", 1);
            FrontEndController.GetTransactionData(StartupTxFallBack01, 1);
            FrontEndController.GetTransactionData(StartupTxFallBack02, 1);
         }
         else
         {
            // We are asked for a specified start position either an address or a transaction that has been pre-validated
            Msg.Log("StartupController: Mode = Online, specified start position");

            _adaptorSel.SetChosenAdaptor(AdaptorSelector.AvailableAdaptor.AdaptorBtcDotInfo);

            if (!string.IsNullOrEmpty(GlobalData.Instance.StartTx))
            {
               FrontEndController.GetTransactionData(GlobalData.Instance.StartTx, 1);
            }
            else if (!string.IsNullOrEmpty(GlobalData.Instance.StartAddr))
            {
               FrontEndController.GetAddressData(GlobalData.Instance.StartAddr, 1);
            }
            else
            {
               // We're supposed to receive pre-validated tx or address, not blanks
               // But let's startup anyway (we could be in editor play mode) with a known good start point
               Msg.LogWarning("StartUpController: Mode = Online, using default start...");
               FrontEndController.GetTransactionData(StartupTxFallBack01, 1);
               FrontEndController.GetTransactionData(StartupTxFallBack02, 1);
            }
         }

         // Various tests  

         // Tx
         // CdmCore.GetGraphFragment("00c3b434effcb7a9f267ccc1f3c199694fef85491c3491ef6b29dec2fb2f8592", NodeType.Tx, 1, OnGetNodeFailed);
         // OLDER CdmCore.GetGraphFragment("0fbf2d16605c10dbdec919fafd6f9e5060c33e2838127d9a13af3671fef25316", NodeType.Tx, 1, OnGetNodeFailed);

         // this is a tx inside 17 nimja, on an addr that has input and output         
         //CdmCore.GetGraphFragment("84f3ff57816950ec4eed0cb800182b91995ceb10effd422f74b1f3c063a1d6b0", NodeType.Tx, 1, OnGetNodeFailed);

         // Addr
         // [33 bees] is massive 42k tx, paged, the offline file is just up to 20
         //CdmCore.GetGraphFragment("33bzHo3UmE3eMtWb9VhztzH27t8DjXsDCc", NodeType.Addr, 1, OnGetNodeFailed);

         // [17 nimja]
         //FrontEndController.GetAddressData("17mjNZWa3LVXnpiewKae3VkWyDCqKwt7PV", 1);
         //_frontEndController.GetTransactionData("7f9dbf1518cc060c4d61cfb7ff5e717e75de7194388477c0af08ac1a1633e253", 1);

         if (GlobalData.Instance.CardboardAvailable && GlobalData.Instance.CardboardRequested)
         {
            // If cardboard requested AND available then we are in VR mode, so the [X] (which maps to escape on keyboard) means quit app
            // copied from GVR demo manager
            Input.backButtonLeavesApp = true;
         }

      }

      // Update is called once per frame
      void Update()
      {
         if (_monoscopicWithHeadTracking)
         {
            UpdateHeadTrackingForVRCameras();
         }

         // Exit when (X) is tapped. Copied from GVR demo manager
         if (Input.GetKeyDown(KeyCode.Escape))
         {
            if (GlobalData.Instance.CardboardAvailable && GlobalData.Instance.CardboardRequested)
            {
               // If cardboard requested AND available then we are in VR mode, so the [X] (which maps to escape on keyboard) means quit app
               Application.Quit();
            }
         }
      }

      private void UpdateHeadTrackingForVRCameras()
      {
         Quaternion pose = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRSettings.enabled ? UnityEngine.XR.XRNode.Head : UnityEngine.XR.XRNode.CenterEye);
         Camera[] cams = Camera.allCameras;
         for (int i = 0; i < cams.Length; i++)
         {
            Camera cam = cams[i];
            if (cam.targetTexture == null && cam.cullingMask != 0)
            {
               cam.GetComponent<Transform>().localRotation = pose;
            }
         }
      }
   }
}
