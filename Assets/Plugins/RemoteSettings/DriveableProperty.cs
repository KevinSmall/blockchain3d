using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEngine.Analytics
{
	[Serializable]
	public class DriveableProperty {
	    [Serializable]
	    public class FieldWithRemoteSettingsKey
	    {
	        [SerializeField]
	        private Object m_Target;
	        public Object target
	        {
	            get { return m_Target; }
	            set { m_Target = target; }
	        }

	        [SerializeField]
	        private string m_FieldPath;

	        public string fieldPath
	        {
	            get { return m_FieldPath; }
	            set { m_FieldPath = value; }
	        }

	        [SerializeField]
			private string m_RSKeyName;

	        public string rsKeyName
	        {
	            get { return m_RSKeyName; }
	            set { m_RSKeyName = rsKeyName; }
	        }

	        [SerializeField]
	        private string m_Type;

	        public string type
	        {
	            get { return m_Type; }
	            set { m_Type = type; }
	        }

	        public void SetValue (object val)
	        {
	            object target = m_Target;

	            foreach(var s in m_FieldPath.Split('.'))
	            {
	                try
	                {
	                    var temp = target.GetType().GetProperty(s);
	                    temp.SetValue(target, val, null);
	                }
	                catch
	                {
	                    var temp = target.GetType().GetField(s);
	                    temp.SetValue(target, val);
	                }
	            }
	        }

	        public Type GetTypeOfField()
	        {
	            object target = m_Target;
	            foreach (var s in m_FieldPath.Split('.'))
	            {
	                try
	                {
	                    var temp = target.GetType().GetProperty(s);
	                    target = temp.GetValue(target, null);
	                    //return typeof(temp.GetValue());
	                }
	                catch
	                {
	                    var temp = target.GetType().GetField(s);
	                    target = temp.GetValue(target);
	                }
	            }
	            return target.GetType();
	        }
	    }
	    [SerializeField]
	    private List<FieldWithRemoteSettingsKey> m_Fields;
	    public List<FieldWithRemoteSettingsKey> fields
	    {
	        get { return m_Fields; }
	        set { m_Fields = fields; }
	    }
	}
}