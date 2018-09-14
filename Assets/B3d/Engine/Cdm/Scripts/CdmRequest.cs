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
   /// A CdmRequest is a request by a frontend for some graph data.
   /// It consists of a node, a certain number of edges from that node, plus all the nodes at the end of the edges 
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
   }
}
