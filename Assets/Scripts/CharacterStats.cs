using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// static class for extending enumeration methods.
public static class StatExtensions
{
	// bitwise stat variables for defining centralized stat types and conditions
    public static StatType RegenStats = StatType.HP | StatType.MP;
    public static StatType positiveBaseStats = StatType.HP;
    // default percentage of HP that will be given when a character is revived.
    public static float defaultRevivePercent = 0.1f;
    // default lower limits for stats
    public static Dictionary<StatType,float> defaultLowerLimits = new Dictionary<StatType, float> {
	    {StatType.HP, 0f},
	};
	// default upper limits for stats
    public static Dictionary<StatType,float> defaultUpperLimits = new Dictionary<StatType, float> {
	    {StatType.HP, 0f},
	};
    
    // this may not be the best method or used in the future, but i wanted to keep it here for future referance.
    // This would allow one to do something like stat.canAutoRegen
    public static bool canAutoRegen(this StatType stat)
    {
    	// bitwise AND operation to determine if the flag is in the defined stat set.
    	return (RegenStats & stat) == stat ? true : false;
    }

    // check if this stat's base stat can be 0 or less.
    public static bool needsPositiveBase(this StatType stat)
    {
    	// bitwise AND operation to determine if the flag is in the defined stat set.
    	return (positiveBaseStats & stat) == stat ? true : false;
    }

