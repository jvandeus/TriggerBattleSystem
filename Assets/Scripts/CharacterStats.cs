using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// static class for extending enumeration methods.
public static class StatExtensions
{
	// bitwise stat variables for defining centralized stat types and conditions
    public static StatType RegenStats = StatType.HP | StatType.MP;
    
    // this may not be the best method or used in the future, but i wanted to keep it here for future referance. This would allow one to do something like stat.canAutoRegen
    public static bool canAutoRegen(this StatType stat)
    {
    	// bitwise AND operation to determine if the flag is is in the defined stat set.
    	return (RegenStats & stat) == stat ? true : false;
    }
}

// NOTE: referance here: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/enumeration-types
// define the stats as flags to they can easily be for centralized condition/requirements checking.
[Flags]
public enum StatType {
	NONE = 0x0,
	HP = 0x1,
	MP = 0x2,
	SPD = 0x4,
	STR = 0x8,
	DEX = 0x10,
	INT = 0x20
};

// defining the different status this character can be in
public enum StatusType {HEALTHY, KNOCKOUT, DEAD};

// TODO: figure out the proper components needed
public class CharacterStats : MonoBehaviour {
	public int level;
	public int xp;
	public StatusType status;
	private Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
	// TODO: look into if the dictionary is the way do go. Might not be since it throws expections if elements are not added with the .Add function.
	public float MaxHitPoints
	{
		get { return baseStats[StatType.HP]; }
		set
		{
			baseStats[StatType.HP] = value;
		}
	}
	public float MaxMagicPoints
	{
		get { return baseStats[StatType.MP]; }
		set
		{
			baseStats[StatType.MP] = value;
		}
	}
	public float Speed
	{
		get { return baseStats[StatType.SPD]; }
		set
		{
			baseStats[StatType.SPD] = value;
		}
	}
	public float Strength
	{
		get { return baseStats[StatType.STR]; }
		set
		{
			baseStats[StatType.STR] = value;
		}
	}
	public float Dexterity
	{
		get { return baseStats[StatType.DEX]; }
		set
		{
			baseStats[StatType.DEX] = value;
		}
	}
	public float Intelligence
	{
		get { return baseStats[StatType.INT]; }
		set
		{
			baseStats[StatType.INT] = value;
		}
	}

	// All essential intialization after the component is instantiated and before the component gets enabled.
	void Awake () {
		// TODO: initialize all stats in the baseStats dictionary to 0 with the .Add() function
		
	}

	// Initialization after the component first gets enabled. This happens on the very next Update() call.
	void Start () {
		// display stats on screen somehow.
	}

	public static int rollDiceDropLowest(int diceAmount, int diceSides, int dropAmount)
	{
		List<int> diceResults = new List<int>();
		int total = 0;
		string output = "Dice Results: ";
		for (int i = 0; i < diceAmount; i++) {
			diceResults[i] = UnityEngine.Random.Range(1, diceSides+1);
		}
		diceResults.Sort((a, b) => -1* a.CompareTo(b)); // descending sort
		for (int i = 0; i < diceAmount; i++) {
			if (i < diceSides-dropAmount) {
				total += diceResults[i];
			}
			output += diceResults[i].ToString()+" ";
		}
		output += "with a toal result of "+total.ToString();
		Debug.Log(output);
		return total;
	}

	// randomize all stats. For now we can do a 4d6, drop the lowest for each stat.
	public void randomizeBaseStats()
	{
		// get the values of the enum so we may iterate through them
		var values = Enum.GetValues(typeof(StatType));
		// set the value to the last in the enum
		StatType stat = (StatType) Enum.ToObject(typeof(StatType), values.GetValue(values.GetUpperBound(0)));
		// COUNT IS TEMP FOR DEBUG! just want to make sure we don't loop forever.
		int count = 0;
		// loop and increment the stat type back until we hit the NONE type and we are done.
		while (stat != StatType.NONE && count < 20) {
			Debug.Log("Rolling for state type"+stat.ToString());
			this.baseStats[stat] = (float)rollDiceDropLowest(4, 6, 1);
			stat--;
			count++;
		}
	}

	// set all base stats to zero
	public void zeroBaseStats()
	{
		// get the values of the enum so we may iterate through them
		var values = Enum.GetValues(typeof(StatType));
		// set the value to the last in the enum
		StatType stat = (StatType) Enum.ToObject(typeof(StatType), values.GetValue(values.GetUpperBound(0)));
		// COUNT IS TEMP FOR DEBUG! just want to make sure we don't loop forever.
		int count = 0;
		// loop and increment the stat type back until we hit the NONE type and we are done.
		while (stat != StatType.NONE && count < 20) {
			Debug.Log("Rolling for state type"+stat.ToString());
			this.baseStats[stat] = (float)rollDiceDropLowest(4, 6, 1);
			stat--;
			count++;
		}
	}

	// TODO: create a function to get the effective stat, after all effects from outside sources
	
	// TODO: function to apply a temporary stat boost or debuff with different such as time or turns or ticks, whatever.
	// NOTE: consider how this will work with HP and MP. To keep things generic im going to consider it like a debuff on health.

	// TODO: function to check for effect of a stat lowering or raising. For example if health goes to zero, the player is dead or knocked out.
	
	// TODO: function to check if the player is dead?

	// TODO: function to rise this objects statistics to the next "level"
}