using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private Transform lookAt;
    private Vector3 startOffset;
    private Vector3 moveVector;

    private float transition = 1f;
    private readonly float animationDuration = 2f;
    private Vector3 animationOffset = new(0f, 5f, -5f);

    // Start is called before the first frame update
    void Start()
    {
        lookAt = GameObject.FindGameObjectWithTag("Agent").transform; // Gets Players current position
        startOffset = transform.position - lookAt.position; // start distance of cam
    }

    // Update is called once per frame
    void Update()
    {
        moveVector = lookAt.position + startOffset;
       
        // X - To stay in center
        moveVector.x = 0; 
        
        // Y - to restrict till a certain height
        moveVector.y = Mathf.Clamp(moveVector.y,1,5);

        if (transition < 0f)
        {
            transform.position = moveVector;  // pos of cam = pos of player 
        }
        else
        {   
            // Animation at start of the game
            transform.position = Vector3.Lerp(moveVector + animationOffset, moveVector, transition);
            transition += Time.deltaTime * 1 / animationDuration;
            transform.LookAt(lookAt.position + Vector3.up); 
        }

       
    }
}