    // get the default limit if there is one set, otherwise get null
    public static float? getDefaultLowerLimit(this StatType stat)
    {
    	float limit;
    	if (defaultLowerLimits.TryGetValue(stat, out limit)) {
    		return limit;
    	}
    	return null;
    }
    // get the default limit if there is one set, otherwise get null
    public static float? getDefaultUpperLimit(this StatType stat)
    {
    	float limit;
    	if (defaultUpperLimits.TryGetValue(stat, out limit)) {
    		return limit;
    	}
    	return null;
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

// define the different conditions this character can be in
public enum ConditionType {NONE, HEALTHY, KNOCKOUT, LIMBO, DEAD};

// Defines a value for a stat to be assigne to any menber of the StatType enum
[System.Serializable]
public struct StatValue
{
    // The stats core value before any modifiers
    public float baseValue;
    // The amount this stat is modified relative to the baseValue
    public float modifier;
    // the minimun this stat can be modified to relative to 0. Set to null for unlimited
    public float? lowerLimit;
    // the max this stat can be modified to relative to the baseValue. Set to null for unlimited
    public float? upperLimit;

    // generic full constructor
    public StatValue (float value = 0f, float mod = 0f, float? min = null, float? max = null)
    {
        this.baseValue = value;
        this.modifier = mod;
        this.lowerLimit = min;
        this.upperLimit = max;
    }
    // conversion from float to StatValue.
    // EX: StatValue statpoint = 15f; would result in a stat with baseValue of 15f
    public static implicit operator StatValue(float value)
    {
        return new StatValue(value);
    }
    // conversion from float to StatValue.
    // EX: StatValue statpoint = 15D; would result in a stat with baseValue of 15f
    public static implicit operator StatValue(double value)
    {
        return new StatValue((float)value);
    }
    // conversion from float to StatValue.
    // EX: StatValue statpoint = 15; would result in a stat with baseValue of 15f
    public static implicit operator StatValue(int value)
    {
        return new StatValue((float)value);
    }
    // conversion from StatValue to float.
    // EX: float statvalue = new StatValue(15f, 1f); would result in 16f
    public static implicit operator float(StatValue value)
    {
        return value.baseValue + value.modifier;
    }
    // conversion from StatValue to double.
    // EX: double statvalue = new StatValue(15f, 1f); would result in 16D
    public static implicit operator double(StatValue value)
    {
        return (double)(float)value;
    }
    // conversion from StatValue to int.
    // EX: int statvalue = new StatValue(15f, 1f); would result in 16
    public static implicit operator int(StatValue value)
    {
        return (int)Mathf.Round((float)value);
    }
    // conversion from StatValue to string.
    // EX: string statvalue = new StatValue(15f, 1f); would result in 16.00
    public static implicit operator string(StatValue value)
    {
        // output to 2 decimal places
        return string.Format("{0:N2}", (float)value);
    }
    public override string ToString()
    {   
        return string.Format("{0:N2}", (this.baseValue + this.modifier));
    }
}

// Manage a set of StatValues for a character
public class CharacterStats : MonoBehaviour {
	[SerializeField]
	public string characterName;
	[SerializeField]
	public int level;
	[SerializeField]
	public int xp;
	// the current condition of this player, health-wise
	[HideInInspector, SerializeField]
	private ConditionType condition;
	// this will store any manually set condition for use in the updateCondition() function
	private ConditionType pendingCondition = ConditionType.NONE;
	// defines what condition this character will revive to if no condition is provided.
	private ConditionType reviveCondition = ConditionType.HEALTHY;
	// Indexs a StatValue for each of the StateType enum
	private Dictionary<StatType, StatValue> statSet = new Dictionary<StatType, StatValue>();
	// lists for storing the dictionary data for serialization
	[HideInInspector, SerializeField]
	private List<StatType> _statSetKeys = new List<StatType> {};
	[HideInInspector, SerializeField]
	private List<StatValue> _statSetValues = new List<StatValue> {};
	// bitwise flags to define the stat types that the condition is dependant upon, so we know when to update it.
	private StatType conditionStats = StatType.HP;
	// flags to define what stats cannot be modified. Initially this is will be no stat types.
	private StatType immutableStats = StatType.NONE;
	// getters and setters for base stats.
	public float BaseHP
	{
		get { return this.getBaseStatOrZero(StatType.HP); }
		set {
			this.setBaseStat(StatType.HP, value);
		}
	}
	public float BaseMP
	{
		get { return this.getBaseStatOrZero(StatType.MP); }
		set {
			this.setBaseStat(StatType.MP, value);
		}
	}
	public float BaseSPD
	{
		get { return this.getBaseStatOrZero(StatType.SPD); }
		set {
			this.setBaseStat(StatType.SPD, value);
		}
	}
	public float BaseSTR
	{
		get { return this.getBaseStatOrZero(StatType.STR); }
		set {
			this.setBaseStat(StatType.STR, value);
		}
	}
	public float BaseDEX
	{
		get { return this.getBaseStatOrZero(StatType.DEX); }
		set {
			this.setBaseStat(StatType.DEX, value);
		}
	}
	public float BaseINT
	{
		get { return this.getBaseStatOrZero(StatType.INT); }
		set {
			this.setBaseStat(StatType.INT, value);
		}
	}
	// getters and setters for each effective stat
	public float HP
	{
		get { return this.getStat(StatType.HP); }
		set {
			this.setStat(StatType.HP, value);
		}
	}
	public float MP
	{
		get { return this.getStat(StatType.MP); }
		set {
			this.setStat(StatType.MP, value);
		}
	}
	public float SPD
	{
		get { return this.getStat(StatType.SPD); }
		set {
			this.setStat(StatType.SPD, value);
		}
	}
	public float STR
	{
		get { return this.getStat(StatType.STR); }
		set {
			this.setStat(StatType.STR, value);
		}
	}
	public float DEX
	{
		get { return this.getStat(StatType.DEX); }
		set {
			this.setStat(StatType.DEX, value);
		}
	}
	public float INT
	{
		get { return this.getStat(StatType.INT); }
		set {
			this.setStat(StatType.INT, value);
		}
	}
	// getter and setter for Condition, so we can be sure to have stats reflect this
	[ExposeProperty]
	public ConditionType Condition
	{
		get { return this.condition; }
		set {
			// set a pending condition and let updateCondition() function handle it.
			this.pendingCondition = value;
			updateCondition();
		}
	}

	// Unity doesn't support dictoinary serialization, so we transfer the data to a list of keys and values first.
	public void storeStatSet()
    {
        this._statSetKeys.Clear();
        this._statSetValues.Clear();

        // TODO: possibly iterate through the enum instead of the dictionary, and store a 0 StatValue if not set in the dictionary?
        foreach (var kvp in this.statSet)
        {
            this._statSetKeys.Add(kvp.Key);
            this._statSetValues.Add(kvp.Value);
        }
    }
    // Load the data from the the key and value lists back into the dictionary
    public void loadStatSet()
    {
        this.statSet = new Dictionary<StatType, StatValue>();

        for (int i = 0; i != Math.Min(this._statSetKeys.Count, this._statSetValues.Count); i++)
            this.statSet.Add(this._statSetKeys[i], this._statSetValues[i]);
    }

	// All essential intialization after the component is instantiated and before the component gets enabled.
	void Awake () {
		// because dictionaries are nto serializable, we will load the data from lists first
		this.loadStatSet();
		// make sure at least the base stat for HP is greater than zero
		if (this.BaseHP <= 0f) {
			this.BaseHP = 1;
		}
	}

	// Initialization after the component first gets enabled. This happens on the very next Update() call.
	void Start () {
		// if there is no name, give it a name manually for now.
		if (this.characterName == "") {
			this.characterName = "FartFace";
		}
		// TODO: display stats on screen somehow?
	}

	void OnGUI () {
		this.loadStatSet();
	}

	// return the StatValue of the specified type if it exists in the statSet. Otherwise return a 0 StatValue
	public StatValue getStatValue(StatType type)
	{
		StatValue thisStat;
		if (this.statSet.TryGetValue(type, out thisStat)) {
			return thisStat;
		}
		// some stats can bever have a base value equal to or less than zero, so we need to fix it if its not.
		if (type.needsPositiveBase()) {
			// just set it to 1f for now
			StatValue defaultValue = 1f;
			this.statSet.Add(type, defaultValue);
			return defaultValue;
		}
		return thisStat;
	}

	// return the base stat if it exists, otherwise return 0
	public float getBaseStatOrZero(StatType type)
	{
		StatValue thisStat = this.getStatValue(type);
		return thisStat.baseValue;
	}

	// return the mod stat if it exists, otherwise return 0
	public float getStatMod(StatType type)
	{
		StatValue thisStat = this.getStatValue(type);
		return thisStat.modifier;
	}

	// return the maximum amount a stat can be modified to relative to the base stat
	public float? getUpperLimit(StatType type)
	{
		StatValue thisStat = this.getStatValue(type);
		// if its null we will try and get the default for this stat
		if (thisStat.upperLimit == null) {
			return type.getDefaultUpperLimit();
		}
		return thisStat.upperLimit;
	}

	// return the minimum amount a stat can be modified to relative to 0
	public float? getLowerLimit(StatType type)
	{
		StatValue thisStat = this.getStatValue(type);
		// if its null we will try and get the default for this stat
		if (thisStat.lowerLimit == null) {
			return type.getDefaultLowerLimit();
		}
		return thisStat.lowerLimit;
	}

	// return the maximum amount an effective stat can be. return null if it is not limited
	public float? getMaxStat(StatType type)
	{
		float? limit = this.getUpperLimit(type);
		if (limit == null) return null;
		return this.getBaseStatOrZero(type) + this.getUpperLimit(type);
	}

	// return the minimum amount an effective stat can be. return null if it is not limited
	public float? getMinStat(StatType type)
	{
		float? limit = this.getLowerLimit(type);
		if (limit == null) return null;
		return this.getLowerLimit(type);
	}

	// get the effective stat, with all modifiers applied.
	public float getStat(StatType type)
	{
		StatValue thisStat = this.getStatValue(type);
		return thisStat.baseValue + thisStat.modifier;
	}

	// return all effective stats in one Dictionary
	public Dictionary<StatType, StatValue> getAllStats()
	{
		Dictionary<StatType, StatValue> allStats = new Dictionary<StatType, StatValue>();
		foreach(StatType stat in Enum.GetValues(typeof(StatType))) {
			if (stat != StatType.NONE) {
				allStats.Add(stat, this.getStat(stat));
			}
		}
		return allStats;
	}

	// set a base stat to a value ONLY if they have this stat. otherwise don't do anything.
	// this will not update any dependants so its kept private
	private void setBaseStatIfExists(StatType type, float value)
	{
		if (this.statSet.ContainsKey(type)) {
			this.statSet[type] = new StatValue(value, this.statSet[type].modifier, this.statSet[type].lowerLimit, this.statSet[type].upperLimit);
		}
		// store the Dictionary into serializable lists after any change
		this.storeStatSet();
	}

	// set a base stat to a value OR add it if it doesn't exist.
	// this will not update any dependants so its kept private
	private void setOrAddBaseStat(StatType type, float value)
	{
		// make sure we aren't setting anything non-positive that shouldn't be
		if (type.needsPositiveBase() && value <= 0f) {
			// just set it to 1f for now
			value = 1f;
		}
		// set the stat, and if its not in the dictionary yet, add it.
		if (this.statSet.ContainsKey(type)) {
			this.statSet[type] = new StatValue(value, this.statSet[type].modifier, this.statSet[type].lowerLimit, this.statSet[type].upperLimit);
		} else {
			this.statSet.Add(type, value);
		}
		// store the Dictionary into serializable lists after any change
		this.storeStatSet();
	}

	// set a base stat to a value OR add it if it doesn't exist.
	// this public version will update dependants
	public void setBaseStat(StatType type, float value)
	{
		// don't do anything if the flag is set for this stat to be immutable.
		if ((immutableStats & type) == type) return;
		// set the stat
		this.setOrAddBaseStat(type, value);
		// bitwise AND operation to determine if the flag is in the defined stat set for stats that Condition depends on.
    	if ((this.conditionStats & type) == type) {
    		this.updateCondition();
    	}
	}

	// set a base stat to a value OR add it if it doesn't exist without updating any dependants
	// this will not update any dependants so its kept private
	private void setOrAddStatMod(StatType type, float value)
	{
		if (this.statSet.ContainsKey(type)) {
			this.statSet[type] = new StatValue(this.statSet[type].baseValue, value, this.statSet[type].lowerLimit, this.statSet[type].upperLimit);
		} else {
			this.statSet.Add(type, new StatValue(0f, value));
		}
		// store the Dictionary into serializable lists after any change
		this.storeStatSet();
	}

	// set a modifier for a specified stat to the specified value.
	// The stat mod will be limited by other properties such as min and max, and immutable stat flags.
	// This will update any dependants as well.
	public void setStatMod(StatType type, float value)
	{
		// don't do anything if the flag is set for this stat to be immutable.
		if ((immutableStats & type) == type) return;
		StatValue thisStat = this.getStatValue(type);
		float? lowerLimit = this.getLowerLimit(type);
		float? upperLimit = this.getUpperLimit(type);
		float newValue = value;
		// make sure the value we set don't go beyond lower limit if there is one
		if (lowerLimit != null) {
			lowerLimit = lowerLimit - thisStat.baseValue;
			if (value < lowerLimit) {
				newValue = (float)lowerLimit;
			}
		}
		// make sure the value we set don't go beyond upper limit if there is one
		if (upperLimit != null) {
			if (value > upperLimit) {
				newValue = (float)upperLimit;
			}
		}
		// don't bother adding a mod if its not even changed.
		if (newValue == this.getStatMod(type)) return;
		// set the mod, add the item in the dictionary if it doesn't exist
		this.setOrAddStatMod(type, newValue);
		// bitwise AND operation to determine if the flag is in the defined stat set for stats that Condition depends on.
    	if ((this.conditionStats & type) == type) {
    		this.updateCondition();
    	}
	}

	// Add the specified value to a modifier for the specified stat and update any dependants
	public void modStat(StatType type, float value)
	{
		// set the stat mod relative to its current mod value
		this.setStatMod(type, this.getStatMod(type) + value);
	}

	// set a modifier so that the effective stat of that type will equal the given value and update any dependants
	public void setStat(StatType type, float value)
	{
		// don't do anything if the flag is set for this stat to be immutable.
		if ((immutableStats & type) == type) return;
		// set the stat mod based on what the base stat is.
		this.setStatMod(type, value - this.getBaseStatOrZero(type));
	}

	// remove all modifiers on a stat and update any dependants
	public void removeStatMod(StatType type)
	{
		// don't do anything if the flag is set for this stat to be immutable.
		if ((immutableStats & type) == type) return;
		// set the mod to zero for this stat if it is in the set
		if (this.statSet.ContainsKey(type)) {
			this.setStatMod(type, 0f);
		}
	}

	// check all stats which would change the condition of this character
	public void updateCondition()
	{
		// if there is no change in condition then its the same as no change in condition
		if (this.pendingCondition == this.condition) {
			this.pendingCondition = ConditionType.NONE;
		}
		// check if a forced condition is pending
		if (this.pendingCondition != ConditionType.NONE) {
			// force the stats to what they should be for this condition
			if (this.pendingCondition == ConditionType.DEAD && this.HP != 0f) {
				// manually set the effective hp to 0
				this.setOrAddStatMod(StatType.HP, 0f - this.getBaseStatOrZero(StatType.HP));
			} else if (this.pendingCondition != ConditionType.DEAD && this.HP <= 0f) {
				// set the hp to the default revive percentage
				this.setOrAddStatMod(StatType.HP, this.getReviveHPMod());
			}
			// If all stats were able to be modified, the logic below will update the condition.
		}
		// if HP is below 0, this character is dead no matter what
		if (this.condition != ConditionType.DEAD && this.HP <= 0f) {
			this.condition = ConditionType.DEAD;
			// clear any pending condition
			this.pendingCondition = ConditionType.NONE;
			// TODO: consider making HP immutable, so that the character has to be "revived" before they can heal?
			Debug.Log("The character \""+this.characterName+"\" is now in a "+this.condition.ToString()+" state");
			return;
		}
		// check if the player has has come back from death
		if (this.condition == ConditionType.DEAD && this.HP > 0f && this.pendingCondition == ConditionType.NONE) {
			// if there is no pending condition, set the condition to whatever is the default for reviving
			this.pendingCondition = this.reviveCondition;
		}
		// if there is a condition to change to, do it
		if (this.pendingCondition != ConditionType.NONE) {
			this.condition = this.pendingCondition;
			Debug.Log("The character \""+this.characterName+"\" is now in a "+this.condition.ToString()+" state");
			// clear any pending condition
			this.pendingCondition = ConditionType.NONE;
		}
	}

	// TODO: migrate this to a utilities class
	public static int rollDiceDropLowest(int diceAmount, int diceSides, int dropAmount)
	{
		List<int> diceResults = new List<int>();
		int total = 0;
		int count = 0;
		for (int i = 0; i < diceAmount; i++) {
			diceResults.Add(UnityEngine.Random.Range(1, diceSides+1));
		}
		diceResults.Sort((a, b) => -1* a.CompareTo(b)); // descending sort
		// add up everything except the ones we will drop
		foreach (int value in diceResults) {
			if (count < diceAmount-dropAmount) {
				total += value;
			}
			count++;
		}
		return total;
	}

	// randomize all stats. For now we can do a 4d6, drop the lowest for each stat.
	public void randomizeBaseStats()
	{
		// loop through all types of stats and set a value in the dictionary for each
		foreach(StatType stat in Enum.GetValues(typeof(StatType))) {
			if (stat != StatType.NONE) {
				// check if the stat exists in the baseStat dictionary first. If not, add it.
				this.setOrAddBaseStat(stat, (float)rollDiceDropLowest(4, 6, 1));
			}
		}
	}

	// set all base stats to zero
	public void zeroBaseStats()
	{
		// loop through all types of stats and set a value in the dictionary for each
		foreach(StatType stat in Enum.GetValues(typeof(StatType))) {
			if (stat != StatType.NONE) {
				// check if the stat exists in the baseStat dictionary first. If not, add it.
				this.setOrAddBaseStat(stat, 0f);
			}
		}
	}

	// write a line in the debug window showing each element in the dictionary specified
	public void debugLogStatDictionary(Dictionary<StatType, StatValue> theseStats)
	{
		foreach (KeyValuePair<StatType, StatValue> stat in theseStats) {
			Debug.Log(""+stat.Key.ToString()+": "+stat.Value.ToString());
		}
	}

	// write a line in the debug window showing each base stat for this character
	public void debugLogBaseStats()
	{
		Debug.Log("Base Stats for Character with the name \""+this.characterName+"\" will be displayed next.");
		this.debugLogStatDictionary(this.statSet);
	}

	/**
	 * --------------------------------------------------
	 * Function for typical actions on a character
	 * --------------------------------------------------
	 */

	// check if this character is dead
	public bool isDead()
	{
		return (this.condition == ConditionType.DEAD) ? true : false;
	}

	// check if this character is dead
	public bool isAlive()
	{
		return !this.isDead();
	}

	// heal this character by a specified amount. A positive value would Incease effective HP.
	public void heal(float amount) {
		this.modStat(StatType.HP, amount);
	}

	// damage this character by a specified amount. A positive value would Decrease effective HP.
	public void damage(float amount) {
		this.modStat(StatType.HP, 0f - amount);
	}

	// return HP to its "full" value
	public void fullHeal() {
		this.removeStatMod(StatType.HP);
	}

	// kill the character if they are not already dead
	public void kill()
	{
		this.HP = 0;
	}

	// get the value hp will be at if this character is revived using provided percentage as a float, ranged 0.0f to 1.0f
	public float getReviveHP(float percent)
	{
		float newHP = Mathf.Round(this.BaseHP * percent);
		// make sure its at least 1
		if (newHP < 1f) { newHP = 1f; }
		return newHP;
	}

	// get the value for the stat modifier needed to set HP to the amount it should when this character is revived.
	public float getReviveHPMod(float percent)
	{
		return this.getReviveHP(percent) - this.BaseHP;
	}

	// get the value hp will be at if this character is revived with default properties
	public float getReviveHP()
	{
		return this.getReviveHP(StatExtensions.defaultRevivePercent);
	}

	// get the value for the stat modifier needed to set HP to the amount it should when this character is revived using default properties
	public float getReviveHPMod()
	{
		return this.getReviveHP() - this.BaseHP;
	}

	// revive the character from death using the current defaults for reviving
	public void revive()
	{
		// cannot revive if the character is not dead
		if (this.Condition != ConditionType.DEAD) return;
		// set HP to the default amount
		this.HP = this.getReviveHP();
	}

	// revive the character from death and set hp to the specified percentage of base HP
	public void revive(float percent)
	{
		// cannot revive to 0 hp
		if (percent == 0f) return;
		// cannot revive if the character is not dead
		if (this.Condition != ConditionType.DEAD) return;
		// set HP to the given percentage
		this.HP = this.getReviveHP(percent);
	}

	// revive the character from death, set hp to the default percentage and set to specified condition
	public void revive(ConditionType newCondition)
	{
		// cannot revive character back to dead
		if (newCondition == ConditionType.DEAD) return;
		this.pendingCondition = newCondition;
		// revive to default given percentage
		this.revive();
	}

	// revive the character from death, set hp to the specified percentage of base HP, and set to specified condition
	public void revive(float percent, ConditionType newCondition)
	{
		// cannot revive character back to dead
		if (newCondition == ConditionType.DEAD) return;
		this.pendingCondition = newCondition;
		// revive to the given percentage
		this.revive(percent);
	}
	
	// TODO: function to apply a temporary stat boost or debuff with different such as time or turns or ticks, whatever.

	// TODO: function to rise this objects statistics to the next "level"
}