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
   /// Stateless helpers for Cdm 
   /// </summary>
   class CdmHelpers
   {
      /// <summary>
      /// Translate page to a fromValue, toValue pair for the given pageSize. For example with pageSize = 8:
      ///   Page 1 = 1 - 8, page 2 = 9 - 16
      ///   Page <= 0 returns from-to of 0-0
      /// </summary>
      public static void GetFromToForPage(int pageSize, int pageNumber, out int fromValue, out int toValue)
      {
         if (pageNumber <= 0)
         {
            fromValue = 0;
            toValue = 0;
         }
         else
         {
            fromValue = ((pageNumber - 1) * pageSize) + 1;
            toValue = pageNumber * pageSize;
         }
      }
   }
}