using UnityEngine;
public class TestTransformation : MonoBehaviour
{
    public PlayerTransform playerTransform;
    public TransformationData frogData;
    public TransformationData spiderData;
    public TransformationData ladybugData;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerTransform.TransformInto(frogData);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerTransform.TransformInto(spiderData);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerTransform.TransformInto(ladybugData);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            playerTransform.RevertToBaseForm();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"Can Swim: {playerTransform.CanSwim()}");
            Debug.Log($"Can Wall Climb: {playerTransform.CanWallClimb()}");
            Debug.Log($"Can Fit Small Gaps: {playerTransform.CanFitSmallGaps()}");
            Debug.Log($"Current Form: {playerTransform._currentTransformation.transformName}");
        }
    }
}