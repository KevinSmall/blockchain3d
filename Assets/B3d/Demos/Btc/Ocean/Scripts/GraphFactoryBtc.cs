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
using System.Collections.Generic;
using UnityEngine;

namespace B3d.Demos
{
   /// <summary>
   /// Create game objects for a graph, nodes and edges, controls physics
   /// </summary>
   public class GraphFactoryBtc : MonoBehaviour, IGraphFactory
   {
      public static GraphFactoryBtc Instance;

      [Info("This component creates the game objects for a graph.\nIt genererates node and edge game objects from the supplied prefabs and controls the physics. GraphFactory should implement the interface IGraphFactory and be assigned in the inspector to the FrontEndController.")]
      /// <summary>
      /// All nodes and edge game objects will be created in this folder
      /// </summary>
      [Tooltip("All nodes and edge game objects will be created in this folder")]
      public Transform ParentFolder;

      [Header("Physics")]
      public bool AllStatic = false;
      public bool RepulseActive = true;
      public GameObject NodeAddrPrefab;
      public GameObject NodeTxPrefab;
      public GraphEdgeBrain LinkPrefab;
      public float NodeCreationRange = 7f;
      public float GlobalGravity = 9.81f;
      public float RepulseForceStrength = 5f;
      public float NodeForceSphereRadius = 30f;
      public float LinkForceStrength = 5f;
      public float LinkIntendedLinkLength = 2.8f;

      [Header("Visuals")]
      public GraphVisualSettings Visuals;


      /// <summary>
      /// Index of ids (node or edge) with their GameObject
      /// </summary>
      private Dictionary<string, GameObject> _graphIndex;

      /// <summary>
      /// Global count of nodes (addr or tx)
      /// </summary>
      private static int nodeCount;

      /// <summary>
      /// Global count of links, used when naming the game objects
      /// </summary>
      private static int linkCount;

      void Awake()
      {
         if (Instance == null)
         {
            Msg.Log("GraphFactory created");
            Instance = this;
            _graphIndex = new Dictionary<string, GameObject>();

            //DontDestroyOnLoad(gameObject);
         }
         else
         {
            Msg.Log("GraphFactory re-creation attempted, destroying the new one");
            Destroy(gameObject);
         }
      }

      void IGraphFactory.CreateOrUpdateNode(CdmNode nodeNew, Vector3 location)
      {
         CdmNodeBtc nodeNewBtc = nodeNew as CdmNodeBtc;

         // Do we exist already?
         GameObject nodeExistingGo;
         if (_graphIndex.TryGetValue(nodeNewBtc.NodeId, out nodeExistingGo))
         {
            UpdateExistingNodeGoData(nodeExistingGo, nodeNewBtc);

            Msg.Log("GraphFactory.CreateOrUpdateNode: Node refreshed: " + nodeExistingGo.gameObject.name + " at " + nodeExistingGo.gameObject.transform.position);

            return;
         }

         GameObject nodeCreated = null;
         Vector3 createPos = GetRandomPosNear(location);

         Msg.LogWarning("created near:" + location);

         nodeCreated = InstantiateNode(createPos, nodeNewBtc.NodeType);

         if (nodeCreated != null)
         {
            GraphNodePhysics nodeNode = nodeCreated.GetComponent<GraphNodePhysics>();
            nodeNode.name = nodeNewBtc.NodeId;
            nodeNode.Text = name;

            UpdateExistingNodeGoData(nodeCreated, nodeNewBtc);

            Msg.Log("GraphFactory.CreateOrUpdateNode: Node created: " + nodeCreated.gameObject.name + " at " + nodeCreated.gameObject.transform.position);
            _graphIndex.Add(nodeNewBtc.NodeId, nodeCreated);
         }
         else
         {
            Msg.LogWarning("GraphFactory.CreateOrUpdateNode: Something went wrong, no node created.");
            return;
         }

      }

