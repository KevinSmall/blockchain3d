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

namespace B3d.Engine.FrontEnd
{
   /// <summary>
   /// Interface defining a graph factory. The graph factory will receive calls to create or update
   /// nodes or edges.
   /// </summary>
   public interface IGraphFactory
   {
      void CreateOrUpdateNode(CdmNode nodeNew);
      void CreateOrUpdateEdge(CdmEdge edgeNew);
   }
}