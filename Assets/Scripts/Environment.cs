using System;
using UnityEngine;

// TODO: figure out the proper components needed
public class Environment : MonoBehaviour {
	// All essential intialization after the component is instantiated and before the component gets enabled.
	void Awake () {
	}

	// Initialization after the component first gets enabled. This happens on the very next Update() call.
	void Start () {
	}

	// function to generate a flat grid to serve as a playing field
	// for simplicity's sake this can be a grid of basic squares or 3D boxes with an orthographic camera.
	// alternate colors to make a checker pattern.
	// I imagine in the future the playing field actually takes place in a 3D space, to allow for verticality in AOE abilities, but not needed atm.
}