      /// <summary>
      /// Create a new edge game object, or update an existing one. New edges get added to _graphIndex list.
      /// </summary>
      void IGraphFactory.CreateOrUpdateEdge(CdmEdge edgeNew)
      {
         CdmEdgeBtc edgeNewBtc = edgeNew as CdmEdgeBtc;

         // Do we exist already?
         GameObject edgeExistingGo;
         if (_graphIndex.TryGetValue(edgeNewBtc.EdgeId, out edgeExistingGo))
         {
            UpdateExistingEdgeGoData(edgeExistingGo, edgeNewBtc);
            return;
         }

         // We are new. Do necessary nodes exist?
         GameObject sourceGo;
         bool sourceExists = _graphIndex.TryGetValue(edgeNewBtc.NodeSourceId, out sourceGo);
         GameObject targetGo;
         bool targetExists = _graphIndex.TryGetValue(edgeNewBtc.NodeTargetId, out targetGo);
         if (!sourceExists || !targetExists)
         {
            Msg.LogWarning("GraphFactory.CreateEdge cannot create edge because source or target GameObject dont exist yet");
            return;
         }

         // Do it
         GameObject createdEdge = null;
         //Msg.Log("GraphFactory.CreateEdge: Edge about to be created: " + edgeNewBtc.EdgeId);
         createdEdge = InstantiateEdge(edgeNewBtc.EdgeId, edgeNewBtc.EdgeIdFriendly, edgeNewBtc.EdgeNumberInSource, edgeNewBtc.EdgeNumberInTarget,
            sourceGo, targetGo, edgeNewBtc.EdgeType, true, edgeNewBtc.ValueInSource, edgeNewBtc.ValueInTarget);

         if (createdEdge == null)
         {
            Msg.LogWarning("GraphFactory.CreateEdge: Edge not created.");
         }
         else
         {
            Msg.Log("GraphFactory.GenerateEdge: Edge created: " + createdEdge.gameObject.name);
            GraphEdgeBrain geb = createdEdge.GetComponent<GraphEdgeBrain>();
            if (geb != null)
            {
               geb.CdmEdgeBtc = edgeNewBtc;
            }

            // index it
            _graphIndex.Add(edgeNewBtc.EdgeId, createdEdge);
         }

      }

      private GameObject InstantiateNode(Vector3 createPos, NodeType nodeType)
      {
         if (nodeType == NodeType.Tx)
         {
            return Instantiate(NodeTxPrefab, createPos, Quaternion.identity, ParentFolder) as GameObject;
         }
         else if (nodeType == NodeType.Addr)
         {
            return Instantiate(NodeAddrPrefab, createPos, Quaternion.identity, ParentFolder) as GameObject;
         }
         else
         {
            Msg.LogWarning("GraphFactory.InstantiateNode unknown node type");
            return null;
         }
      }

      public Vector3 GetRandomPos()
      {
         Vector3 createPos = new Vector3(UnityEngine.Random.Range(0, NodeCreationRange), UnityEngine.Random.Range(0, NodeCreationRange), UnityEngine.Random.Range(0, NodeCreationRange));
         return createPos;
      }

      public Vector3 GetRandomPosNear(Vector3 nearThis)
      {
         return (nearThis + (2f * UnityEngine.Random.onUnitSphere));
      }

      /// <summary>
      /// Pass in a node id or an edge id, get back its world position
      /// </summary>
      /// <param name="id">node or edge id</param>
      /// <returns>world position in the scene</returns>
      private Vector3 GetNodeOrEdgeGameObjectPosition(string id)
      {
         Vector3 pos;
         GameObject go;
         if (_graphIndex.TryGetValue(id, out go))
         {
            pos = go.gameObject.transform.position;
         }
         else
         {
            pos = Vector3.zero;
         }
         return pos;
      }

      private void UpdateExistingNodeGoData(GameObject nodeExistingGo, CdmNodeBtc nodeNewBtc)
      {
         GraphNodeBrain gnb = nodeExistingGo.GetComponent<GraphNodeBrain>();
         if (gnb != null)
         {
            gnb.CdmNodeBtc = nodeNewBtc;
            gnb.NodeType = nodeNewBtc.NodeType;
            gnb.ValueMBtc = nodeNewBtc.FinalBalance;
            gnb.TotalEdges = nodeNewBtc.NodeEdgeCountTotal;
            gnb.Id = nodeNewBtc.NodeId;
            gnb.TxDate = nodeNewBtc.CreateDate;
            gnb.BlockHeight = nodeNewBtc.BlockHeight;
            gnb.RelayedBy = nodeNewBtc.RelayedBy;

            // Special case for first ever created node
            if (!GlobalData.Instance.FirstNodeCreatedYet)
            {
               GlobalData.Instance.FirstNodeCreatedYet = true;
               gnb.SetFirstEverCreated();
            }

            // Update UI text els with the new GraphNodeBrain values
            gnb.RefreshUI();
         }
      }

