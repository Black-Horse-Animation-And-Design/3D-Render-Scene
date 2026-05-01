using UnityEngine;

public class BlendshapeDriver : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer smr;

    [SerializeField] int blendshapeA = 0;
    [SerializeField] int blendshapeB = 1;

    [SerializeField] float speed = 200f;

    float goalA;
    float goalB;


    void Update()
    {
        if (smr == null) return;

        float currentA = smr.GetBlendShapeWeight(blendshapeA);
        float currentB = smr.GetBlendShapeWeight(blendshapeB);

        float newA = Mathf.MoveTowards(currentA, goalA, speed * Time.unscaledDeltaTime);
        float newB = Mathf.MoveTowards(currentB, goalB, speed * Time.unscaledDeltaTime);

        smr.SetBlendShapeWeight(blendshapeA, newA);
        smr.SetBlendShapeWeight(blendshapeB, newB);
    }

    public void SetGoalA100()
    {
        goalA = 100f;
        goalB = 0f;
    }

    public void SetGoalATo0_BTo100()
    {
        goalA = 0f;
        goalB = 100f;
    }
}