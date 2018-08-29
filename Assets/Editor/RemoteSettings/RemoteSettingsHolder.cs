using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics
{
	public class RemoteSettingsHolder : ScriptableObject
	{
		public Dictionary<string, RemoteSettingsKeyValueType> rsKeys;
		public List<RemoteSettingsKeyValueType> rsKeyList;
	}

}
