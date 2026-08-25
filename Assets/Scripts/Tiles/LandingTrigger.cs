using UnityEngine;

public class LandingTrigger : MonoBehaviour
{
    public enum LandingType { Upper, Lower }
    public LandingType landingType;
    public StairController mainController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mainController != null)
        {
            mainController.OnLandingTriggerEntered(other, landingType);
        }
    }
}
