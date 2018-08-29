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
using System;

namespace B3d.Engine.Adaptors
{
   /// <summary>
   /// Generic Adaptor definition
   /// All Adaptors (regardless of their family or data provider) must implement this interface
   /// </summary>
   public interface IAdaptor
   {
      /// <summary>
      /// Returns the adaptor family (Btc, Eth, Twitter etc)
      /// </summary>
      Family GetFamily();

      /// <summary>
      /// Check to see if a connection to the adaptor is going to be possible, this may be as simple as checking for internet access
      /// </summary>
      void CheckConnectionPossible(Action callbackOnSuccess, Action callbackOnFail);

      /// <summary>
      /// Check existence of a node, specifying a nodeType
      /// The nodeId is passed back as a string in the callbacks
      /// </summary>
      void CheckNodeExists(string nodeId, NodeType nodeType, Action<string> callbackOnSuccess, Action<string> callbackOnFail);

      /// <summary>
      /// Get a graph fragment: the node requested, the edges requested, and the nodes at the end of each edge, and push them to Cdm
      /// Success means data pushed to Cdm and it will be raising events, the callback on fail is not essential, if 
      /// anything fails then just nothing new will appear in the Cdm
      /// </summary>
      void GetGraphFragment(CdmRequest r, Action<string> callbackOnFail);
   }

}