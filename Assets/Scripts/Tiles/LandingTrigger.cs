using UnityEngine;

public class LandingTrigger : MonoBehaviour
{
    public enum LandingType { Upper, Lower }
    [SerializeField] private LandingType landingType;
    [SerializeField] private StairController mainController;

    public LandingType GetLandingType() => landingType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mainController != null)
        {
            mainController.OnLandingTriggerEntered(other, landingType);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (mainController != null)
        {
            mainController.OnLandingTriggerExited(other, landingType);
        }
    }
}
