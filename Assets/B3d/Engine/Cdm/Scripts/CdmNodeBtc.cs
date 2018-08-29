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

namespace B3d.Engine.Cdm
{
   /// <summary>
   /// Btc specific node attributes
   /// </summary>
   [Serializable]
   public class CdmNodeBtc : CdmNode
   {
      // Address specific
      public float FinalBalance;
      public float TotalReceived;
      public float TotalSent;

      // Transaction specific
      public DateTime CreateDate;
      public string CreateDateStr;
      public int BlockHeight;
      public string RelayedBy;
      public int VoutSize; // total output count
      public int VinSize;  // total input count
   }
}
