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
      public string NodeId;
      public NodeType NodeType;
      public bool IsSentToView;
      public int NodeEdgeCountTotal;
      /// <summary>
      /// Note this value is not used in Btc implementation, see values in CdmNodeBtc subclass instead
      /// </summary>
      public float Value;
   }
}
