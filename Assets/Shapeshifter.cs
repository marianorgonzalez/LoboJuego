using UnityEngine;

public class Shapeshifter : MonoBehaviour
{
    [SerializeField] float pressedInputBuffering = .2f;
    [SerializeField] float shapeshiftCooldown = 1;
    bool isHuman = true;
    float lastShapeshiftTime = Mathf.NegativeInfinity;
    bool IsShapeshiftInCooldown => Time.time - lastShapeshiftTime <= shapeshiftCooldown;
    bool ShapeshiftRequested => Time.time - shapeshiftPressTime <= pressedInputBuffering;
    float shapeshiftPressTime = Mathf.NegativeInfinity;

    void Update()
    {
        if (Input.GetButtonDown("Fire3"))
            shapeshiftPressTime = Time.time;
    }

    void FixedUpdate()
    {
        if (IsShapeshiftInCooldown == false && ShapeshiftRequested)
        {
            ShapeShift();
        }
    }

    private void ShapeShift()
    {
        isHuman = !isHuman;
        if (isHuman)
        {
            print("Te convertiste en humano!");
        }
        else
        {
            print("Te convertiste en hombre lobo!");
        }
        lastShapeshiftTime = Time.time;
        shapeshiftPressTime = Mathf.NegativeInfinity;
    }
}
