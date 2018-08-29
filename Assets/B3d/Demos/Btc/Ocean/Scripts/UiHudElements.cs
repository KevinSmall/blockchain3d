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

using UnityEngine.UI;

namespace B3d.Demos
{
   /// <summary>
   /// BitHudElements is the interface point  between all the flavours of UI/HUD screens and
   /// the C# code to populate.  The C# code to populate values doesnt know or care what HUD 
   /// is implmented.  HUDs must implement all values even if not used (just make not visible)
   /// so code doesnt need to bother with null checks. 
   /// </summary>
   public struct UiHudElements
      {
         public Text AddrOrTxLabel;
         public Text AddrOrTxValue;
         public Text ShortLabel;
         public Text ShortValue;
         public Text BlockLabel;
         public Text BlockValue;
         public Text LinkedAddrOrTxLabel;
         public Text LinkedAddrOrTxValue;
         public Text mBtcLabel;
         public Text mBtcValue;
         public Text DateLabel;
         public Text DateValue;
         public Text InfoLabel;
      }
}
