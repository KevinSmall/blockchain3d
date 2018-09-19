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

using System;
using System.Diagnostics;

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Node data, format closely follows GraphML
   /// </summary>
   [DebuggerDisplay("NodeId = {NodeId}")]
   [Serializable]
   public class CdmNode
   {
      /// <summary>
      /// Node identifier. Must be unique across all nodes and all edges for the whole graph always.
      /// </summary>
      public string NodeId;

      /// <summary>
      /// Node type, eg an address or a transaction.
      /// </summary>
      public NodeType NodeType;

      /// <summary>
      /// Total edges that are associated with this node. 0 means unknown.
      /// </summary>
      public int NodeEdgeCountTotal;
      
      /// <summary>
      /// Depth is the distance from the initial node. Values are:
      ///   -1 means unknown
      ///    0 means the start node (the root node for a graph)
      ///    1 means a distance of 1 edge from the root
      ///    2 means a distance of 2 edges from the root and so on 
      /// Since the graph can have cycles, this value is really just an approximation, we can't rely on it being 100% accurate.
      /// </summary>
      public int Depth = -1;

      /// <summary>
      /// A generic value field. 
      /// Note this Value is not used in Btc implementation, see multiple value fields in CdmNodeBtc subclass instead.
      /// </summary>
      public float Value;
   }
}
