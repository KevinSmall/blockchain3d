using UnityEngine;
using RS = UnityEngine.RemoteSettings;

namespace UnityEngine.Analytics
{
	[AddComponentMenu("Analytics/RemoteSettings")]
	public class RemoteSettings : MonoBehaviour {
	    [SerializeField]
	    private DriveableProperty m_DriveableProperty = new DriveableProperty();
	    internal DriveableProperty DP
	    {
	        get { return m_DriveableProperty; }
	        set { m_DriveableProperty = value; }
	    }

	    void Start()
	    {
			RemoteSettingsUpdated();
	        // Add this class's updated settings handler to the RemoteSettings.Updated event.
	        RS.Updated += RemoteSettingsUpdated;
	    }

	    void RemoteSettingsUpdated()
		{
			for (int i = 0; i < m_DriveableProperty.fields.Count; i++)
			{
				var f = m_DriveableProperty.fields[i];
				if (!string.IsNullOrEmpty(f.rsKeyName) && RS.HasKey(f.rsKeyName) && f.target != null && !string.IsNullOrEmpty(f.fieldPath))
				{
					//Type doesn't work with a switch, so let's do it this way
					if (f.type == "bool")
					{
						f.SetValue(RS.GetBool(f.rsKeyName));
					}
					else if (f.type == "float")
					{
						f.SetValue(RS.GetFloat(f.rsKeyName));
					}
					else if (f.type == "int")
					{
						f.SetValue(RS.GetInt(f.rsKeyName));
					}
					else if (f.type == "string")
					{
						f.SetValue(RS.GetString(f.rsKeyName));
					}
				}
			}
		}
	}
}
