using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B3d.Tools
{
   public class InfoAttribute : PropertyAttribute
   {

#if UNITY_EDITOR
      public string Message;
      public MessageType Type;
      public bool MessageAfterProperty;

      public InfoAttribute(string message)
      {
         this.Message = message;
         this.Type = UnityEditor.MessageType.Info;
      }
#else
		public InfoAttribute(string message)
		{

		}
#endif
   }
}