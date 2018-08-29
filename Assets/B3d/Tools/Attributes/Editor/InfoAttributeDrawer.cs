#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace B3d.Tools
{
   [CustomPropertyDrawer(typeof(InfoAttribute))]
   /// <summary>
   /// Display message before a property
   /// </summary>
   public class InfoAttributeDrawer : PropertyDrawer
   {
      private int iconWidth = 60;
      private int textSpaceBefore = 4;
      private int textSpaceAfter = 9;

      InfoAttribute InfoAttribute { get { return ((InfoAttribute)attribute); } }

      private float CalculateTextboxHeight(string str)
      {
         GUIStyle style = new GUIStyle(EditorStyles.helpBox);
         style.richText = true;

         float newHeight = style.CalcHeight(new GUIContent(str), EditorGUIUtility.currentViewWidth - iconWidth);
         return newHeight;
      }

      /// <summary>
      /// OnGUI, displays the property and the textbox
      /// </summary>
      public override void OnGUI(Rect rectangle, SerializedProperty prop, GUIContent label)
      {
         if (IsHelpEnabled())
         {
            EditorStyles.helpBox.richText = true;
            Rect posInfo = rectangle;
            Rect posText = rectangle;


            posInfo.height = CalculateTextboxHeight(InfoAttribute.Message);

            posText.y += posInfo.height + textSpaceBefore;
            posText.height = GetPropertyHeight(prop, label);

            EditorGUI.HelpBox(posInfo, InfoAttribute.Message, InfoAttribute.Type);
            EditorGUI.PropertyField(posText, prop, label, true);
         }
         else
         {
            // If switchd off
            Rect textFieldPosition = rectangle;
            textFieldPosition.height = GetPropertyHeight(prop, label);
            EditorGUI.PropertyField(textFieldPosition, prop, label, true);
         }
      }

      /// <summary>
      /// Returns the height of property + help text
      /// </summary>
      public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
      {
         if (IsHelpEnabled())
         {
            return EditorGUI.GetPropertyHeight(prop) + CalculateTextboxHeight(InfoAttribute.Message) + textSpaceAfter + textSpaceBefore;
         }
         else
         {
            return EditorGUI.GetPropertyHeight(prop);
         }
      }

      /// <summary>
      /// Checks the editor prefs to see if help is enabled or not
      /// </summary>
      /// <returns><c>true</c>, if enabled was helped, <c>false</c> otherwise.</returns>
      private bool IsHelpEnabled()
      {
         // TODO could perhaps switch off info boxes here, according to editor prefs
         return true;
      }
   }
}

#endif