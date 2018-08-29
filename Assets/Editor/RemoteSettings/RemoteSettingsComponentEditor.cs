using UnityEngine;
using UnityEngine.Analytics;
using UnityEditor;
using UnityEditor.Connect;
using UnityEditor.Analytics;
using RS = UnityEngine.RemoteSettings;

namespace UnityEngine.Analytics
{
	[CustomEditor(typeof(RS))]
	[CanEditMultipleObjects]
	internal class RemoteSettingsComponentEditor : Editor
	{
		SerializedProperty driveableProp;

		private static string k_Installed = "UnityAnalyticsRemoteSettingsInstallKey";
		private static string k_RSKeysExist = "UnityAnalyticsRemoteSettingsAreSet";

		private GUIContent m_AnalyticsNotEnabledHeaderContetn = new GUIContent("Unity Analytics is not enabled");
		private GUIContent m_AnalyticsNotEnabledContent = new GUIContent("To use Unity Remote Settings, please enable Unity Analytics from the Services window. Go to Window > Services to open Unity Services Window and follow the prompts.");
		private GUIContent m_NoKeysHeaderContent = new GUIContent("Unity Remote Settings have not been pulled");
		private GUIContent m_NoKeysContent = new GUIContent("To start using Unity Remote Settings, please make sure to go to Window > Unity Analytics > Remote Settings and click on “Refresh” to pull the latest Remote Settings from the server.");

		private void OnEnable()
		{
			driveableProp = serializedObject.FindProperty("m_DriveableProperty");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!AnalyticsSettings.enabled)
			{
				GUILayout.Label(m_AnalyticsNotEnabledHeaderContetn, EditorStyles.boldLabel);
				GUILayout.Label(m_AnalyticsNotEnabledContent, EditorStyles.wordWrappedLabel);
			}
			else if (EditorPrefs.GetBool(k_Installed + Application.cloudProjectId) && !EditorPrefs.GetBool(k_RSKeysExist + Application.cloudProjectId))
			{
				GUILayout.Label(m_NoKeysHeaderContent, EditorStyles.boldLabel);
				GUILayout.Label(m_NoKeysContent, EditorStyles.wordWrappedLabel);
			}
			else
			{
				EditorGUILayout.PropertyField(driveableProp);
				serializedObject.ApplyModifiedProperties();
			}

		}
	}
}
