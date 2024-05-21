using UnityEngine;

public class PlaneSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Vector3 startCorner;
    public Vector3 endCorner;
    public int rows = 5;
    public int columns = 5;

    void Start()
    {
        SpawnBalls();
    }

    void SpawnBalls()
    {
        Vector3 widthVector = (endCorner - startCorner) / columns;
        Vector3 heightVector = (Vector3.Cross(widthVector, Vector3.up).normalized * (Vector3.Distance(startCorner, endCorner) / rows)).normalized * Vector3.Distance(startCorner, endCorner) / rows;

        for (int i = 0; i <= columns; i++)
        {
            for (int j = 0; j <= rows; j++)
            {
                Vector3 spawnPosition = startCorner + i * widthVector + j * heightVector;
                spawnPosition.y = 0.3f;
                Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}
