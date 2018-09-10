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

using B3d.Engine.Cdm;
using B3d.Engine.FrontEnd;
using B3d.Tools;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace B3d.Demos
{
   /// <summary>
   /// There is one of these scripts attached to each node (be it a transaction, input address or output address).
   /// This is the brain of the node, handling audio, color, UI etc.
   /// </summary>
   public class GraphNodeBrain : MonoBehaviour //, IGvrGazeResponder
   {
      //---------------------------------------------------------------------------
      // Common data (common to all node types)
      //---------------------------------------------------------------------------
      public CdmNodeBtc CdmNodeBtc;

      private EventTrigger _et;

      private bool _beingGazedAt = false;
      /// <summary>
      /// If many nodes are spawned close to a parent flower then the HUD display can pick up all sorts of nonsense (lots of gazeat being fired)
      /// To compensate, we suppress any interaction for the first second or so of existence
      /// </summary>
      private bool _isInteractionSuppressed = true;
      private float _interactionSuppressTime;

      /// <summary>
      /// "Global" view showing if any of the GraphNodeBrains are being gazed at (only one at a time can be gazed at)
      /// </summary>
      public static bool IsAnythingBeingGazedAt = false;

      private Color _startingColor;
      private Text _vrHud;

      /// <summary>
      /// The static hud that floats in world space, but looks like screen space because it is attached to the camera
      /// </summary>
      private GameObject _3dHudStatic;

      /// <summary>
      /// Collection of text objects used by whatever HUD is being used.
      /// If >1 HUD needs to be available at once, need >1 of these.
      /// </summary>
      private UiHudElements _bitHudElements;

      public NodeType NodeType;

      public string Id = "not filled yet";
      /// <summary>
      /// first and last few chars of Id
      /// </summary>
      private string _shortId = "not...yet";

      /// <summary>
      /// Value in Milli BTC
      /// On a Tx this is the tx value
      /// On an address it is the balance(?)
      /// </summary>   
      public float ValueMBtc;

      /// <summary>
      /// Total edges for this node, use 0 to represent unknown 
      /// </summary>      
      public int TotalEdges
      {
         get { return _totalEdges; }
         set
         {
            //Debug.Log("TotalEdges changed: old value " + _totalEdges);
            _totalEdges = value;
            //Debug.Log("TotalEdges changed: new value " + _totalEdges);
         }
      }
      protected int _totalEdges = 0;

      /// <summary>
      /// Current number of edges displayed for this node
      /// </summary>
      [SerializeField]
      private int _currentEdges;

      /// <summary>
      /// True if all possible edges are displayed for this node
      /// </summary>
      public bool IsAllEdgesDisplayed
      {
         get
         {
            if (_totalEdges == 0) // total not known
            {
               return false;
            }
            else if (_currentEdges >= _totalEdges)
            {
               return true;
            }
            else
            {
               return false;
            }
         }
      }

      /// <summary>
      /// When requesting address or tx data, there could be 1000's of links out (ie addr>tx, or tx>addr).
      /// FrontEndController holds page size, but the brain must remember how many pages we've seen
      /// </summary>
      private int _edgesPageCount = 0;

      private GameObject _shell;

      private bool _firstNodeCreated = false;

      //---------------------------------------------------------------------------
      // Transaction Specific data
      //---------------------------------------------------------------------------
      public DateTime TxDate = DateTime.Now;
      public int BlockHeight;  // "block_height":471440,
      public string RelayedBy; //"relayed_by":"31.172.85.47"

      //---------------------------------------------------------------------------
      // Address Specific data
      //---------------------------------------------------------------------------
      // nothing yet

      public void Awake()
      {
         // Hook up event trigger (which we can diable if no more links to display - means no expanding recticle);
         _et = GetComponent<EventTrigger>();

         // Hook up to VR HUD
         GameObject goVr = GameObject.FindGameObjectWithTag("VrHud");
         if (goVr != null)
         {
            // grab handles to all the labels
            _vrHud = goVr.GetComponent<Text>();
         }

         // Hook up to 3D HUD
         GameObject go3d = GameObject.FindGameObjectWithTag("3dHudStatic");
         if (go3d != null)
         {
            // grab handles to all the labels
            _bitHudElements.AddrOrTxLabel = go3d.gameObject.transform.Find("Panel/AddrOrTxLabel").gameObject.GetComponent<Text>();
            _bitHudElements.AddrOrTxValue = go3d.gameObject.transform.Find("Panel/AddrOrTxValue").gameObject.GetComponent<Text>();
            _bitHudElements.ShortLabel = go3d.gameObject.transform.Find("Panel/ShortLabel").gameObject.GetComponent<Text>();
            _bitHudElements.ShortValue = go3d.gameObject.transform.Find("Panel/ShortLabel/ShortValue").gameObject.GetComponent<Text>();
            _bitHudElements.BlockLabel = go3d.gameObject.transform.Find("Panel/BlockLabel").gameObject.GetComponent<Text>();
            _bitHudElements.BlockValue = go3d.gameObject.transform.Find("Panel/BlockLabel/BlockValue").gameObject.GetComponent<Text>();
            _bitHudElements.LinkedAddrOrTxLabel = go3d.gameObject.transform.Find("Panel/LinkedAddrOrTxLabel").gameObject.GetComponent<Text>();
            _bitHudElements.LinkedAddrOrTxValue = go3d.gameObject.transform.Find("Panel/LinkedAddrOrTxLabel/LinkedAddrOrTxValue").gameObject.GetComponent<Text>();
            _bitHudElements.mBtcLabel = go3d.gameObject.transform.Find("Panel/mBtcLabel").gameObject.GetComponent<Text>();
            _bitHudElements.mBtcValue = go3d.gameObject.transform.Find("Panel/mBtcLabel/mBtcValue").gameObject.GetComponent<Text>();
            _bitHudElements.DateLabel = go3d.gameObject.transform.Find("Panel/DateLabel").gameObject.GetComponent<Text>();
            _bitHudElements.DateValue = go3d.gameObject.transform.Find("Panel/DateLabel/DateValue").gameObject.GetComponent<Text>();
            _bitHudElements.InfoLabel = go3d.gameObject.transform.Find("Panel/InfoLabel").gameObject.GetComponent<Text>();

            _3dHudStatic = go3d;
         }

         // Move HUD to more convenient positon for VR
         if (GlobalData.Instance.CardboardRequested)
         {
            RectTransform rt = _3dHudStatic.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0.04f, 0.72f, 2.3f);
            //rt.rotation = Quaternion.Euler(new Vector3(-9f, 0f, 0f));
         }

         // Hook up shell
         _shell = gameObject.transform.Find("Shell").gameObject;
      }

      public void CurrentLinksIncrement()
      {
         _currentEdges++;
         if (TotalEdges > 0 && _currentEdges >= TotalEdges)
         {
            _currentEdges = TotalEdges;
            // Switching off events prevents the recticle appearing but also prevents HUD appearing so can't do it :(
            //_et.enabled = false;

            // Adjust visuals if we are now incomplete
            if (NodeType == NodeType.Addr)
            {
               // visuals now show it is complete
               _startingColor = GraphFactoryBtc.Instance.Visuals.ColNodeAddrComplete;
               GetComponent<Renderer>().material.color = _startingColor;
            }
            else if (NodeType == NodeType.Tx)
            {
               // visuals now show it is complete
               _startingColor = GraphFactoryBtc.Instance.Visuals.ColNodeTxComplete;
               GetComponent<Renderer>().material.color = _startingColor;
            }
         }

         // A link increment could change any UI/HUD values, so refresh them if we are interactable
         if (!_isInteractionSuppressed)
         {
            RefreshUI();
         }
      }

      /// <summary>
      /// Called by Invoke
      /// </summary>
      public void EnableInteraction()
      {
         _isInteractionSuppressed = false;
      }

      public void Start()
      {
         _isInteractionSuppressed = true;
         _interactionSuppressTime = GraphFactoryBtc.Instance.Visuals.NodeInteractionSuppressTime;
         Invoke("EnableInteraction", _interactionSuppressTime);

         Color c;
         switch (NodeType)
         {
            case NodeType.Tx:
               if (IsAllEdgesDisplayed)
               {
                  c = GraphFactoryBtc.Instance.Visuals.ColNodeTxComplete;
               }
               else
               {
                  c = GraphFactoryBtc.Instance.Visuals.ColNodeTxIncomplete;
               }
               break;

            case NodeType.Addr:
               if (IsAllEdgesDisplayed)
               {
                  c = GraphFactoryBtc.Instance.Visuals.ColNodeAddrComplete;
               }
               else
               {
                  c = GraphFactoryBtc.Instance.Visuals.ColNodeAddrIncomplete;
               }
               break;

            default:
               c = GraphFactoryBtc.Instance.Visuals.ColNodeDefault;
               break;
         }
         _startingColor = c; // new Color(c.r, c.g, c.b, c.a);

         // set initial alpha to zero because we always fade in
         GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 0f);

         _shortId = Id.Substring(0, 5) + "..." + Id.Right(3);

         //_shaderNotHighlighted = gameObject.GetCom1ponent<Renderer>().material.shader; //Shader.Find(@"Diffuse");
         //_shaderHighlighted = Shader.Find(@"Self-Illumin/BumpedSpecular");

         StartCoroutine(FadeIn());
      }

      public IEnumerator FadeIn()
      {
         float alpha = 0f;

         float t = 0f;
         while (t < 1.0f)
         {
            Color newColor = new Color(_startingColor.r, _startingColor.g, _startingColor.b, Mathf.SmoothStep(alpha, _startingColor.a, t));
            GetComponent<Renderer>().material.color = newColor;

            t += Time.deltaTime / GraphFactoryBtc.Instance.Visuals.NodeFadeInTimeSeconds;

            yield return null;

         }
         GetComponent<Renderer>().material.color = _startingColor;
      }

      public void SetFirstEverCreated()
      {
         _firstNodeCreated = true;
         // currently we're marking the first ever node created by showing its shell
         DisplayShell(true);
      }

      private void DisplayShell(bool displayShell)
      {
         _shell.SetActive(displayShell);
      }

      private void SetGazedAt(bool gazedAt)
      {
         _beingGazedAt = gazedAt;
         IsAnythingBeingGazedAt = gazedAt;

         // Highlight color effect
         if (gazedAt)
         {
            //GetComponent<Renderer>().material.shader = _shaderHighlighted;
            GetComponent<Renderer>().material.color = GraphFactoryBtc.Instance.Visuals.ColAddrGazeAt;
            //GetComponent<Renderer>().material.color = new Color(_startingColor.r, _startingColor.g, _startingColor.b, 0.1f);
            UiMovePointerClickMoveForwards.IsGazing = true;
         }
         else
         {
            //GetComponent<Renderer>().material.shader = _shaderNotHighlighted;
            GetComponent<Renderer>().material.color = _startingColor;
            UiMovePointerClickMoveForwards.IsGazing = false;
         }

         RefreshUI();

         // AUDIO
         if (gazedAt)
         {
            //GetComponent<GvrAudioSource>().Play();
         }
      }

      /// <summary>
      /// Refresh UI, whatever it may be (VR text or screen space or whatever)
      /// Position it in space correctly.
      /// </summary>
      public void RefreshUI()
      {
         RefreshUiValues();

         if (_beingGazedAt)
         {
            // we are being looked at
            // VR HUD
            if (_vrHud != null)
            {
               // only things to do with positioning Vr HUD here (not values)
               _vrHud.text = RefreshUiVrHudText();
            }

            // 3D HUD
            if (_3dHudStatic != null)
            {
               // only things to do with positioning Vr HUD here (not values)            
               // this just makes it float above the in game object
               //Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + 1f, this.transform.position.z);
               //_3dHudStatic.transform.position = pos;
               //_3dHudStatic.transform.parent = this.gameObject.transform;

            }
         }
         //else
         //{ 
         // We're not being looked at - eventually will fade out panel
         //if (_vrHud != null)
         //{
         //   _vrHud.text = "";
         //}
         //}

      }

      /// <summary>
      /// DELETE ME
      /// </summary>
      private string RefreshUiVrHudText()
      {
         string s = "";
         string totalLinksStr = (TotalEdges == 0) ? "?" : TotalEdges.ToString();

         if (_firstNodeCreated)
            s = "First Node Created\n";

         switch (NodeType)
         {
            case NodeType.Tx:
               if (IsAllEdgesDisplayed)
               {
                  s += "Tx: " + _shortId;
                  s += "\n" + "mBTC: " + ValueMBtc.ToString("n2");
                  s += "\n" + TxDate.ToString("d", DateTimeFormatInfo.InvariantInfo);
                  s += "\n" + "Block: " + BlockHeight;
                  //s += "\n" + "Relay: " + RelayedBy;
                  s += "\n" + "Addr: " + _currentEdges + " / " + totalLinksStr;
               }
               else
               {
                  s += "Tx: " + _shortId;
                  s += "\n" + "mBTC: " + ValueMBtc.ToString("n2");
                  s += "\n" + TxDate.ToString("d", DateTimeFormatInfo.InvariantInfo);
                  s += "\n" + "Block: " + BlockHeight;
                  //s += "\n" + "Relay: " + RelayedBy;
                  s += "\n" + "Addr: " + _currentEdges + " / " + totalLinksStr;
                  s += "\n" + "Click for more";
               }
               break;

            case NodeType.Addr:
               if (IsAllEdgesDisplayed)
               {
                  s += "Addr: " + _shortId;
                  s += "\n" + "mBTC: " + ValueMBtc.ToString("n2");
                  s += "\n" + "Tx: " + _currentEdges + " / " + totalLinksStr;
               }
               else
               {
                  s += "Addr: " + _shortId;
                  s += "\n" + "mBTC: " + ValueMBtc.ToString("n2");
                  s += "\n" + "Tx: " + _currentEdges + " / " + totalLinksStr;
                  s += "\n" + "Click for more";
               }
               break;

            default:
               break;
         }

         return s;
      }

      /// <summary>
      /// Refresh UI values, so populate values in BitHudElements without caring about
      /// what actual HUD consumes them
      /// </summary>
      private void RefreshUiValues()
      {
         string linksTotalStr = (TotalEdges == 0) ? "?" : TotalEdges.ToString();
         string linksExpression = _currentEdges + " / " + linksTotalStr;

         switch (NodeType)
         {
            case NodeType.Tx:
               if (IsAllEdgesDisplayed)
               {
                  _bitHudElements.AddrOrTxLabel.text = "Transaction";
                  _bitHudElements.AddrOrTxValue.text = Id;
                  _bitHudElements.ShortLabel.text = "Short Tx.:";
                  _bitHudElements.ShortValue.text = _shortId;
                  _bitHudElements.BlockLabel.text = "Block:";
                  _bitHudElements.BlockValue.text = BlockHeight.ToString();
                  _bitHudElements.LinkedAddrOrTxLabel.text = "Links:";
                  _bitHudElements.LinkedAddrOrTxValue.text = linksExpression;
                  _bitHudElements.mBtcLabel.text = "mBTC:";
                  _bitHudElements.mBtcValue.text = ValueMBtc.ToString("n2");
                  _bitHudElements.DateLabel.text = "Date:";
                  _bitHudElements.DateValue.text = TxDate.ToString("d", DateTimeFormatInfo.InvariantInfo);
                  _bitHudElements.InfoLabel.text = "All addresses shown";
               }
               else
               {
                  _bitHudElements.AddrOrTxLabel.text = "Transaction";
                  _bitHudElements.AddrOrTxValue.text = Id;
                  _bitHudElements.ShortLabel.text = "Short Tx.:";
                  _bitHudElements.ShortValue.text = _shortId;
                  _bitHudElements.BlockLabel.text = "Block:";
                  _bitHudElements.BlockValue.text = BlockHeight.ToString();
                  _bitHudElements.LinkedAddrOrTxLabel.text = "Links:";
                  _bitHudElements.LinkedAddrOrTxValue.text = linksExpression;
                  _bitHudElements.mBtcLabel.text = "mBTC:";
                  _bitHudElements.mBtcValue.text = ValueMBtc.ToString("n2");
                  _bitHudElements.DateLabel.text = "Date:";
                  _bitHudElements.DateValue.text = TxDate.ToString("d", DateTimeFormatInfo.InvariantInfo);
                  _bitHudElements.InfoLabel.text = "Click tx. for more links";
               }
               break;

            case NodeType.Addr:
               if (IsAllEdgesDisplayed)
               {
                  _bitHudElements.AddrOrTxLabel.text = "Address";
                  _bitHudElements.AddrOrTxValue.text = Id;
                  _bitHudElements.ShortLabel.text = "Short Addr.:";
                  _bitHudElements.ShortValue.text = _shortId;
                  _bitHudElements.BlockLabel.text = String.Empty;
                  _bitHudElements.BlockValue.text = String.Empty;
                  _bitHudElements.LinkedAddrOrTxLabel.text = "Links:";
                  _bitHudElements.LinkedAddrOrTxValue.text = linksExpression;
                  _bitHudElements.mBtcLabel.text = "mBTC:";
                  _bitHudElements.mBtcValue.text = ValueMBtc.ToString("n2");
                  _bitHudElements.DateLabel.text = String.Empty;
                  _bitHudElements.DateValue.text = String.Empty;
                  _bitHudElements.InfoLabel.text = "Click addr. for more links";
               }
               else
               {
                  _bitHudElements.AddrOrTxLabel.text = "Address";
                  _bitHudElements.AddrOrTxValue.text = Id;
                  _bitHudElements.ShortLabel.text = "Short Addr.:";
                  _bitHudElements.ShortValue.text = _shortId;
                  _bitHudElements.BlockLabel.text = String.Empty;
                  _bitHudElements.BlockValue.text = String.Empty;
                  _bitHudElements.LinkedAddrOrTxLabel.text = "Links:";
                  _bitHudElements.LinkedAddrOrTxValue.text = linksExpression;
                  _bitHudElements.mBtcLabel.text = "mBTC:";
                  _bitHudElements.mBtcValue.text = ValueMBtc.ToString("n2");
                  _bitHudElements.DateLabel.text = String.Empty;
                  _bitHudElements.DateValue.text = String.Empty;
                  _bitHudElements.InfoLabel.text = "All transactions shown";
               }
               break;

            default:
               break;
         }

         if (_firstNodeCreated)
         {
            _bitHudElements.AddrOrTxLabel.text += " (first shown, has a shell around it)";
         }
      }

      public void RequestAddressData()
      {
         _edgesPageCount += 1; // assume success (allows multiple clicks on node to queue up incoming data and it should work ok)
         Debug.Log("GraphNodeBrain is requesting address data for: " + Id + " page " + _edgesPageCount);
         FrontEndController.Instance.GetAddressData(Id, _edgesPageCount);  // fire and forget
      }

      public void RequestTxData()
      {
         _edgesPageCount += 1; // assume success (allows multiple clicks on node to queue up incoming data and it should work ok)
         Debug.Log("GraphNodeBrain is requesting tx data for: " + Id + " page " + _edgesPageCount);
         FrontEndController.Instance.GetTransactionData(Id, _edgesPageCount);  // fire and forget
      }

      /// Called when the user is looking on a GameObject with this script,
      /// as long as it is set to an appropriate layer (see GvrGaze).
      public void OnGazeEnter()
      {
         if (_isInteractionSuppressed)
            return;

         SetGazedAt(true);
      }

      /// Called when the user stops looking on the GameObject, after OnGazeEnter
      /// was already called.
      public void OnGazeExit()
      {
         if (_isInteractionSuppressed)
            return;

         SetGazedAt(false);
      }

      /// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
      public void OnGazeTrigger()
      {
         if (_isInteractionSuppressed)
            return;

         switch (NodeType)
         {
            case NodeType.Tx:
               if (IsAllEdgesDisplayed)
               {
                  // nothing to do, all data present
               }
               else
               {
                  GraphFactoryBtc.Instance.AllStatic = false; // if physics was off, it needs to come back on
                  RequestTxData();
               }
               break;

            case NodeType.Addr:
               if (IsAllEdgesDisplayed)
               {
                  // nothing to do, all data present
               }
               else
               {
                  GraphFactoryBtc.Instance.AllStatic = false; // if physics was off, it needs to come back on
                  RequestAddressData();
               }
               break;

            default:
               break;
         }
      }
   }
}