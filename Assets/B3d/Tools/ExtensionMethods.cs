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
using UnityEngine;

namespace B3d.Tools
{
   public static class ExtensionMethods
   {
      public static string Right(this string sValue, int iMaxLength)
      {
         //Check if the value is valid
         if (string.IsNullOrEmpty(sValue))
         {
            //Set valid empty string as string could be null
            sValue = string.Empty;
         }
         else if (sValue.Length > iMaxLength)
         {
            //Make the string no longer than the max length
            sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
         }
         return sValue;
      }

      public static T GetRandomEnumValue<T>(this T t)
      {
         var v = Enum.GetValues(typeof(T));
         return (T)v.GetValue(new System.Random().Next(v.Length));
      }

      public static Color ColorRgb(int r, int g, int b)
      {
         return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, 1f);
      }

      public static Color ColorRgba(int r, int g, int b, int a)
      {
         return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
      }

      public static void PlayerPrefsSetBool(string name, bool booleanValue)
      //public static void SetBool(this PlayerPrefs p, string name, bool booleanValue)
      // above doesnt work as extensio methods need object instance
      {
         PlayerPrefs.SetInt(name, booleanValue ? 1 : 0);
      }

      public static bool PlayerPrefsGetBool(string name)
      {
         return PlayerPrefs.GetInt(name) == 1 ? true : false;
      }
   }
}