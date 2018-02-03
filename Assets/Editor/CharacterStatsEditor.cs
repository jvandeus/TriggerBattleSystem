using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
 
[CustomEditor(typeof(CharacterStats))]
public class CharacterStatsEditor : Editor
{
	CharacterStats m_Instance;
	PropertyField[] m_fields;
 
	public void OnEnable()
	{
		m_Instance = target as CharacterStats;
		m_fields = ExposeProperties.GetProperties(m_Instance);
	}
 
	public override void OnInspectorGUI()
	{
		if (m_Instance == null)
			return;
		this.DrawDefaultInspector();
		ExposeProperties.Expose(m_fields);
		// loads the stat set dictionary from the lists to facilitate serialization
		m_Instance.loadStatSet();

		///////////////////////////////////////////////////
		// now begin the custom layout for our inspector //
		///////////////////////////////////////////////////
		// TODO: possibly just make a custom propery drawer for StatValue and use that for each stat?

		var emptyOptions = new GUILayoutOption[0];
		var optsCol1 = new GUILayoutOption[0];
		GUILayoutOption[] optsCol2 = {GUILayout.MaxWidth(100)};
		GUILayoutOption[] optsCol3 = {GUILayout.MaxWidth(100)};
		
		float newBaseValue = 0f;
		float newModValue = 0f;
		float newFinValue = 0f;
		float oldBaseValue = 0f;
		float oldModValue = 0f;
		float oldFinValue = 0f;

		// health bar cause why not
		ProgressBar(m_Instance.HP/ m_Instance.BaseHP, "Current HP");

		// begin stat header row
		EditorGUILayout.BeginVertical(emptyOptions);
		EditorGUILayout.BeginHorizontal(emptyOptions);
		// start with a label for all the stats
		EditorGUILayout.LabelField("Stat Type", "Base", optsCol1);
		// start with a label for all the stats
		EditorGUILayout.LabelField("Mod", optsCol2);
		// start with a label for all the stats
		EditorGUILayout.LabelField("Real", optsCol3);
		// end header row
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		// loop through all types of stats
		foreach(StatType stat in Enum.GetValues(typeof(StatType))) {
			// we want to set a feild for each type except the NONE type
			if (stat != StatType.NONE) {
				// get the old values
				oldBaseValue = m_Instance.getBaseStatOrZero(stat);
				oldModValue = m_Instance.getStatMod(stat);
				oldFinValue = m_Instance.getStat(stat);
				// make a new row
				EditorGUILayout.BeginVertical(emptyOptions);
				EditorGUILayout.BeginHorizontal(emptyOptions);
				// Start with the base stat, and label it with the base value
				newBaseValue = EditorGUILayout.FloatField(stat.ToString(), oldBaseValue, optsCol1);
				if (oldBaseValue != newBaseValue) {
					m_Instance.setBaseStat(stat, newBaseValue);
				}
				// then the mod, with no label
				newModValue = EditorGUILayout.FloatField(GUIContent.none, oldModValue, optsCol2);
				if (oldModValue != newModValue)
					m_Instance.setStatMod(stat, newModValue);
				// then the effective value, with no label
				newFinValue = EditorGUILayout.FloatField(GUIContent.none, oldFinValue, optsCol3);
				if (oldFinValue != newFinValue)
					m_Instance.setStat(stat, newFinValue);
				// end row
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
		}
	}

	// Custom GUILayout progress bar.
    void ProgressBar (float value, string label) {
        // Get a rect for the progress bar using the same margins as a textfield:
        Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
        EditorGUI.ProgressBar (rect, value, label);
        EditorGUILayout.Space ();
    }
}