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
//
// Some Force Directed Graph code based on:
// Bamfax/ForceDirectedNodeGraph3DUnity licensed under GNU GPL v3.0
// https://github.com/Bamfax/ForceDirectedNodeGraph3DUnity

using B3d.Engine.Cdm;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace B3d.Demos
{
   public class GraphEdgeBrain : MonoBehaviour
   {
      public CdmEdgeBtc CdmEdgeBtc;

      public Material LinkMaterial;

      public string id;
      public int edgeNumInSource;
      public int edgeNumInTarget;
      public GameObject source;
      public GameObject target;
      public static float intendedLinkLength;
      public static float forceStrength;

      public EdgeType BitLinkTypePrimary
      {
         get { return _bitLinkTypePrimary; }
      }
      [SerializeField]
      private EdgeType _bitLinkTypePrimary;

      public EdgeType CdmEdgeType;
      
      /// <summary>
      /// In mixed link cases, this is the other type, and left null when there is only primary
      /// </summary>
      [SerializeField]
      private EdgeType? _bitLinkTypeSecondary;

      /// <summary>
      /// For links of type input or output
      /// </summary>
      public GameObject ArrowHeadPrimary;

      /// <summary>
      /// For links of type mixed
      /// </summary>
      private GameObject _arrowHeadSecondary;

      /// <summary>
      /// Value in Milli BTC
      /// </summary>   
      private float _valueMBtcPrimary;
      private bool _isValueKnownPrimary;
      public Canvas LabelCanvasPrimary;
      public Text LabelValuePrimary;

      // Secondary display only used if the link is mixed (both inputs and outputs)
      private float _valueMBtcSecondary;
      private bool _isValueKnownSecondary;
      private GameObject _goCanvasSecondary;
      private Canvas _labelCanvasSecondary;
      private Text _labelValueSecondary;

      private static GraphFactoryBtc _graphFactory;

      private Color _startingColor;

      private Component sourceRigidBody;
      private Component targetRigidBody;
      private LineRenderer _lineRenderer;
      private float intendedLinkLengthSquared;
      private float distSqrNorm;
      private Vector3 _lineCenterPos;

      public void Awake()
      {
      }

      // Use this for initialization
      void Start()
      {
         // TODO this is inefficient and ugly
         _graphFactory = FindObjectOfType<GraphFactoryBtc>();
         _lineRenderer = gameObject.AddComponent<LineRenderer>();
         //lineRenderer = GetComponent<LineRenderer>();

         // Text label is mBTC and starts off invisible (to avoid having to fade it, it just appears after main fade in complete)
         LabelValuePrimary.text = _valueMBtcPrimary.ToString("n2");
         LabelCanvasPrimary.enabled = false;
         if (_valueMBtcPrimary > 0f)
         {
            _isValueKnownPrimary = true;
         }

         // Color link according to type
         Color c;

         switch (_bitLinkTypePrimary)
         {
            case EdgeType.Input:
               c = GraphFactoryBtc.Instance.Visuals.ColLinkInput;
               break;
            case EdgeType.Output:
               c = GraphFactoryBtc.Instance.Visuals.ColLinkOutput;
               break;
            default:
               c = GraphFactoryBtc.Instance.Visuals.ColLinkDefault;
               break;
         }
         // Override when we are mixed
         if (_bitLinkTypeSecondary != null)
         {
            c = GraphFactoryBtc.Instance.Visuals.ColLinkMixed;
         }

         _startingColor = new Color(c.r, c.g, c.b, c.a);
         // set initial alpha to zero because we always fade in
         c.a = 0f;

         //draw line
         _lineRenderer.material = LinkMaterial;
         _lineRenderer.material.SetColor("_Color", c);
         _lineRenderer.startWidth = GraphFactoryBtc.Instance.Visuals.WidthLinkDefault;
         _lineRenderer.endWidth = GraphFactoryBtc.Instance.Visuals.WidthLinkDefault;
         _lineRenderer.positionCount = 2;
         _lineRenderer.sortingOrder = -1;

         // send receive shadows
         _lineRenderer.receiveShadows = false;
         _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

         _lineRenderer.SetPosition(0, source.transform.position);
         _lineRenderer.SetPosition(1, target.transform.position);
         sourceRigidBody = source.GetComponent<Rigidbody>();
         targetRigidBody = target.GetComponent<Rigidbody>();
         intendedLinkLengthSquared = _graphFactory.LinkIntendedLinkLength * _graphFactory.LinkIntendedLinkLength;

         // arrow head
         //_arrowHeadRenderer.material.SetColor("_Color", c);
         // flip head for inputs - better done in update as we have to move it anyway, remember the arrow is attached to the parent game object
         // not to the line (renderer) and so since line renderer is redrawn each frame, so arrow must move too. If a true 3d line were used, could
         // attach arrowhead to the line truly and then not move it at all.

         // Added to ensure line width adjusted after line renderer created
         AdjustLineWidth();
         EnableAndPositionAllCanvases();

         // Fade in link
         StartCoroutine(FadeIn());

         // Cheat "fade" for label
         if (true)
         {
            Invoke("CanvasEnable", GraphFactoryBtc.Instance.Visuals.NodeFadeInTimeSeconds);
         }
      }

      /// <summary>
      /// Initial link setup, can be type Input or Output (type mixed is for internal use to this class only)
      /// </summary>
      /// <param name="initialBitLinkType">Can be type Input or Output</param>
      /// <param name="isValueKnown"></param>
      /// <param name="valueMBtc"></param>
      public void SetBitLinkTypeAndValue_Initial(EdgeType initialBitLinkType, bool isValueKnown, float valueMBtc, EdgeType cdmEdgeType)
      {
         // TYPE
         CdmEdgeType = cdmEdgeType;
         _bitLinkTypePrimary = initialBitLinkType;
         //Debug.Log("BitLinkBrain.SetBitLinkTypeAndValue_Initial: adding primary link type: " + initialBitLinkType.ToString() + " to link");

         // VALUE
         _isValueKnownPrimary = isValueKnown;
         _valueMBtcPrimary = valueMBtc;

         AdjustLineWidth();
         EnableAndPositionAllCanvases();
      }

      private void AdjustLineWidth()
      {
         // total value
         float totalValue = _valueMBtcPrimary + _valueMBtcSecondary;

         float newWidth = GraphFactoryBtc.Instance.Visuals.WidthLinkMin;
         float minWidth = GraphFactoryBtc.Instance.Visuals.WidthLinkMin;
         float maxWidth = GraphFactoryBtc.Instance.Visuals.WidthLinkMax;

         // Step 1 normalise mbtc value to 0..1
         float t = NormaliseMbtcValue(totalValue);

         // Step 2 smooth the 0..1 to a pleasing curve
         newWidth = Mathf.SmoothStep(minWidth, maxWidth, t);

         // at present this method can be called after instantiate(), after awake() but before start() so line renderer can be blank
         if (_lineRenderer != null)
         {
            _lineRenderer.startWidth = newWidth;
            _lineRenderer.endWidth = newWidth;
            //Debug.Log("EXISTING LINE WIDTH IS " + _lineRenderer.startWidth);
            //float currentWidth = _lineRenderer.startWidth;
            //float targetWidth = newWidth;
            //float multi = targetWidth / currentWidth;

            //Debug.Log("MULTIPLIER IS " + multi);
            //_lineRenderer.widthMultiplier = multi;
         }

         //Debug.Log("LINE WIDTH MAPPING: TotalValue: " + totalValue + " maps to Width: " + newWidth);
      }

      /// <summary>
      /// Normalise the BTC value to a 0..1 value
      /// This doesnt need to know and shouldnt care about what the actual line width will be
      /// </summary>
      private float NormaliseMbtcValue(float totalValue)
      {
         // Window is the focus point that will be where the line width changes
         // Outside this will be more or less fixed small, fixed large line width
         float mbtcWindowLow = 100f;
         float mbtcWindowHigh = 4000f;
         float mbtcWindowWidth = mbtcWindowHigh - mbtcWindowLow;
         // Proportion is how much the focus point takes up in the normalised value
         float windowProportionLow = 0.16f;
         float windowProportionHigh = 0.98f;

         // There are then three zones, smaller than Btc window, in Btc window and above it
         if (totalValue < mbtcWindowLow)
         {
            // ZONE 1 = linear from 0 to lower proportion         
            return Mathf.Lerp(0f, windowProportionLow, totalValue / mbtcWindowLow);
         }
         else if (totalValue >= mbtcWindowLow && totalValue <= mbtcWindowHigh)
         {
            // ZONE 2 = linear from lower to high proportion 
            return Mathf.Lerp(windowProportionLow, windowProportionHigh, (totalValue - mbtcWindowLow) / mbtcWindowWidth);
         }
         else
         {
            // flat
            return 1.0f;
         }
      }

      private float GetSuitableLabelWidth(string textToMeasure)
      {
         int len = textToMeasure.Length;
         if (len >= 16)
            return 160f;
         if (len >= 11)
            return 120f;
         else if (len >= 6)
            return 80f;
         else
            return 60f;
      }

      private void EnableAndPositionAllCanvases()
      {
         // and also set line width?
         //GlobalData.Instance.WidthLinkMin;

         // string lenth >11 needs canvas lengtheed
         // Enable
         if (_isValueKnownPrimary)
         {
            LabelCanvasPrimary.enabled = true;
            LabelValuePrimary.text = _valueMBtcPrimary.ToString("n2");

            float labelWidth = GetSuitableLabelWidth(LabelValuePrimary.text);
            RectTransform rt = LabelCanvasPrimary.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(labelWidth, 30f);
         }
         else
         {
            LabelCanvasPrimary.enabled = false;
            LabelValuePrimary.text = "";
         }

         if (_isValueKnownSecondary)
         {
            _labelCanvasSecondary.enabled = true;
            _labelValueSecondary.text = _valueMBtcSecondary.ToString("n2");

            float labelWidth = GetSuitableLabelWidth(_labelValueSecondary.text);
            RectTransform rt = _labelCanvasSecondary.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(labelWidth, 30f);

         }
         else
         {
            if (_labelCanvasSecondary != null)
            {
               _labelCanvasSecondary.enabled = false;
               _labelValueSecondary.text = "";
            }
         }

         // Position
         if (_isValueKnownPrimary && !_isValueKnownSecondary)
         {
            // Primary label alone
            LabelCanvasPrimary.transform.localPosition = new Vector3(0f, 0.2f, 0.8f);
         }
         else if (_isValueKnownPrimary && _isValueKnownSecondary)
         {
            // Primary and secondary labels
            LabelCanvasPrimary.transform.localPosition = GetSecondaryCanvasLabelLocalTransformAdjustment(_bitLinkTypePrimary);
            _labelCanvasSecondary.transform.localPosition = GetSecondaryCanvasLabelLocalTransformAdjustment((EdgeType)_bitLinkTypeSecondary);
         }
      }

      private Vector3 GetSecondaryCanvasLabelLocalTransformAdjustment(EdgeType edgeType)
      {
         if (edgeType == EdgeType.Input)
         {
            return new Vector3(-0.37f, 0.42f, 0.67f);
         }
         else if (edgeType == EdgeType.Output)
         {
            return new Vector3(0.18f, -1.04f, -0.32f);
         }
         else
         {
            return Vector3.zero;
         }
      }

      /// <summary>
      /// Link is a known duplicate (ie has input and output) and we're givn information for the other "half" of it
      /// </summary>
      /// <param name="additionalBitLinkType">Can be type Input or Output</param>
      /// <param name="isValueKnown"></param>
      /// <param name="valueMBtc"></param>
      public void SetBitLinkTypeAndValue_Additional(EdgeType additionalBitLinkType, bool isValueKnown, float valueMBtc, EdgeType cdmEdgeType)
      {
         // TYPE
         CdmEdgeType = cdmEdgeType;
         
         //-- early outs
         // TYPE
         if (_bitLinkTypeSecondary != null)
         {
            Debug.LogWarning("GraphEdgeBrain.SetBitLinkTypeAndValue_Additional: unexpected (not necessarily a problem) - link already has a secondary type");
            return;
         }

         if (_bitLinkTypePrimary == additionalBitLinkType)
         {
            Debug.LogWarning("GraphEdgeBrain.SetBitLinkTypeAndValue_Additional: unexpected - the new link type is same as existing");
            return;
         }
         //-- end early outs

         // We have for the first time been promoted to a mixed link
         _bitLinkTypeSecondary = additionalBitLinkType;
         //Debug.Log("GraphEdgeBrain.SetBitLinkTypeAndValue_Additional: adding secondary link type: " + additionalBitLinkType.ToString() + " to link");

         // Create new arrowhead and place it
         UpdateSelfTransformPositionToBeLineCenter();
         _arrowHeadSecondary = Instantiate(ArrowHeadPrimary, transform.position, Quaternion.identity, transform) as GameObject;
         _arrowHeadSecondary.transform.localRotation = new Quaternion(-1.0f, 0f, 0f, 0f); // quaternion values obtained by playing with inspector
         _arrowHeadSecondary.transform.localPosition = new Vector3(0f, -0.1f, 0f);        // position values obtained by playing with inspector

         // Create new label and place it
         _goCanvasSecondary = Instantiate(LabelCanvasPrimary.gameObject, transform.position, Quaternion.identity, transform) as GameObject;
         // billboard rotator will do this _goCanvasSecondary.transform.localRotation = new Quaternion(-1.0f, 0f, 0f, 0f); // quaternion values obtained by playing with inspector
         _goCanvasSecondary.transform.localPosition = new Vector3(0f, -0.1f, 0f);        // position values obtained by playing with inspector

         //_labelCanvasSecondary
         _labelCanvasSecondary = _goCanvasSecondary.GetComponent<Canvas>();
         _labelValueSecondary = _labelCanvasSecondary.GetComponentInChildren<Text>(true);

         // UPDATE 2nd PANEL
         _valueMBtcSecondary = valueMBtc;
         _isValueKnownSecondary = isValueKnown;

         AdjustLineWidth();
         EnableAndPositionAllCanvases();
      }

      /// <summary>
      /// Also used by an Invoke!
      /// </summary>
      public void CanvasEnable()
      {
         if (!GlobalData.Instance.IsGlobalLinkLabelActive)
            return;

         if (_isValueKnownPrimary)
         {
            LabelCanvasPrimary.enabled = true;
         }

         if (_bitLinkTypeSecondary != null && _isValueKnownSecondary && _labelCanvasSecondary != null)
         {
            _labelCanvasSecondary.enabled = true;
         }
      }

      public void CanvasDisable()
      {
         LabelCanvasPrimary.enabled = false;

         if (_bitLinkTypeSecondary != null && _labelCanvasSecondary != null)
         {
            _labelCanvasSecondary.enabled = false;
         }
      }

      public IEnumerator FadeIn()
      {
         float alpha = 0f;

         //Debug.Log("FADE: fade in starts");

         float t = 0f;
         while (t < 1.0f)
         {
            Color newColor = new Color(_startingColor.r, _startingColor.g, _startingColor.b, Mathf.SmoothStep(alpha, _startingColor.a, t));
            //GetComponent<Renderer>().material.color = newColor;
            _lineRenderer.material.SetColor("_Color", newColor);

            // Debug.Log("FADE: link col" + newColor.a);

            t += Time.deltaTime / GraphFactoryBtc.Instance.Visuals.NodeFadeInTimeSeconds;

            yield return null;

         }
         Color finalColor = _startingColor;
         //GetComponent<Renderer>().material.color = finalColor;
         _lineRenderer.material.SetColor("_Color", finalColor);

         //Debug.Log("FADE: fade in ends link col" + finalColor.a);

      }

      // Update is called once per frame
      void Update()
      {
         //--------------------------------------------------------------------------------------------
         // Line      
         //--------------------------------------------------------------------------------------------
         intendedLinkLengthSquared = _graphFactory.LinkIntendedLinkLength * _graphFactory.LinkIntendedLinkLength;

         float linkLength = Vector3.Distance(source.transform.position, target.transform.position);
         if (linkLength < 1.1f)
         {
            // really short link, so we're just starting up and moving, let it draw whole point to point
            _lineRenderer.SetPosition(0, source.transform.position);
            _lineRenderer.SetPosition(1, target.transform.position);
         }
         else
         {
            // dont draw inside cubes or spheres, shorten the line a bit
            // shorten by half a unit at either end
            // o----o--------------------------------------o-----o
            //
            float fracLengthStart = 0.5f / linkLength;
            float fracLengthEnd = (linkLength - 0.5f) / linkLength;
            Vector3 newSource = Vector3.Lerp(source.transform.position, target.transform.position, fracLengthStart);
            Vector3 newTarget = Vector3.Lerp(source.transform.position, target.transform.position, fracLengthEnd);
            _lineRenderer.SetPosition(0, newSource);
            _lineRenderer.SetPosition(1, newTarget);
         }
         //Debug.Log("GraphEdgeBrain: this: " + this.name + " source: " + source.name + " source pos:" + source.transform.position + 
         //   " target: " + target.name + " target pos: " + target.transform.position);

         UpdateSelfTransformPositionToBeLineCenter();

         //--------------------------------------------------------------------------------------------
         // Arrowhead
         // Our transform (although it has no drawable components on it, it just acts as parent for arrowhead) needs to move to allow arrow head to position easily
         //--------------------------------------------------------------------------------------------
         transform.LookAt(source.transform);

         if (_bitLinkTypePrimary == EdgeType.Input)
         {
            transform.Rotate(-90f, 180f, 0f);
         }
         else //if (_bitLinkType == BitLinkType.Output) does this help or not?
         {
            transform.Rotate(-90f, 0f, 0f);
         }
         // for type mixed rotate not the transform but the child gameobject itself, and this is already done at point of creation
         // so should be no further need to rotate the additional arrow for mixed node cases

      }

      private void UpdateSelfTransformPositionToBeLineCenter()
      {
         _lineCenterPos = source.transform.position + 0.5f * (target.transform.position - source.transform.position);
         transform.position = _lineCenterPos;
      }

      void FixedUpdate()
      {
         if (!_graphFactory.AllStatic)
         {
            FdgAttraction();
         }
      }

      void FdgAttraction()
      {
         Vector3 forceDirection = sourceRigidBody.transform.position - targetRigidBody.transform.position;
         float distSqr = forceDirection.sqrMagnitude;

         if (distSqr > intendedLinkLengthSquared)
         {
            distSqrNorm = distSqr / intendedLinkLengthSquared;

            Vector3 targetRbImpulse;
            Vector3 sourceRbImpulse;

            targetRbImpulse = forceDirection.normalized * forceStrength * distSqrNorm;
            sourceRbImpulse = forceDirection.normalized * -1 * forceStrength * distSqrNorm;

            // Attract
            ((Rigidbody)targetRigidBody as Rigidbody).AddForce(targetRbImpulse);
            ((Rigidbody)sourceRigidBody as Rigidbody).AddForce(sourceRbImpulse);

            //Debug.Log(this.gameObject.name + ": targetRbI" + targetRbImpulse + " sourceRbI" + sourceRbImpulse);
         }
      }
   }
}
