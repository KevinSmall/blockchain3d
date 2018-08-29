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
using B3d.Engine.Cdm;
using B3d.Tools;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace B3d.Demos
{
   /// <summary>
   /// Login screen 
   /// Note workflow is that if a specific addr or tx is entered, the LetsGo button is always pressable even if addr/tx remains unvalidated
   /// </summary>
   public class LoginScreenController : MonoBehaviour
   {
      [Info("This component does the checks for internet access, and checks existence of the user entered address or transaction." +
        " This component needs an adaptor to use to do this. ")]
      /// <summary>
      /// GO carrying an IAdaptor object (since Unity doesnt easily expose interfaces)
      /// </summary>
      [Tooltip("GameObject carrying an IAdaptor interface, since Unity doesnt easily expose interfaces to inspector")]
      public GameObject IAdaptorGO;

      /// <summary>
      /// Adaptor
      /// </summary>
      private IAdaptor _iAdaptor;

      /// <summary>
      /// The scene to load when finished 
      /// </summary>
      [Tooltip("The scene to load when finished")]
      public string SceneToLoadWhenFinished;

      [Header("Connections to Screen Elements")]
      public Text LabelVersion;
      /// <summary>
      /// Screen element for: Show any old starting tx or address (random)
      /// </summary>
      public Toggle OptionA_ShowAny;
      /// <summary>
      /// Screen element for: Show specific starting tx or address
      /// </summary>
      public Toggle OptionB_ShowSpec;
      public InputField InputField_AddrTx;
      /// <summary>
      /// Screen element for: Show offline demo
      /// </summary>
      public Toggle OptionC_ShowOff;

      public Toggle OptionA_View3d;
      public Toggle OptionA_ViewVr;

      public Text ConnectionLog;
      public Text Instructions;
      public Button ButtonLetsGo;
      public Text NewsText;

      [Header("Images")]
      /// <summary>
      /// Image to use when fading screen in/out.  Expect a full screen sized Image perhaps
      /// attached to a canvas under the main camera.
      /// </summary>
      public Image Fader;

      /// <summary>
      /// For storing player prefs
      /// </summary>
      private const string c_addr_or_tx_value = "addr_or_tx_value";

      /// <summary>
      /// connection log displayed on gui
      /// </summary>
      private string _connLog;

      private enum ConnCheckState
      {
         Idle,
         NetworkCheck,
         NetworkCheckPassed,
         NetworkCheckFailed,
      }
      private ConnCheckState _connCheckState;

      // Faffing around with whether or not LetGo button is active
      private bool _addrCheckFailed = false;
      private bool _addrCheckComplete = false;
      private bool _txCheckFailed = false;
      private bool _txCheckComplete = false;
      private bool _addrAndTxCheckWereRequested;

      /// <summary>
      /// Flag true if we're shutting down this screen and moving to next
      /// </summary>
      private bool _letsGoApprovedIsInProgress = false;
      private bool _cardboardAvailable = false;

      // Instructions displayed on screen
      private const string c_inst_standalone = "Use mouse and keys WASD to fly around. Use keys QE to float up and down, L to toggle labels, P to toggle physics, M to toggle mouse invert, Esc key to toggle free mouse. Look at an address (cube) or transaction (sphere) and left click to see more links.";
      private const string c_inst_phone_3d = "Move device around to look around the world. Gaze at an address (cube) or transaction (sphere) and single tap to see more links from it. Gaze at empty space and tap to float in that direction. Double tap to stop floating.";
      private const string c_inst_phone_vr = "Look around the world at addresses (cubes) and transactions (spheres). Gaze at an address or transaction and single tap to see more links from it. Gaze at empty space and single tap to float in that direction. Double tap to stop floating.";

      void Awake()
      {
         // Extract the IAdaptorBtc from the game object assigned in inspector (necessary since Unity doesnt show interfaces in inspector)
         _iAdaptor = IAdaptorGO.GetComponent<IAdaptorBtc>();
         if (_iAdaptor == null)
         {
            Msg.LogError("Cannot find IAdaptorBtc");
         }
      }

      // Use this for initialization
      void Start()
      {
         // Do the fade, always
         FadeSetupAndActivate();

         // Check for cardboard being available in the project build (eg it is there for Android, prob iOS, but not Windows, Mac)
         string[] d = UnityEngine.XR.XRSettings.supportedDevices;
         for (int i = 0; i < d.Length; i++)
         {
            Msg.Log("VRSettings.supportedDevices: " + d[i]);
            if (d[i] == "cardboard")
            {
               _cardboardAvailable = true;
            }
         }
         // Called from login scene
         LabelVersion.text = GetVersion();

         // Defaults
         _connCheckState = ConnCheckState.Idle;
         _connLog = "";
         ConnectionLog.text = "";
         Instructions.text = "";

         // Only offer cardboard if we found it in the build
         OptionA_ViewVr.interactable = _cardboardAvailable;

         //OptionC_ShowOff.interactable = false;
         _letsGoApprovedIsInProgress = false;

         // Try to get stored value
         InputField_AddrTx.text = PlayerPrefs.GetString(c_addr_or_tx_value, "");

         // Any remote settings?
         if (NewsText != null)
         {
            NewsText.text = "";
         }
         RemoteSettings.Updated += new RemoteSettings.UpdatedEventHandler(OnRemoteUpdate);

         CheckConnectionInvokable();
      }

      /// <summary>
      /// May be called by invoke
      /// </summary>
      private void CheckConnectionInvokable()
      {
         _connCheckState = ConnCheckState.NetworkCheck;
         Msg.Log("Checking internet connection...");
         _iAdaptor.CheckConnectionPossible(() => OnCheckConnectionOk(), () => OnCheckConnectionFail());
      }

      private void OnCheckConnectionOk()
      {
         // All OK
         Msg.Log("LoginScreenController.OnCheckConnectionOk callback was called");
         _connCheckState = ConnCheckState.NetworkCheckPassed;
         ConnectionLog.text = "Internet connection ok.";
         // to be safe, the logic to enable buttons has been moved to update()      
      }

      private void OnCheckConnectionFail()
      {
         // Fail
         Msg.Log("LoginScreenController.OnCheckConnectionFail callback was called");
         _connCheckState = ConnCheckState.NetworkCheckFailed;
         ConnectionLog.text = "No internet connection found, will keep trying. Try the offline demo?";

         // Try again 
         Invoke("CheckConnectionInvokable", 5f);
      }

      /// <summary>
      /// Remote settings might change what we display
      /// </summary>
      private void OnRemoteUpdate()
      {
         //Debug.Log("In remote update");
         //Debug.Log(RemoteSettings.GetString("B3D_NEWS"));
         string remoteNews = RemoteSettings.GetString("B3D_NEWS");

         if (remoteNews != null && remoteNews != "")
         {
            if (NewsText != null)
            {
               NewsText.text = remoteNews;
            }
         }
      }

      private string GetVersion()
      {
         string s = "v" + Application.version;

#if UNITY_EDITOR
         s += " Editor";
#elif UNITY_WEBGL
      s += " WebGL";
#elif UNITY_STANDALONE_OSX
      s += " OSX";
#elif UNITY_STANDALONE_WIN
      s += " Win";
#elif UNITY_STANDALONE_LINUX
      s += " Linux";
#elif UNITY_IOS
      s += " iOS";
#elif UNITY_ANDROID
      s += " Android";
#elif UNITY_WEBGL
      s += " WebGL";
#elif UNITY_WSA
      s += " UWP";
#else
      s += " Unk";
#endif
         return s;
      }

      public void OnToggleOptionBShowSpecChanged()
      {
         OptionB_ShowSpec.isOn = true;
         //PlayerPrefs.SetString(c_addr_or_tx_value, InputField_AddrTx.text);

      }

      public void OnToggleOptionBShowSpecEnded()
      {
         // after edit ends, store value
         PlayerPrefs.SetString(c_addr_or_tx_value, InputField_AddrTx.text);
      }

      // for the button that looks like a label next to toggle grp 1, option C
      public void OnToggle1_OptionC_ShowOff_ButtonPress()
      {
         OptionC_ShowOff.isOn = true;
      }

      public void LetsGoToWebsite()
      {
         Msg.Log("LetsGoToWebsite");
         Application.OpenURL("https://blockchain3d.info/");
      }

      /// <summary>
      /// All validation of whether or not LetsGo should be interactable at all is done in Update()
      /// We can assume that we are online if we need to be
      /// </summary>
      public void LetsGo()
      {
         // in certain cases (eg specific tx or addr entered and we've not validated it yet) we cant approve the lets go (ie cant move to next scene)
         bool letsGoApproved = false;
         // these can be filled in ValidateAddrOrTxAndLetsGo() and then later examined by GameController
         GlobalData.Instance.StartAddr = null;
         GlobalData.Instance.StartTx = null;

         if (OptionA_ShowAny.isOn)
         {
            // Start at "random" sort of address/tx
            GlobalData.Instance.OfflineTransactionDataRequested = true;
            GlobalData.Instance.OfflineAddressDataRequested = false;
            letsGoApproved = true;
         }
         else if (OptionB_ShowSpec.isOn)
         {
            // Start at specific address or tx
            GlobalData.Instance.OfflineTransactionDataRequested = false;
            GlobalData.Instance.OfflineAddressDataRequested = false;

            // the final LetsGoApproved() call will happen asynch later, when addr or tx is validated
            ValidateAddrOrTxAndLetsGo();
            letsGoApproved = false;
         }
         else if (OptionC_ShowOff.isOn)
         {
            // Offline play
            GlobalData.Instance.OfflineTransactionDataRequested = true;
            GlobalData.Instance.OfflineAddressDataRequested = true;
            letsGoApproved = true;
         }

         // Setting for viewer
         // The world scene will sort out switching to VR mode, we just do the determination here
         if (OptionA_View3d.isOn)
         {
            GlobalData.Instance.CardboardAvailable = _cardboardAvailable;
            GlobalData.Instance.CardboardRequested = false;
         }
         else if (OptionA_ViewVr.isOn)
         {
            GlobalData.Instance.CardboardAvailable = _cardboardAvailable;
            GlobalData.Instance.CardboardRequested = true;
         }

         // Go
         if (letsGoApproved)
         {
            LetsGoApproved();
         }
      }

      private void ValidateAddrOrTxAndLetsGo()
      {
         // Work out if it is tx or addr and validate it
         // Address
         string addressId = InputField_AddrTx.text;
         _iAdaptor.CheckNodeExists(addressId, NodeType.Addr, (addr) => OnCheckAddrOk(addr), (addr) => OnCheckAddrFail(addr));
         _addrAndTxCheckWereRequested = true;

         // Tx
         string txId = InputField_AddrTx.text;
         _iAdaptor.CheckNodeExists(txId, NodeType.Tx, (tx) => OnCheckTxOk(tx), (tx) => OnCheckTxFail(tx));
         _addrAndTxCheckWereRequested = true;
      }

      private void OnCheckTxOk(string tx)
      {
         _txCheckComplete = true;
         _txCheckFailed = false;
         Msg.Log("Transaction " + tx + " validated ok, world starting...");
         GlobalData.Instance.StartAddr = null;
         GlobalData.Instance.StartTx = tx;
         LetsGoApproved();
      }

      private void OnCheckTxFail(string tx)
      {
         _txCheckComplete = true;
         _txCheckFailed = true;
         Msg.Log("Transaction " + tx + " did not validate ok.");
      }

      private void OnCheckAddrOk(string addr)
      {
         _addrCheckComplete = true;
         _addrCheckFailed = false;
         Msg.Log("Address " + addr + " validated ok, world starting...");
         GlobalData.Instance.StartAddr = addr;
         GlobalData.Instance.StartTx = null;
         LetsGoApproved();
      }

      private void OnCheckAddrFail(string addr)
      {
         _addrCheckComplete = true;
         _addrCheckFailed = true;
         Msg.Log("Address " + addr + " did not validate ok.");
      }

      private void LetsGoApproved()
      {
         _letsGoApprovedIsInProgress = true;
         ActivateFader(true, 0.85f);
         Invoke("LoadScene", 0.9f);
      }

      private void LoadScene()
      {
         //sceneLoaded = Add a delegate to this to get notifications when a scene has loaded.
         //sceneUnloaded = Add a delegate to this to get notifications when a scene has unloaded
         SceneManager.LoadSceneAsync(SceneToLoadWhenFinished, LoadSceneMode.Single);
      }

      private void ConnLog(string s)
      {
         if (String.IsNullOrEmpty(_connLog))
            _connLog = s;
         else
            _connLog = _connLog + "\n" + s;
      }

      // Update is called once per frame
      void Update()
      {
         if (_letsGoApprovedIsInProgress)
            return;

         // Appropriate instructions
         if (_cardboardAvailable)
         {
            if (OptionA_View3d.isOn)
               Instructions.text = c_inst_phone_3d;
            else
               Instructions.text = c_inst_phone_vr;
         }
         else
         {
            Instructions.text = c_inst_standalone;
         }

         // Handle final launch (the LetsGoApproved() part) after LetsGo() pressed and tx or addr was validated
         if (ButtonLetsGo.interactable == true /*this is here to ensure network connection present*/ && OptionB_ShowSpec.isOn == true)
         {
            // both checks done
            if (_txCheckComplete && _addrCheckComplete)
            {
               // at least one check passed
               if (!_txCheckFailed || !_addrCheckFailed)
               {
                  if (_txCheckFailed)
                     ConnectionLog.text = "Address is valid.";
                  else
                     ConnectionLog.text = "Transaction is valid.";

                  LetsGoApproved();
               }
               else if (_txCheckFailed && _addrCheckFailed)
               {
                  // both checks failed
                  // yes I know should not set this every frame
                  ConnectionLog.text = "Bad Address or Transaction. Check or try another option.";
               }
            }
            else if (_addrAndTxCheckWereRequested)
            {
               ConnectionLog.text = "Checking address or transaction...";
            }
         }

         // This handles the "Lets Go" button status, is it interactable or not?
         // In general we care only about network connectivity
         switch (_connCheckState)
         {
            case ConnCheckState.Idle:
               ButtonLetsGo.interactable = false;
               break;
            case ConnCheckState.NetworkCheck:
               ButtonLetsGo.interactable = false;
               break;
            case ConnCheckState.NetworkCheckPassed:
               ButtonLetsGo.interactable = true;
               break;
            case ConnCheckState.NetworkCheckFailed:
               ButtonLetsGo.interactable = false;
               break;
            default:
               break;
         }

         // Over-ride is for offline play
         if (OptionC_ShowOff.isOn == true)
         {
            ButtonLetsGo.interactable = true;
         }

      }

      #region fade
      /// <summary>
      /// Fades from black at the start
      /// </summary>
      void FadeSetupAndActivate()
      {
         Fader.gameObject.SetActive(true);
         //Fader.SetActive(true);
         Fader.GetComponent<Image>().color = new Color(0, 0, 0, 1f);
         ActivateFader(false, 1f);
      }

      /// Fades the fader in or out depending on the state
      /// </summary>
      /// <param name="state">false = fade from black to clear, true = fade from clear to black</param>
      public void ActivateFader(bool fadeFromClearToBlack, float duration)
      {
         if (Fader == null)
         {
            return;
         }
         Fader.gameObject.SetActive(true);
         if (fadeFromClearToBlack)
         {
            // clear to black
            StartCoroutine(FadeImage(Fader, duration, new Color(34f / 255f, 44f / 255f, 54f / 255f, 1f)));
         }
         else
         {
            // black to clear
            StartCoroutine(FadeImage(Fader, duration, new Color(34f / 255f, 44f / 255f, 54f / 255f, 0f)));
         }
      }

      public IEnumerator FadeImage(Image target, float duration, Color color)
      {
         if (target == null)
            yield break;

         float alpha = target.color.a;

         for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
         {
            if (target == null)
               yield break;
            Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha, color.a, t));
            target.color = newColor;
            yield return null;
         }
         target.color = color;

         // we have faded to clear, we can disable self (doesnt work well as in fade to black cases we want the fade to be retained)
         if (color.a < 0.1f)
         {
            Fader.gameObject.SetActive(false);
         }
      }
      #endregion

   }
}