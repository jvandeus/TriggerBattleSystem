using System;
using UnityEngine;
using System.Collections;

public enum StatType {MAXHP, MAXMP, SPD, STR, DEX, INT};
public enum StatusType {HEALTHY, DEAD, KNOCKOUT};

// TODO: figure out the proper components needed
public class CharacterStats : MonoBehaviour {
	public int level;
	public int xp;
	public StatusType status;
	public float[] baseStats;
	public float MaxHitPoints
	{
		get { return baseStats[(int)StatType.MAXHP]; }
		set
		{
			baseStats[(int)StatType.MAXHP] = value;
		}
	}
	public float MaxMagicPoints
	{
		get { return baseStats[(int)StatType.MAXMP]; }
		set
		{
			baseStats[(int)StatType.MAXMP] = value;
		}
	}
	public float Speed
	{
		get { return baseStats[(int)StatType.SPD]; }
		set
		{
			baseStats[(int)StatType.SPD] = value;
		}
	}
	public float Strength
	{
		get { return baseStats[(int)StatType.STR]; }
		set
		{
			baseStats[(int)StatType.STR] = value;
		}
	}
	public float Dexterity
	{
		get { return baseStats[(int)StatType.DEX]; }
		set
		{
			baseStats[(int)StatType.DEX] = value;
		}
	}
	public float Intelligence
	{
		get { return baseStats[(int)StatType.INT]; }
		set
		{
			baseStats[(int)StatType.INT] = value;
		}
	}

	// All essential intialization after the component is instantiated and before the component gets enabled.
	void Awake () {
	}

	// Initialization after the component first gets enabled. This happens on the very next Update() call.
	void Start () {
	}

	// TODO: create a function to get the effective stat, after all effects from outside sources

	// TODO: randomize all stats to an initial state. For now we can do a 4d6, drop the lowest for each stat.
	
	// TODO: function to apply a temporary stat boost or debuff with different such as time or turns or ticks, whatever.
	// NOTE: consider how this will work with HP and MP. To keep things generic im going to consider it like a debuff on health.

	// TODO: function to check for effect of a stat lowering or raising. For example if health goes to zero, the player is dead or knocked out.
	
	// TODO: function to check if the player is dead?

	// TODO: function to rise this objects statistics to the next "level"
}