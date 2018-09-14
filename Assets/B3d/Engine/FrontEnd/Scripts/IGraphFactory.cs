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
using UnityEngine;

namespace B3d.Engine.FrontEnd
{
   /// <summary>
   /// Interface defining a graph factory. The graph factory will receive calls to create or update
   /// nodes or edges.
   /// </summary>
   public interface IGraphFactory
   {
      /// <summary>
      /// Create a new, or update an existing, node. Optionally a locaiton may be present, if the frontend stored it when request created.
      /// </summary>
      /// <param name="nodeNew">All the info about the node (cast to appropriate subclass to see details)</param>
      /// <param name="location">Optional location given by frontend when request originally made</param>
      void CreateOrUpdateNode(CdmNode nodeNew, Vector3 location = default(Vector3));

      /// <summary>
      /// Create a new, or update an existing, edge. 
      /// </summary>
      /// <param name="edgeNew">All the info about the edge (cast to appropriate subclass to see details)</param>      
      void CreateOrUpdateEdge(CdmEdge edgeNew);
   }
}