using UnityEngine;

namespace ProjectX
{
	public class CanvasGroupCrossFade : MonoBehaviour
	{
		public CanvasGroup fromGroup;
		public CanvasGroup toGroup;

		public float time = 1f;

		public virtual void Activate()
		{
            fromGroup.interactable = false;

            toGroup.gameObject.SetActive(true);
			toGroup.alpha = 0f;
			toGroup.interactable = true;

            CanvasGroupFade.FadeAlpha(this, fromGroup, 0.0f, time);
            CanvasGroupFade.FadeAlpha(this, toGroup, 0.0f, time);
		}
	}
}