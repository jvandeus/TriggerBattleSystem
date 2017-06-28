using System;
using UnityEngine;
using System.Collections;

enum StatType {MAXHP, SPD, STR, DEX, INT};

// TODO: figure out the proper components needed
public class CharacterStats : MonoBehaviour {
	public int level;
	public int xp;
	public float[] baseStats;
	public float MaxHitpoints
	{
		get { return baseStats[(int)StatType.MAXHP]; }
		set
		{
			baseStats[(int)StatType.MAXHP] = value;
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

	// TODO: function to rise this objects statistics to the next "level"
}