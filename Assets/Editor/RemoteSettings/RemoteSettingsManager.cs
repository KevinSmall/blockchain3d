using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace UnityEngine.Analytics
{
	public class RemoteSettingsManager : EditorWindow
	{
		private readonly static string k_PluginVersion = "0.1.5-beta";

		private readonly static string k_FetchKey = "UnityAnalyticsRemoteSettingsFetchKey";
		private readonly static string k_Installed = "UnityAnalyticsRemoteSettingsInstallKey";
		private readonly static string k_RSKeysExist = "UnityAnalyticsRemoteSettingsAreSet";
		private readonly static string k_CurrentEnvironment = "UnityAnalyticsRemoteSettingsEnvironment";

		private string m_AppId = "";
		private string m_SecretKey = "";
		private string m_RSId = "";
		private RemoteSettingsHolder RSDataStore;
		private string m_CurrentEnvironment = "Release";
		private List<string> m_EnvironmentNames = new List<string>();

		//GUI variables
		private const string m_TabTitle = "Remote Settings";
		private const string m_ServerErrorDialogTitle = "Can't get Remote Settings";
		private const string m_ServerErrorDialogBtnLabel = "OK";
		private const string m_NoRSKeysError = "No RemoteSettings keys have been found, please go to the dashboard to add them.";

		//Labels
		private GUIContent m_SecretKeyContent = new GUIContent("Project Secret Key", "Copy the key from the 'Configure' page of your project dashboard");
		private GUIContent m_RemoteSettingsHeaderContent = new GUIContent("Unity Remote Settings");
		private GUIContent m_RemoteSettingsIntroContent = new GUIContent("Unity Remote Settings enables to make the game appearance and properties of your app without publishing an app update");
		private GUIContent m_SecretKeyPrefixContent = new GUIContent("Please enter the Project Secret Key Key to authenticate your project.");
		private GUIContent m_GridKeyContent = new GUIContent("Key");
		private GUIContent m_GridTypeContent = new GUIContent("Type");
		private GUIContent m_GridValueContent = new GUIContent("Value");
		private GUIContent m_RemoteSettingsSetupContent = new GUIContent("To start setting up key-value pairs for the Remote Settings");
		private GUIContent m_RefreshKeysContent = new GUIContent("If you already have key-value pairs in your dashboard");
		private GUIContent m_AnalyticsNotEnabledHeaderContent = new GUIContent("Unity Analytics is not enabled");
		private GUIContent m_AnalyticsNotEnabledContent = new GUIContent("To use Unity Remote Settings, please enable Unity Analytics from the Services window. Go to Window > Services to open Unity Services Window and follow the prompts.");

		//Link Labels
		private GUIContent m_LearnMoreLinkContent = new GUIContent("Learn more");
		private GUIContent m_SecretKeyLinkContent = new GUIContent("Look up the key.");

		//Button Labels
		private GUIContent m_NextButtonContent = new GUIContent("Next");
		private GUIContent m_GoToDashboardButtonContent = new GUIContent("Go To Dashboard");
		private GUIContent m_RefreshButtonContent = new GUIContent("Refresh");

		private Vector2 m_RemoteSettingsListScrollPos;

		private const float m_HeaderSpace = 10f;
		private const float m_AfterParagraphSpace = 20f;
		private const float m_ColumnCount = 3f;

		private Color m_LinkColor = new Color(0f, 188f, 212f, 100f);
		private Color m_DefaultColor = Color.white;

		//File path vars
		private const string m_PathUnity = "Unity";
		private const string m_PathAnalytics = "Analytics";
		private const string m_PathConfig = "config";
		private const string m_DataStoreName = "RemoteSettingsDataStore";
		private const string m_PathToDataStore = "Assets/Editor/RemoteSettings/Data/{0}.asset";
		private const string k_RemoteSettingsDataPath = "Assets/Editor/RemoteSettings/Data";

		//Web variables
		//REST API paths
		//private const string BasePath = "https://cloud-staging.uca.cloud.unity3d.com/";
		private const string BasePath = "https://analytics.cloud.unity3d.com/";
		private const string APIPath = BasePath + "api/v2/projects/";
		private const string ConfigurationPath = APIPath + "{0}/configurations/";
		private const string RemoteSettingsPath = APIPath + "{0}/configurations/{1}/remotesettings";

		//Link URLs
		private const string m_DocumentationURL = "https://github.com/UnityTech/RemoteConfigEditor";
		private const string m_SecretKeyURL = BasePath + "projects/{0}/edit/";
		private const string m_DashboardURL = BasePath + "remote_settings/{0}/";

		[MenuItem("Window/Unity Analytics/Remote Settings")]
		static void RemoteSettingsManagerMenuOption()
		{
			EditorWindow.GetWindow(typeof(RemoteSettingsManager), false, m_TabTitle);
		}

		private void OnEnable()
		{
            EditorApplication.playmodeStateChanged += EditorApplication_PlaymodeStateChanged;
			if (UnityEditor.Analytics.AnalyticsSettings.enabled)
			{
				if (EditorPrefs.GetBool(k_Installed + m_AppId))
				{
					CheckAndCreateDataStore();
					SubmitRequest();
				}
			}
            //CheckAndCreateDataStore();
            //CreateDataStoreDict();
        }

        private void OnDisable()
		{
			EditorApplication.playmodeStateChanged -= EditorApplication_PlaymodeStateChanged;
		}

		void OnFocus()
		{
			if (EditorPrefs.GetBool(k_Installed + m_AppId, false))
			{
				RestoreValues();
			}
			else
			{
				SetInitValues();
			}
			if (RSDataStore == null)
			{
				CheckAndCreateDataStore();
			}
		}

		void EditorApplication_PlaymodeStateChanged()
		{
			if (EditorApplication.isPlaying)
			{
				CreateDataStoreDict();
				this.Repaint();
			}
		}

		private void OnGUI()
		{
			if (UnityEditor.Analytics.AnalyticsSettings.enabled)
			{
				if (!EditorPrefs.GetBool(k_Installed + m_AppId))
				{
					AddHeader();
					RestoreAppId();
					GUILayout.Label(m_SecretKeyPrefixContent);
					GUI.contentColor = m_LinkColor;
					if (GUILayout.Button(m_SecretKeyLinkContent, GUI.skin.label))
					{
						Application.OpenURL(string.Format(m_SecretKeyURL, m_AppId));
					}
					GUI.contentColor = m_DefaultColor;
					string oldKey = m_SecretKey;
					m_SecretKey = EditorGUILayout.TextField(m_SecretKeyContent, m_SecretKey);
					if (oldKey != m_SecretKey && !string.IsNullOrEmpty(m_SecretKey))
					{
						EditorPrefs.SetString(k_FetchKey + m_AppId, m_SecretKey);
					}
					GUI.enabled = !string.IsNullOrEmpty(m_SecretKey);
					GUILayout.Space(m_AfterParagraphSpace);
					if (GUILayout.Button(m_NextButtonContent))
					{
						CheckAndCreateDataStore();
						SubmitRequest();
						if (GUI.changed)
						{
							EditorUtility.SetDirty(RSDataStore);
						}
					}
					GUI.enabled = true;
				}
				else if (EditorPrefs.GetBool(k_Installed + m_AppId) && EditorPrefs.GetBool(k_RSKeysExist + m_AppId))
				{
					EditorGUI.BeginDisabledGroup(m_EnvironmentNames.Count <= 1);
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label("Configuration");
						GUIContent ddBtnContent = new GUIContent(m_CurrentEnvironment);
						Rect rect = GUILayoutUtility.GetRect(ddBtnContent, EditorStyles.popup);
						if (GUI.Button(rect, ddBtnContent, EditorStyles.popup))
						{
							BuildPopupListForRSEnvironments().DropDown(rect);
						}
					}
					EditorGUI.EndDisabledGroup();

					float columnWidth = EditorGUIUtility.currentViewWidth / m_ColumnCount;
					using (new GUILayout.VerticalScope("box"))
					{
						using (new EditorGUILayout.HorizontalScope())
						{
                            using (new GUILayout.HorizontalScope("box"))
							{
								GUILayout.Label(m_GridKeyContent, EditorStyles.boldLabel, GUILayout.Width(columnWidth));
								GUILayout.Label(m_GridTypeContent, EditorStyles.boldLabel, GUILayout.Width(columnWidth));
								GUILayout.Label(m_GridValueContent, EditorStyles.boldLabel);
							}
						}
						using (new GUILayout.VerticalScope("box"))
						{
							if (RSDataStore.rsKeys != null)
							{
								m_RemoteSettingsListScrollPos = EditorGUILayout.BeginScrollView(m_RemoteSettingsListScrollPos);
								foreach (KeyValuePair<string, RemoteSettingsKeyValueType> rsPair in RSDataStore.rsKeys)
								{
                                    // background = RSDataStore.rsKeys.Count % 2 == 0 ? new GUIStyle ("OL EntryBackEven") : new GUIStyle ("OL EntryBackOdd");
                                    using (new EditorGUILayout.HorizontalScope())
									{
										GUILayout.Label(rsPair.Key, GUILayout.Width(columnWidth));
										GUILayout.Label(rsPair.Value.type, GUILayout.Width(columnWidth));
										GUILayout.Label(rsPair.Value.value, EditorStyles.wordWrappedLabel);
									}
								}
								EditorGUILayout.EndScrollView();
							}
						}
					}
					AddFooterButtons();
				}
				else if (EditorPrefs.GetBool(k_Installed + m_AppId) && !EditorPrefs.GetBool(k_RSKeysExist + m_AppId))
				{
					AddHeader();
					GUILayout.Label(m_RemoteSettingsSetupContent);
					if (GUILayout.Button(m_GoToDashboardButtonContent, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
					{
						Application.OpenURL(string.Format(m_DashboardURL, m_AppId));
					}
					GUILayout.Space(m_AfterParagraphSpace);
					GUILayout.Label(m_RefreshKeysContent);
					if (GUILayout.Button(m_RefreshButtonContent, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
					{
						SubmitRequest();
					}
				}
			}
			else
			{
				AddHeader();
				GUILayout.Label(m_AnalyticsNotEnabledHeaderContent, EditorStyles.boldLabel);
				GUILayout.Label(m_AnalyticsNotEnabledContent, EditorStyles.wordWrappedLabel);
			}
			if (GUI.changed)
			{
				EditorUtility.SetDirty(RSDataStore);
			}
		}

		private GenericMenu BuildPopupListForRSEnvironments()
		{
			var menu = new GenericMenu();

			for (int i = 0; i < m_EnvironmentNames.Count; i++)
			{
				menu.AddItem(new GUIContent(m_EnvironmentNames[i]), m_EnvironmentNames[i] == m_CurrentEnvironment, EnvironmentSelectionCallback, m_EnvironmentNames[i]);
			}

			return menu;
		}

		private void EnvironmentSelectionCallback(object obj)
		{
			m_CurrentEnvironment = (string)obj;
            EditorPrefs.SetString(k_CurrentEnvironment + m_AppId, m_CurrentEnvironment);
			SubmitRequest();
		}

		private void AddHeader()
		{
			GUILayout.Space(m_HeaderSpace);
			GUILayout.Label(m_RemoteSettingsHeaderContent, EditorStyles.boldLabel);
			GUILayout.Label(m_RemoteSettingsIntroContent, EditorStyles.wordWrappedLabel);
			GUI.contentColor = m_LinkColor;
			if (GUILayout.Button(m_LearnMoreLinkContent, GUI.skin.label))
			{
				Application.OpenURL(m_DocumentationURL);
			}
			GUI.contentColor = m_DefaultColor;
			GUILayout.Space(m_AfterParagraphSpace);
		}

		private void AddFooterButtons() 
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button(m_RefreshButtonContent, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
				{
					string filePath = System.IO.Path.Combine(Application.persistentDataPath, m_PathUnity);
					filePath = System.IO.Path.Combine(filePath, m_AppId);
					filePath = System.IO.Path.Combine(filePath, m_PathAnalytics);
					filePath = System.IO.Path.Combine(filePath, m_PathConfig);
					FileUtil.DeleteFileOrDirectory (filePath);
					SubmitRequest();
				}
				if (GUILayout.Button(m_GoToDashboardButtonContent))
				{
					Application.OpenURL(string.Format(m_DashboardURL, m_AppId));
				}
			}
		}

		private void CheckAndCreateAssetFolder(string path)
        {
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            string assetPath = null;
            foreach (string folder in folders)
            {
                if (assetPath == null)
                    assetPath = folder;
                else
                {
                    string folderPath = System.IO.Path.Combine(assetPath, folder);
                    if (!Directory.Exists(folderPath))
                        AssetDatabase.CreateFolder(assetPath, folder);
                    assetPath = folderPath;
                }
            }
        }

		private void CheckAndCreateDataStore()
		{
			string formattedPath = string.Format(m_PathToDataStore, m_DataStoreName);
			if (AssetDatabase.FindAssets(m_DataStoreName).Length == 0)
			{
				RemoteSettingsHolder asset = ScriptableObject.CreateInstance<RemoteSettingsHolder>();
				asset.rsKeyList = new List<RemoteSettingsKeyValueType>();
				CheckAndCreateAssetFolder(k_RemoteSettingsDataPath);
				AssetDatabase.CreateAsset(asset, formattedPath);
				AssetDatabase.SaveAssets();
				RSDataStore = AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteSettingsHolder)) as RemoteSettingsHolder;
			}
			else
			{
				RSDataStore = AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteSettingsHolder)) as RemoteSettingsHolder;
			}
		}

		private void CreateDataStoreDict()
		{
			if (RSDataStore.rsKeyList.Count != 0)
			{
				if (RSDataStore.rsKeys == null)
				{
					RSDataStore.rsKeys = new Dictionary<string, RemoteSettingsKeyValueType>();
				}
				else
				{
					RSDataStore.rsKeys.Clear();
				}
				foreach (RemoteSettingsKeyValueType rsPair in RSDataStore.rsKeyList)
				{
					RSDataStore.rsKeys.Add(rsPair.key, rsPair);
				}
			}
		}

		protected void RestoreAppId()
		{
#if UNITY_5_3_OR_NEWER
			if (string.IsNullOrEmpty(m_AppId) && !string.IsNullOrEmpty(Application.cloudProjectId) || !m_AppId.Equals(Application.cloudProjectId))
			{
				m_AppId = Application.cloudProjectId;
			}
#endif
		}

		protected void SetInitValues()
		{
			RestoreAppId();
			if (!string.IsNullOrEmpty(EditorPrefs.GetString(k_FetchKey + m_AppId)))
			{
				m_SecretKey = EditorPrefs.GetString(k_FetchKey + m_AppId, m_SecretKey);
			}
		}

		protected void RestoreValues()
		{
			RestoreAppId();

			m_SecretKey = EditorPrefs.GetString(k_FetchKey + m_AppId, m_SecretKey);

            m_CurrentEnvironment = EditorPrefs.GetString (k_CurrentEnvironment + m_AppId, m_CurrentEnvironment);
		}

		private void SubmitRequest()
		{
			using (WebClient client = new WebClient())
			{
				client.Encoding = System.Text.Encoding.UTF8;
				Authorization(client);
				string url = string.Format(ConfigurationPath, m_AppId);
				string result = "";
				try
				{
					result = client.DownloadString(new Uri(url));
				}
				catch (WebException ex)
				{
					EditorUtility.DisplayDialog(m_ServerErrorDialogTitle, ex.Message, m_ServerErrorDialogBtnLabel);
                    EditorPrefs.SetBool(k_Installed + m_AppId, false);
                    return;
				}

				EditorPrefs.SetBool(k_Installed + m_AppId, true);
				m_EnvironmentNames.Clear();
				result = "{ \"array\": " + result + "}";
				var dict = MiniJSON.Json.Deserialize(result) as Dictionary<string, object>;
				var list = (dict["array"]) as List<object>;
				for (int i = 0; i < list.Count; i++)
				{
					var valueDict = list[i] as Dictionary<string, object>;
					if (!m_EnvironmentNames.Contains(valueDict["name"].ToString()))
					{
						m_EnvironmentNames.Add(valueDict["name"].ToString());
					}
					if (valueDict["name"].ToString() == m_CurrentEnvironment)
					{
						m_RSId = valueDict["id"].ToString();
					}
				}

				if (string.IsNullOrEmpty(m_RSId))
				{
                    if (m_EnvironmentNames.Count > 0) 
                    {
                        var valueDict = list [0] as Dictionary<string, object>;
                        EnvironmentSelectionCallback (valueDict ["name"]);
                        m_RSId = valueDict ["id"].ToString ();
                    }
                    else 
                    {
                        EditorPrefs.SetBool (k_RSKeysExist + m_AppId, false);
                        EditorUtility.DisplayDialog (m_ServerErrorDialogTitle, m_NoRSKeysError, m_ServerErrorDialogBtnLabel);
                        return;
                    }
				}
				url = string.Format(RemoteSettingsPath, m_AppId, m_RSId);
				try
				{
					result = client.DownloadString(new Uri(url));
					result = "{ \"array\": " + result + "}";
					dict = MiniJSON.Json.Deserialize(result) as Dictionary<string, object>;
					list = (dict["array"]) as List<object>;
					RSDataStore.rsKeyList.Clear();
					if (list.Count == 0)
					{
						EditorPrefs.SetBool(k_RSKeysExist + m_AppId, false);
					}
					else
					{
						for (int i = 0; i < list.Count; i++)
						{
							var valueDict = list[i] as Dictionary<string, object>;
							RSDataStore.rsKeyList.Add(new RemoteSettingsKeyValueType(valueDict["key"].ToString(), valueDict["value"].ToString(), valueDict["type"].ToString()));
						}
						EditorPrefs.SetBool(k_RSKeysExist + m_AppId, true);
					}
					CreateDataStoreDict();
				}
				catch (WebException ex)
				{
					EditorUtility.DisplayDialog(m_ServerErrorDialogTitle, ex.Message, m_ServerErrorDialogBtnLabel);
				}
			}
		}

		protected void Authorization(WebClient client)
		{
#if UNITY_EDITOR_WIN
        // Bypassing SSL security in Windows to work around a CURL bug.
        // This is insecure and should be fixed when the Engine supports SSL.
        ServicePointManager.ServerCertificateValidationCallback = delegate (System.Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
            return true;
        };
#endif
			string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(m_AppId + ":" + m_SecretKey));
			client.Headers.Add("Content-Type", "application/json");
			client.Headers.Add(HttpRequestHeader.UserAgent, "Unity Editor " + Application.unityVersion + " RS " + k_PluginVersion);
			client.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", credentials));
		}
	}

	[System.Serializable]
	public struct RemoteSettingsKeyValueType
	{
		public string key;
		public string value;
		public string type;

		public RemoteSettingsKeyValueType(string k, string v, string t)
		{
			key = k;
			value = v;
			type = t;
		}
	}
}
