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

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Ethereum specific graph data. It is in this class the BTC specific overrides (eg for merging Edges) are handled.
   /// </summary>
   public class CdmGraphEth : CdmGraph
   {
      public override void AddEdge(CdmEdge e)
      {
         CdmEdgeEth ee = e as CdmEdgeEth;
         // Here is where you can add an edge to a Eth graph in an Eth specific way
         _edges.Add(ee);
      }
   }
}
