using UnityEngine;

public class PixelData : MonoBehaviour
{
	public bool Next()
	{
		state++;
		return state == 2;
	}

	private int state;
}