      private void UpdateExistingEdgeGoData(GameObject edgeExistingGo, CdmEdgeBtc edgeNewBtc)
      {
         Msg.Log("GraphFactory.UpdateExistingEdgeGoData is refreshing edge data");

         // TODO What might need updated? Since Cdm handles merges, it seems safe enough to overwrite whole object.
         GraphEdgeBrain blb = edgeExistingGo.GetComponent<GraphEdgeBrain>();
         if (blb != null)
         {
            blb.CdmEdgeBtc = edgeNewBtc;
            // TODO plus propogate any values across the edge GO ?
         }
      }

      /// <summary>
      /// Create game object for edge
      /// Terrible code, needs refactored
      /// </summary>
      private GameObject InstantiateEdge(string id, string idFriendly, int edgeNumSource, int edgeNumTarget, GameObject source, GameObject target, EdgeType edgeType, bool valueIsKnown, float valueMBtc, float valueMBtcAdditional)
      {
         if (source == null || target == null || (source == target))
         {
            Msg.LogWarning("GraphFactory.InstantiateEdge: source or target do not exist, or are identical. Edge not created.");
            return null;
         }

         // an entirely new link
         GraphEdgeBrain linkObject = Instantiate(LinkPrefab, new Vector3(0, 0, 0), Quaternion.identity, ParentFolder) as GraphEdgeBrain;
         linkObject.id = id;
         linkObject.edgeNumInSource = edgeNumSource;
         linkObject.edgeNumInTarget = edgeNumTarget;
         linkObject.name = idFriendly + "--" + edgeNumSource + "--" + edgeNumTarget; // Game Object name
         linkObject.source = source;
         linkObject.target = target;
         //linkObject.SetBitLinkValue(valueIsKnown, valueMBtc);
         //linkObject.SetBitLinkType(linkType);

         EdgeType linkType = EdgeType.Input;
         if (edgeType == EdgeType.Input)
         {
            linkType = EdgeType.Input;
            linkObject.SetBitLinkTypeAndValue_Initial(linkType, valueIsKnown, valueMBtc, edgeType);

         }
         else if (edgeType == EdgeType.Output)
         {
            linkType = EdgeType.Output;
            linkObject.SetBitLinkTypeAndValue_Initial(linkType, valueIsKnown, valueMBtcAdditional, edgeType);
         }
         else
         {
            // TODO Horrible and needs refactored, this is hangover from proof of concept
            // We are now a mixed link, the method SetBitLinkTypeAndValue_Additional() will handle setting the type to Mixed itself
            Msg.Log("GraphFactory.InstantiateEdge: Link between source " + source.name + " and target " + target.name + " being PROMOTED TO MIXED");
            linkObject.SetBitLinkTypeAndValue_Initial(EdgeType.Input, valueIsKnown, valueMBtc, edgeType);
            linkObject.SetBitLinkTypeAndValue_Additional(EdgeType.Output, valueIsKnown, valueMBtcAdditional, edgeType);

            // this counts towards our link counters of course
            UpdateLinkCountersOnSourceAndTarget(source, target);
         }

         linkCount++;

         // Update link counters
         UpdateLinkCountersOnSourceAndTarget(source, target);

         return linkObject.gameObject;
      }

      private static void UpdateLinkCountersOnSourceAndTarget(GameObject source, GameObject target)
      {
         // update link counters on source and target
         GraphNodeBrain gnbSource = source.GetComponent<GraphNodeBrain>();
         GraphNodeBrain gnbTarget = target.GetComponent<GraphNodeBrain>();
         if (gnbSource != null)
         {
            gnbSource.CurrentLinksIncrement();
         }
         if (gnbTarget != null)
         {
            gnbTarget.CurrentLinksIncrement();
         }
      }

      void Start()
      {
         nodeCount = 0;
         linkCount = 0;
      }

      void Update()
      {
         // Apply any settings changes made here to the links themselves
         GraphEdgeBrain.intendedLinkLength = LinkIntendedLinkLength;
         GraphEdgeBrain.forceStrength = LinkForceStrength;
      }
   }
}