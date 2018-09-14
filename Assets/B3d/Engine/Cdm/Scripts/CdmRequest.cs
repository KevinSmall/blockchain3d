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

using UnityEngine;

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// A CdmRequest is a request by a frontend for some graph data.
   /// A requests asks for a node, a certain number of edges radiating out from that node, plus all the nodes at the end of those edges 
   /// Optionally (at the frontend's discretion) it can also store a location in 3d space, so that when request is fulfilled
   /// the frontend knows roughly where the new nodes belong. These location coords are "pass-through" from the Cdm and Adaptor perspective.
   /// </summary>
   public class CdmRequest
   {
      public string NodeId;      
      public NodeType NodeType;
      public int EdgeCountFrom;
      public int EdgeCountTo;

      /// <summary>
      /// If a request is flagged as being cache fill only, it means that when CdmCore receives data it will not 
      /// send it on to the frontends (eg by raising events).
      /// </summary>
      public bool IsCacheFillOnly;

      /// <summary>
      /// Roughly where new nodes might exist in the world. Held for convenience of the frontend, if the frontend
      /// chooses to store them. These location coords are "pass-through" from the Cdm and Adaptor perspective.
      /// </summary>
      public Vector3 WorldLocation;
   }
}
