using System;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine.Analytics;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UnityEngine.Analytics
{
	[CustomPropertyDrawer(typeof(DriveableProperty), true)]
	public class DriveablePropertyDrawer : PropertyDrawer
	{
		protected class State
		{
			internal ReorderableList m_ReorderableList;
			public int lastSelectedIndex;
		}
		private SerializedProperty m_FieldsArray;
		private ReorderableList m_ReorderableList;
		private int m_LastSelectedIndex;

		private const int m_ExtraSpacing = 9;
		private const int m_HeaderHeight = 16;

		private Dictionary<string, State> m_States = new Dictionary<string, State>();

		private GUIContent m_NoFieldContent = new GUIContent("No Field");
		private GUIContent m_ParametersHeaderContent = new GUIContent("Parameters");
		private GUIContent m_RemoteSettingKeyLabelContent = new GUIContent("Remote Setting Key");

		private const string m_DataStoreName = "RemoteSettingsDataStore";
		private const string m_PathToDataStore = "Assets/Editor/RemoteSettings/Data/{0}.asset";

		private const string m_FieldsString = "m_Fields";
		private const string m_TargetString = "m_Target";
		private const string m_FieldPathString = "m_FieldPath";
		private const string m_TypeString = "m_Type";
		private const string m_RSKeyNameString = "m_RSKeyName";
		private const string m_Boolean = "boolean";
		private const string m_Bool = "bool";
		private const string m_Float = "float";
		private const string m_Int = "int";
		private const string m_String = "string";

		private RemoteSettingsHolder RSDataStore;

		private State GetState(SerializedProperty prop)
		{
			State state;
			string key = prop.propertyPath;
			m_States.TryGetValue(key, out state);
			if (state == null)
			{
				state = new State();
				SerializedProperty fieldsArray = prop.FindPropertyRelative(m_FieldsString);
				state.m_ReorderableList = new ReorderableList(prop.serializedObject, fieldsArray, false, true, true, true);
				state.m_ReorderableList.drawHeaderCallback = DrawHeader;
				state.m_ReorderableList.drawElementCallback = DrawParam;
				state.m_ReorderableList.onSelectCallback = SelectParam;
				state.m_ReorderableList.onReorderCallback = EndDragChild;
				state.m_ReorderableList.onAddCallback = AddParam;
				state.m_ReorderableList.onRemoveCallback = RemoveButton;
				// Two standard lines with standard spacing between and extra spacing below to better separate items visually.
				state.m_ReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + m_ExtraSpacing;
				state.m_ReorderableList.index = 0;

				m_States[key] = state;
			}
			return state;
		}

		private State RestoreState(SerializedProperty prop)
		{
			State state = GetState(prop);
			m_FieldsArray = state.m_ReorderableList.serializedProperty;
			m_ReorderableList = state.m_ReorderableList;
			m_LastSelectedIndex = state.lastSelectedIndex;

			return state;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (RSDataStore == null)
			{
				RSDataStore = AssetDatabase.LoadAssetAtPath(string.Format(m_PathToDataStore, m_DataStoreName), typeof(RemoteSettingsHolder)) as RemoteSettingsHolder;
			}
			State state = RestoreState(property);

			OnGUI(position);

			state.lastSelectedIndex = m_LastSelectedIndex;
		}

		void OnGUI(Rect position)
		{
			if (m_FieldsArray == null || !m_FieldsArray.isArray)
			{
				return;
			}
			if (m_ReorderableList != null)
			{
				var oldIdentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				m_ReorderableList.DoList(position);
				EditorGUI.indentLevel = oldIdentLevel;
			}
		}

		protected virtual void DrawHeader(Rect headerRect)
		{
			headerRect.height = m_HeaderHeight;
			GUI.Label(headerRect, m_ParametersHeaderContent);
		}

		Rect[] GetRowRects(Rect rect)
		{
			Rect[] rects = new Rect[3];

			rect.height = EditorGUIUtility.singleLineHeight;
			rect.y += 2;

			Rect targetRect = rect;
			targetRect.width *= 0.5f;

			Rect keyRect = rect;
			keyRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			Rect propRect = rect;
			propRect.xMin = targetRect.xMax + EditorGUIUtility.standardVerticalSpacing;

			rects[0] = targetRect;
			rects[1] = keyRect;
			rects[2] = propRect;
			return rects;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			RestoreState(property);

			float height = 0f;
			if (m_ReorderableList != null)
			{
				height = m_ReorderableList.GetHeight();
			}
			return height;
		}

		void DrawParam(Rect rect, int index, bool isactive, bool isfocused)
		{
			var field = m_FieldsArray.GetArrayElementAtIndex(index);

			rect.y++;

			Rect[] subRects = GetRowRects(rect);
			Rect targetRect = subRects[0];
			Rect keyRect = subRects[1];
			Rect propRect = subRects[2];

			var fieldTarget = field.FindPropertyRelative(m_TargetString);
			var fieldName = field.FindPropertyRelative(m_FieldPathString);
			var keyName = field.FindPropertyRelative(m_RSKeyNameString);

			EditorGUI.BeginChangeCheck();
			{
				GUI.Box(targetRect, GUIContent.none);
				EditorGUI.PropertyField(targetRect, fieldTarget, GUIContent.none);
				if (EditorGUI.EndChangeCheck())
					fieldName.stringValue = null;
			}

			EditorGUI.BeginDisabledGroup(fieldTarget.objectReferenceValue == null);

			EditorGUI.BeginProperty(propRect, GUIContent.none, fieldName);
			{
				GUIContent buttonContent;
				var buttonLabel = new StringBuilder();
				if (string.IsNullOrEmpty(fieldName.stringValue) || fieldTarget.objectReferenceValue == null)
				{
					buttonLabel.Append("No field");
				}
				else
				{
					buttonLabel.Append(fieldTarget.objectReferenceValue.GetType().Name);
					if (!string.IsNullOrEmpty(fieldName.stringValue))
					{
						buttonLabel.Append(".");
						buttonLabel.Append(fieldName.stringValue);
					}
				}
				buttonContent = new GUIContent(buttonLabel.ToString());

				if (GUI.Button(propRect, buttonContent, EditorStyles.popup))
				{
					BuildPopupListForField(fieldTarget.objectReferenceValue, field).DropDown(propRect);
				}
			}

			EditorGUI.EndProperty();
			EditorGUI.EndDisabledGroup();

			var label = new GUIContent(m_RemoteSettingKeyLabelContent);

			EditorGUI.BeginProperty(keyRect, label, keyName);
			{
				var newKeyRect = EditorGUI.PrefixLabel(keyRect, label);
				GUIContent buttonContent;
				var buttonLabel = new StringBuilder();
				if (string.IsNullOrEmpty(keyName.stringValue))
				{
					buttonLabel.Append("No Field");
				}
				else if (RSDataStore.rsKeys == null || !RSDataStore.rsKeys.ContainsKey(keyName.stringValue))
				{
					buttonLabel.Append("Key no longer exists");
				}
				else
				{
					buttonLabel.Append(keyName.stringValue);
				}
				buttonContent = new GUIContent(buttonLabel.ToString());
				//if(EditorGUI.DropdownButton(newKeyRect, buttonContent, FocusType.Keyboard))
				if (GUI.Button(newKeyRect, buttonContent, EditorStyles.popup))
				{
					BuildPopupListForRSKeys(field).DropDown(newKeyRect);
				}

				EditorGUI.EndProperty();
			}

		}

		public GenericMenu BuildPopupListForRSKeys(SerializedProperty field)
		{
			var keyName = field.FindPropertyRelative(m_RSKeyNameString).stringValue;

			var menu = new GenericMenu();

			menu.AddItem(m_NoFieldContent,
				string.IsNullOrEmpty(keyName),
						 SetRSKey,
						 new RemoteKeySetter(field, null, null));

			menu.AddSeparator("");

			if (RSDataStore != null)
			{
				foreach (RemoteSettingsKeyValueType rsKeyVal in RSDataStore.rsKeyList)
				{
					var fieldType = field.FindPropertyRelative(m_TypeString).stringValue;
					if (fieldType == m_Boolean)
					{
						fieldType = m_Bool;
					}
					if ((!string.IsNullOrEmpty(fieldType) &&
						 fieldType == rsKeyVal.type) ||
						string.IsNullOrEmpty(fieldType) ||
						string.IsNullOrEmpty(field.FindPropertyRelative(m_FieldPathString).stringValue))
					{
						var activated = (keyName == rsKeyVal.key);
						menu.AddItem(new GUIContent(rsKeyVal.key),
							activated,
							SetRSKey,
							new RemoteKeySetter(field, rsKeyVal.key, rsKeyVal.type));
					}
				}
			}
			return menu;
		}

		public GenericMenu BuildPopupListForField(Object target, SerializedProperty field)
		{
			GameObject targetToUse;
			if (target is Component)
				targetToUse = ((Component)target).gameObject;
			else
				targetToUse = (GameObject)target;


			var fieldName = field.FindPropertyRelative(m_FieldPathString).stringValue;

			var menu = new GenericMenu();

			menu.AddItem(m_NoFieldContent,
						 string.IsNullOrEmpty(fieldName),
						 SetProperty,
						 new PropertySetter(field, target, null, null));

			if (targetToUse == null)
				return menu;

			menu.AddSeparator("");
			GeneratePopupForType(menu, targetToUse, targetToUse, field, "", 0);

			Component[] comps = targetToUse.GetComponents<Component>();

			foreach (Component comp in comps)
			{
				if (comp == null)
					continue;

				GeneratePopupForType(menu, comp, comp, field, "", 0);
			}
			return menu;
		}

		private void GeneratePopupForType(GenericMenu menu,
				Object originalTarget,
				object target,
				SerializedProperty fieldProp,
				String prefix,
				int depth)
		{
			var fields = Array.FindAll(target.GetType().GetMembers(),
					x => (x.GetType().Name == "MonoProperty" ||
						  x.GetType().Name == "MonoField"));


			foreach (var field in fields)
			{
				var path = "";
				if (prefix == "")
					path = field.Name;
				else
					path = String.Concat(prefix, "/", field.Name);

				Type myType = field.GetType();

				myType = (field.GetType().Name == "MonoField" ?
						  ((FieldInfo)field).FieldType :
						  ((PropertyInfo)field).PropertyType);


				if (myType.IsPrimitive || myType == typeof(string))
				{
					var fieldPath = path.Replace("/", ".");
					var activated = ((fieldProp.FindPropertyRelative(m_TargetString).objectReferenceValue
									  == originalTarget) &&
									 (fieldProp.FindPropertyRelative(m_FieldPathString).stringValue
									  == fieldPath));
					string typeStr = "";

					if (myType == typeof(bool))
					{
						typeStr = m_Bool;
					}
					else if (myType == typeof(float))
					{
						typeStr = m_Float;
					}
					else if (myType == typeof(int))
					{
						typeStr = m_Int;
					}
					else if (myType == typeof(string))
					{
						typeStr = m_String;
					}
					else
					{
						continue;
					}

					if ((!string.IsNullOrEmpty(fieldProp.FindPropertyRelative(m_TypeString).stringValue)
						&& fieldProp.FindPropertyRelative(m_TypeString).stringValue == typeStr)
					   || string.IsNullOrEmpty(fieldProp.FindPropertyRelative(m_TypeString).stringValue)
					   || string.IsNullOrEmpty(fieldProp.FindPropertyRelative(m_RSKeyNameString).stringValue))
					{
						menu.AddItem(new GUIContent(originalTarget.GetType().Name + "/" + path),
						activated,
						SetProperty,
						new PropertySetter(fieldProp,
							originalTarget,
							fieldPath, typeStr));
					}
				}
				else if (depth <= 1)
				{
					/* it must be a struct, and we can expand it */
					object temp;
					if ((field.Name == "mesh" && target.GetType().Name == "MeshFilter") ||
						((field.Name == "material" || field.Name == "materials")
						 && target is Renderer))
						continue;

					temp = GetValue(field, target);
					if (temp != null)
					{
						GeneratePopupForType(menu,
							originalTarget,
							(object)(temp),
							fieldProp,
							path,
							depth + 1);
					}
				}
				/* ignore structs at depth > 1, because we can't expand forever and
	             * we don't want to send structs as strings
	             */
			}
		}

		void SelectParam(ReorderableList list)
		{
			m_LastSelectedIndex = list.index;
		}

		void EndDragChild(ReorderableList list)
		{
			m_LastSelectedIndex = list.index;
		}

		void RemoveButton(ReorderableList list)
		{
			ReorderableList.defaultBehaviours.DoRemoveButton(list);
			m_LastSelectedIndex = list.index;

			list.displayAdd = true;
		}

		private void AddParam(ReorderableList list)
		{
			ReorderableList.defaultBehaviours.DoAddButton(list);
			m_LastSelectedIndex = list.index;
			var field = m_FieldsArray.GetArrayElementAtIndex(list.index);

			var target = field.FindPropertyRelative(m_TargetString);
			var fieldPath = field.FindPropertyRelative(m_FieldPathString);
			var rsKeyName = field.FindPropertyRelative(m_RSKeyNameString);

			if (list.index == 0)
			{
				target.objectReferenceValue = null;
			}
			else
			{
				var prev = m_FieldsArray.GetArrayElementAtIndex(list.index - 1);
				target.objectReferenceValue =
					prev.FindPropertyRelative(m_TargetString).objectReferenceValue;
			}
			fieldPath.stringValue = null;
			rsKeyName.stringValue = null;


			list.displayAdd = true;
		}

		public static object GetValue(MemberInfo m, object v)
		{
			object ret = null;
			try
			{
				ret = ((m is FieldInfo) ?
					   ((FieldInfo)m).GetValue(v) :
					   ((PropertyInfo)m).GetValue(v, null));
			}
			/* some properties are not supported, and we should just not list them */
			catch (TargetInvocationException) { }
			/* we don't support indexed properties, either, which trigger this exception */
			catch (TargetParameterCountException) { }
			return ret;
		}

		static void SetProperty(object source)
		{
			((PropertySetter)source).Assign();
		}

		static void ClearProperty(object source)
		{
			((PropertySetter)source).Clear();
		}

		static void SetRSKey(object source)
		{
			((RemoteKeySetter)source).Assign();
		}

		static void ClearRSKey(object source)
		{
			((RemoteKeySetter)source).Clear();
		}

		struct PropertySetter
		{
			readonly SerializedProperty m_Prop;
			readonly object m_Target;
			readonly String m_FieldPath;
			readonly String m_Type;

			public PropertySetter(SerializedProperty p,
								  object target,
								  String fp,
								  String t)
			{
				m_Prop = p;
				m_Target = target;
				m_FieldPath = fp;
				m_Type = t;
			}

			public void Assign()
			{
				m_Prop.FindPropertyRelative(m_TargetString).objectReferenceValue = (Object)m_Target;
				m_Prop.FindPropertyRelative(m_FieldPathString).stringValue = m_FieldPath;
				if (string.IsNullOrEmpty(m_Prop.FindPropertyRelative(m_RSKeyNameString).stringValue))
				{
					m_Prop.FindPropertyRelative(m_TypeString).stringValue = m_Type;
				}
				m_Prop.serializedObject.ApplyModifiedProperties();
			}

			public void Clear()
			{
				m_Prop.FindPropertyRelative(m_TargetString).objectReferenceValue = null;
				m_Prop.FindPropertyRelative(m_FieldPathString).stringValue = null;
				m_Prop.FindPropertyRelative(m_TypeString).stringValue = null;
				m_Prop.serializedObject.ApplyModifiedProperties();
			}
		}

		struct RemoteKeySetter
		{
			readonly SerializedProperty m_Prop;
			readonly String m_RsKey;
			readonly String m_Type;

			public RemoteKeySetter(SerializedProperty p, String rsk, String t)
			{
				m_Prop = p;
				m_RsKey = rsk;
				m_Type = t;
			}

			public void Assign()
			{
				m_Prop.FindPropertyRelative(m_RSKeyNameString).stringValue = m_RsKey;
				if (string.IsNullOrEmpty(m_Prop.FindPropertyRelative(m_FieldPathString).stringValue))
				{
					m_Prop.FindPropertyRelative(m_TypeString).stringValue = m_Type;
				}
				m_Prop.serializedObject.ApplyModifiedProperties();
			}

			public void Clear()
			{
				m_Prop.FindPropertyRelative(m_RSKeyNameString).stringValue = null;
				m_Prop.FindPropertyRelative(m_TypeString).stringValue = null;

				m_Prop.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
