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
using B3d.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Common Data Model core, consists of the big CdmPool of all nodes and edges, and methods to send and receive graph fragments
   /// </summary>
   public class CdmCore : MonoBehaviour
   {
      /// <summary>
      /// Event raised when a request for graph data is fulfilled
      /// </summary>
      public event EventHandler<CdmPoolEventArgs> OnCdmPoolRequestFulfilled = delegate { };

      [Info("Cdm stands for Common Data Model. This component receives data from the specified adaptor and stores it in a common data structure." +
         " This component receives requests for data from the frontend and raises events when new data is received. It also provides a graph cache.")]
      /// <summary>
      /// GameObject carrying an IAdaptor interface
      /// </summary>
      [Tooltip("GameObject carrying an IAdaptor interface")]
      public GameObject IAdaptorGO;

      // The big pool of all nodes and edges
      public CdmGraph CdmPool;

      /// <summary>
      /// Adaptor selector that can return currently active adaptor
      /// </summary>
      AdaptorSelector _adaptorSelector;

      #region UI-Facing Methods
      /// <summary>
      /// Check existence of a node in the underlying source data, specifying a nodeType
      /// The nodeId is passed back as a string in the callbacks
      /// </summary>
      public void CheckNodeExistsInSource(string nodeId, NodeType nodeType, Action<string> callbackOnSuccess, Action<string> callbackOnFail)
      {
         _adaptorSelector.GetChosenAdaptor().CheckNodeExists(nodeId, nodeType, callbackOnSuccess, callbackOnFail);
      }

      /// <summary>
      /// Get (request) a graph fragment: the node requested, the edges requested, and the nodes at the end of each edge, and add them 
      /// to CdmPool. Success means data pushed to Cdm and it will be raising events, the callback on fail is not essential, if anything
      /// fails then just nothing new will appear in the Cdm
      /// </summary>
      /// <param name="nodeId">Id of address or tx</param>
      /// <param name="nodeType">Address or tx</param>
      /// <param name="edgeCountFrom">Edge counts starts at 1</param>
      /// <param name="edgeCountTo">Edge count to</param>
      /// <param name="callbackOnFail">Callback will be executed if the adaptor fails to provide the data</param>
      /// <param name="location">Optionally can record the approximate location of graph root in frontend, used by frontend only not Cdm</param>
      public void GetGraphFragment(string nodeId, NodeType nodeType, int edgeCountFrom, int edgeCountTo, Action<string> callbackOnFail, Vector3 location = default(Vector3))
      {
         if (edgeCountTo < edgeCountFrom)
         {
            Msg.LogError("CdmCore.GetGraphFragment bad edge number range");
            callbackOnFail("CdmCore.GetGraphFragment bad edge number range for NodeId " + nodeId);
            return;
         }

         // Build Request
         CdmRequest r = new CdmRequest()
         {
            NodeId = nodeId,
            NodeType = nodeType,
            EdgeCountFrom = edgeCountFrom,
            EdgeCountTo = edgeCountTo,
            WorldLocation = location
         };

         // Can CdmPool fulfill the request in its entirety? This means we already have all data and dont even need to ask adaptor for data
         CdmGraph g = CanCdmPoolFulfillRequest(r, true);
         if (g != null && !r.IsCacheFillOnly)
         {
            // Tell listeners
            RaiseOnCdmPoolRequestFulfilled(r, g);
         }
         else
         {
            // Ask adaptor to fulfill the request
            _adaptorSelector.GetChosenAdaptor().GetGraphFragment(r, callbackOnFail);
         }
      }

      private void RaiseOnCdmPoolRequestFulfilled(CdmRequest r, CdmGraph g)
      {
         if (g == null)
         {
            Msg.LogWarning("CdmCore.RaiseOnCdmPoolRequestFulfilled was asked to raise pool change event for a null graph");
            return;
         }

         // Raise event for listeners
         CdmPoolEventArgs e = new CdmPoolEventArgs() { CdmRequest = r, CdmGraph = g };
         OnCdmPoolRequestFulfilled(this, e);
      }
      #endregion

      #region Adaptor-Facing Methods
      /// <summary>
      /// Pushes a Cdm Graph fragment g into the cdmpool, fulfilling request r
      /// It is fine to provide more nodes and edges in the graph fragment g, anything extra (over and above what fulfills
      /// the request r) is stored in the pool anyway (the pool acts as a cache), just won't be send to UI listeners.
      /// It is also fine to pass in a null request, this means data will be added to the Cdm cache, but no events will
      /// be raised for UI listeners
      /// </summary>
      /// <param name="r">Request that has been fulfilled either in this call or prior, null if data to be added to cache only</param>
      /// <param name="g">Graph fragment</param>
      public void IngestCdmGraphFragment(CdmRequest r, CdmGraph g)
      {
         // Add the new fragment to the pool         
         // Remember Btc etc specific functionality is supported in Cdm* derived classes
         CdmPool.AddNodeRange(g.GetAllNodes());
         CdmPool.AddEdgeRange(g.GetAllEdges());

         // Add depth information and tell listeners, but only for edges that were requested in request r
         if (r != null && !r.IsCacheFillOnly)
         {
            CdmGraph gFragmentFromPool = CanCdmPoolFulfillRequest(r, false);
            UpdateFragmentAddDepth(r, gFragmentFromPool);
            RaiseOnCdmPoolRequestFulfilled(r, gFragmentFromPool);
         }
         else
         {
            Msg.Log("CdmCore.IngestCdmGraphFragment received a cache-fill call (data added to pool, not passed to frontends)");
         }
      }
      #endregion

      /// <summary>
      /// Add depth values to the graph fragment
      /// </summary>
      private void UpdateFragmentAddDepth(CdmRequest r, CdmGraph gFragmentFromPool)
      {
         // a) node process: if request.node == node being inspected, and depth = -1, make depth 0. If depth = anything else, leave it alone.
         // b) edge process: look at the nodes on either end of the edge (call them n1 and n2) and examine their depth values:
         //      n1  n2
         //      -1  -1  both unknown, should not happen, leave alone
         //       0  -1  make the -1 = 1 (same case as below, order doesnt matter)
         //      -1   X  make the -1 = X + 1 (same case as above, order doesnt matter)
         //       X   Y  both known, leave alone

         // a) Node process
         List<CdmNode> nodes = gFragmentFromPool.GetAllNodes();
         foreach (CdmNode n in nodes)
         {
            if (r.NodeId == n.NodeId)
            {
               if (n.Depth == -1)
               {
                  // the node we're looking at is the node that was asked for in the request. Now, if previously
                  // we didn't know it's depth we know now that it must be the first requested node for this graph.
                  n.Depth = 0;
               }
               break;
            }
         }

         // b) Edge process
         List<CdmEdge> edges = gFragmentFromPool.GetAllEdges();
         foreach (CdmEdge e in edges)
         {
            CdmNode n1 = null;
            CdmNode n2 = null;
            gFragmentFromPool.GetNodesAtEitherEndofEdge(e.EdgeId, out n1, out n2);
            if (n1.Depth == -1 && n2.Depth == -1)
            {
               // both unknown, shouldn't happen, just leave alone
               Msg.LogWarning("CdmCore.UpdateFragmentAddDepth() has both nodes unknown depth for edge: " + e.EdgeId);
            }
            else if (n1.Depth != -1 || n2.Depth != -1)
            {
               // precisely one unknown, so now we know the other
               if (n1.Depth == -1)
               {
                  n1.Depth = n2.Depth + 1;
               }
               else
               {
                  n2.Depth = n1.Depth + 1;
               }
            }
            else
            {
               // both known, this is fine where we're just refreshing known values, so leave alone
            }
         }
      }

      /// <summary>
      /// If the CdmPool can fulfill the request, return the graph fragment required, return null otherwise
      /// </summary>
      /// <param name="r">The request we want to fulfill, which is a node and a range from-to of edge numbers</param>
      /// <param name="isPerfectMatchRequired">When true, the pool must perfectly match request, when false will return whatever pool has, even if incomplete</param>
      /// <returns>Graph fragment that fully or partially fulfills request, or null</returns>
      private CdmGraph CanCdmPoolFulfillRequest(CdmRequest r, bool isPerfectMatchRequired)
      {
         if (r == null)
         {
            return null;
         }
         CdmNode n = CdmPool.FindNodeById(r.NodeId);
         if (n == null)
         {
            return null;
         }

         int edgesAskedForCount = r.EdgeCountTo - r.EdgeCountFrom + 1;
         int edgesFoundCount = 0;
         int endOfEdgeNodesAskedForCount = edgesAskedForCount;
         int endOfEdgeNodesFoundCount = 0;

         // Add requested node to graph fragment we will send
         CdmGraph gFragment = CreateNewGraph(_adaptorSelector.GetChosenAdaptor().GetFamily());
         gFragment.AddNode(n);

         // Get requisite edges
         for (int i = r.EdgeCountFrom; i <= r.EdgeCountTo; i++)
         {
            // This searches both ways around, sender and receiver
            CdmEdge e = CdmPool.FindEdgeByNodeAndNumber(n.NodeId, i);
            if (e != null)
            {
               // Store the edge
               edgesFoundCount++;
               gFragment.AddEdge(e);

               // Store the node at the end of the edge
               CdmNode eoen = CdmPool.FindNodeAtEndOfEdge(n.NodeId, e.EdgeId);
               gFragment.AddNode(eoen);
               endOfEdgeNodesFoundCount++;
            }
         }

         if (isPerfectMatchRequired)
         {
            if (edgesFoundCount >= edgesAskedForCount && endOfEdgeNodesFoundCount >= endOfEdgeNodesAskedForCount)
            {
               return gFragment;
            }
            else
            {
               return null;
            }
         }
         else
         {
            return gFragment;
         }
      }

      private CdmGraph CreateNewGraph(Family family)
      {
         if (family == Family.Btc)
         {
            return new CdmGraphBtc();
         }
         else if (family == Family.Eth)
         {
            return new CdmGraphEth();
         }
         else
         {
            Msg.LogError("CdmCore.CreateNewGraph unknown Family");
            return null;
         }
      }

      private void Awake()
      {
         // Extract the adaptor selector from the game object assigned in inspector
         Msg.Log("CdmCore.Awake is looking for an AdaptorSelector");
         _adaptorSelector = IAdaptorGO.GetComponent<AdaptorSelector>();

         if (_adaptorSelector == null)
         {
            Msg.LogError("CdmCore cannot find AdaptorSelector");
         }

         // Create graph pool of the appropriate family type
         if (CdmPool == null)
         {
            CdmPool = CreateNewGraph(_adaptorSelector.GetChosenAdaptor().GetFamily());
         }
      }

      // Use this for initialization
      void Start()
      {
      }

      // Update is called once per frame
      void Update()
      {

      }

   }
}