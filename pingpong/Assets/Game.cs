using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    [System.Serializable]
    public class GameParameters
    {
        [Header("Ball Settings")]
        public float BallSpeed; 
        public float BallRadius;
    
        [Header("Platform Settings")]
        public float PlatformSpeed;
        public float PlatformWidth; 
        public float PlatformHeight;
    
        [Header("Screen Settings")]
        public float ScreenWidth; 
        public float ScreenHeight; 
    
        [Header("Tilt Settings")]
        public float TiltFactor; 
    }

    [System.Serializable]
    public class GameState
    {
        public Vector2 BallPosition;
        public Vector2 BallVelocity;
        public float PlatformPosition;
    }

    [SerializeField] private GameParameters parameters = new GameParameters();
    private GameState state = new GameState();

    private class GameObjects
    {
        public GameObject Background;
        public GameObject Ball;
        public GameObject Platform;
    }

    private GameObjects gameObjects = new GameObjects();

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        CreateGameObjects();
        ResetGame();
    }

    private void CreateGameObjects()
    {
        gameObjects.Background = CreateQuad("Background", new Vector3(0, 0, 1), new Vector2(parameters.ScreenWidth, parameters.ScreenHeight), CreateMaterial(Color.black));
        gameObjects.Ball = CreateQuad("Ball", Vector3.zero, new Vector2(parameters.BallRadius * 2, parameters.BallRadius * 2), CreateMaterial(Color.red));
        gameObjects.Platform = CreateQuad("Platform", new Vector3(0, -parameters.ScreenHeight / 2, 0), new Vector2(parameters.PlatformWidth, parameters.PlatformHeight), CreateMaterial(Color.blue));
    }

    private GameObject CreateQuad(string name, Vector3 position, Vector2 scale, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.position = position;
        quad.transform.localScale = new Vector3(scale.x, scale.y, 1);
        quad.GetComponent<Renderer>().material = material;
        Destroy(quad.GetComponent<Collider>());
        return quad;
    }

    private Material CreateMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Unlit/Color"));
        material.color = color;
        return material;
    }

    private void ResetGame()
    {
        state.BallPosition = Vector2.zero;
        state.BallVelocity = new Vector2(parameters.BallSpeed, parameters.BallSpeed).normalized * parameters.BallSpeed;
        state.PlatformPosition = 0;
    }

    private void Update()
    {
        UpdatePlatform();
        UpdateBall();
        CheckCollision();
        UpdateGraphics();
    }

    private void UpdatePlatform()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        state.PlatformPosition += horizontalInput * parameters.PlatformSpeed * Time.deltaTime;
        state.PlatformPosition = Mathf.Clamp(state.PlatformPosition, -parameters.ScreenWidth / 2 + parameters.PlatformWidth / 2, parameters.ScreenWidth / 2 - parameters.PlatformWidth / 2);
    }

    private void UpdateBall()
    {
        state.BallPosition += state.BallVelocity * Time.deltaTime;

        if (Mathf.Abs(state.BallPosition.x) > parameters.ScreenWidth / 2 - parameters.BallRadius)
        {
            state.BallVelocity.x = -state.BallVelocity.x;
            state.BallPosition.x = Mathf.Sign(state.BallPosition.x) * (parameters.ScreenWidth / 2 - parameters.BallRadius);
        }
        if (state.BallPosition.y > parameters.ScreenHeight / 2 - parameters.BallRadius)
        {
            state.BallVelocity.y = -state.BallVelocity.y;
            state.BallPosition.y = parameters.ScreenHeight / 2 - parameters.BallRadius;
        }

        if (state.BallPosition.y < -parameters.ScreenHeight / 2 + parameters.BallRadius)
        {
            ResetGame();
        }
    }

    private void CheckCollision()
    {
        if (state.BallPosition.y - parameters.BallRadius <= -parameters.ScreenHeight / 2 + parameters.PlatformHeight &&
            state.BallPosition.x >= state.PlatformPosition - parameters.PlatformWidth / 2 &&
            state.BallPosition.x <= state.PlatformPosition + parameters.PlatformWidth / 2)
        {
            state.BallVelocity.y = Mathf.Abs(state.BallVelocity.y);
            state.BallVelocity = Quaternion.Euler(0, 0, Random.Range(-15f, 15f)) * state.BallVelocity;
            state.BallVelocity = state.BallVelocity.normalized * parameters.BallSpeed;
            state.BallPosition.y = -parameters.ScreenHeight / 2 + parameters.PlatformHeight + parameters.BallRadius;
        }
    }

    private void UpdateGraphics()
    {
        if (gameObjects.Ball != null)
        {
            gameObjects.Ball.transform.position = new Vector3(state.BallPosition.x + parameters.TiltFactor, state.BallPosition.y, 0);
        }

        if (gameObjects.Platform != null)
        {
            gameObjects.Platform.transform.position = new Vector3(state.PlatformPosition - parameters.TiltFactor, -parameters.ScreenHeight / 2 + parameters.PlatformHeight / 2, 0);
        }
    }

    private void OnDestroy()
    {
        DestroyGameObjects();
    }

    private void DestroyGameObjects()
    {
        Destroy(gameObjects.Background);
        Destroy(gameObjects.Ball);
        Destroy(gameObjects.Platform);
    }
}