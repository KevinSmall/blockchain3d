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
using B3d.Tools;
using System;
using UnityEngine;

namespace B3d.Engine.FrontEnd
{
   /// <summary>
   /// Handles UI and sends requests to the Cdm pool. Listens to Cdm pool for new nodes and creates
   /// game objects for them by calling graph factory, attaching visuals and physics
   /// </summary>
   public class FrontEndController : MonoBehaviour
   {
      public static FrontEndController Instance;

      [Info("This component is the link between the Cdm (Common Data Model) component and the GraphFactory. Assign both of these in the inspector.  GraphFactoryGO is a GameObject that contains a script that implements IGraphFactory.")]

      /// <summary>
      /// Core Data Model CDM script
      /// </summary>
      [Tooltip("Core Data Model CDM script")]
      public CdmCore CdmCore;

      /// <summary>
      /// GraphFactory to create game objects
      /// </summary>
      [Tooltip("Game Object containing a script that implements IGraphFactory")]
      public GameObject GraphFactoryGO;

      public IGraphFactory GraphFactory;

      /// <summary>
      /// Number of additional edges to display each time a node is tapped 
      /// </summary>
      public int PageSize = 8;

      void Awake()
      {
         if (Instance == null)
         {
            Msg.Log("FrontEndController created");
            Instance = this;
            //DontDestroyOnLoad(gameObject);
         }
         else
         {
            Msg.Log("FrontEndController re-creation attempted, destroying the new one");
            Destroy(gameObject);
         }

         // Get the graph factory
         GraphFactory = GraphFactoryGO.GetComponent<IGraphFactory>();

         // Listen to Cdm events
         CdmCore.OnCdmPoolRequestFulfilled += OnCdmPoolGraphAdded;
      }
      
      public void CheckAddressExists(string id, Action<string> callbackOnSuccess, Action<string> callbackOnFail)
      {
         CdmCore.CheckNodeExistsInSource(id, NodeType.Addr, callbackOnSuccess, callbackOnFail);
      }
      
      public void CheckTransactionExists(string id, Action<string> callbackOnSuccess, Action<string> callbackOnFail)
      {
         CdmCore.CheckNodeExistsInSource(id, NodeType.Tx, callbackOnSuccess, callbackOnFail);
      }

      /// <summary>
      /// Sends a request to the Cdm for the given address and page. Optionally a world space location can be added
      /// which will be available in the returned request. Method OnCdmPoolGraphAdded() is called when request
      /// is fullfilled
      /// </summary>
      /// <param name="id">Address id</param>
      /// <param name="page">Page of data</param>
      /// <param name="location">Optional locaation in world space that this will be near</param>
      public void GetAddressData(string id, int page, Vector3 location = default(Vector3))
      {
         int edgeCountFrom = 0;
         int edgeCountTo = 0;
         CdmHelpers.GetFromToForPage(PageSize, page, out edgeCountFrom, out edgeCountTo);
         CdmCore.GetGraphFragment(id, NodeType.Addr, edgeCountFrom, edgeCountTo, OnGetNodeFailed, location);
      }

      /// <summary>
      /// Sends a request to the Cdm for the given tx and page. Optionally a world space location can be added
      /// which will be available in the returned request. Method OnCdmPoolGraphAdded() is called when request
      /// is fullfilled
      /// </summary>
      /// <param name="id">Tx id</param>
      /// <param name="page">Page of data</param>
      /// <param name="location">Optional locaation in world space that this will be near</param>
      public void GetTransactionData(string id, int page, Vector3 location = default(Vector3))
      {
         int edgeCountFrom = 0;
         int edgeCountTo = 0;
         CdmHelpers.GetFromToForPage(PageSize, page, out edgeCountFrom, out edgeCountTo);
         CdmCore.GetGraphFragment(id, NodeType.Tx, edgeCountFrom, edgeCountTo, OnGetNodeFailed, location);
      }

      private void OnCdmPoolGraphAdded(object sender, CdmPoolEventArgs args)
      {
         // Note: it is possible to filter to just showing graphs that WE requested from the cdmpool. To do this, examine args.CdmRequest to see
         // what the request was. For now this is not needed and not done.

         CdmGraphBtc gb = args.CdmGraph as CdmGraphBtc;
         if (gb == null)
         {
            Msg.LogError("FrontEndController.OnCdmPoolGraphAdded heard event but graph fragment is null or not correct type");
            return;
         }
         else
         {
            Msg.Log("FrontEndController.OnCdmPoolGraphAdded heard event that graph fragment" + gb.GraphId + " is to be added to pool");
         }

         // Create nodes and edges
         Vector3 location = args.CdmRequest.WorldLocation;

         foreach (CdmNode n in gb.GetAllNodes())
         {
            CdmNodeBtc nb = n as CdmNodeBtc;
            GraphFactory.CreateOrUpdateNode(nb, location);
         }

         foreach (CdmEdge e in gb.GetAllEdges())
         {
            CdmEdgeBtc eb = e as CdmEdgeBtc;
            GraphFactory.CreateOrUpdateEdge(eb);
         }

         // An audio-visual feast
         if (AudioManager.Instance != null)
         {
            AudioManager.Instance.PlayPop();
         }
      }

      private void OnGetNodeFailed(string s)
      {
         Msg.LogWarning("FrontEndController called GetNode but the call failed. Reason: " + s);
      }
   }
}