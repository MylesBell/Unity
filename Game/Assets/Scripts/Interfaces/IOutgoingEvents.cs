using UnityEngine;
using System.Collections;

public interface IOutgoingEvents {

	// This is called when Game State changes from Setup to Idle or from End to Idle
	void GameReady();

	// This is called when Game State changes from Idle to Playing
	void GameStart();

	// This is called when Game State changes from Playing to End
	void GameEnd();

	void HeroDeath();
}
