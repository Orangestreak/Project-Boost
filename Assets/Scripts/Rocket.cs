using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{

    AudioSource audioSource;
    Rigidbody rigidBody;

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip complete;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem pMainEngine;
    [SerializeField] ParticleSystem pComplete;
    [SerializeField] ParticleSystem pDeath;

    bool isTransitioning = false;

    bool collisionsDisabled = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTransitioning)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        } else if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !collisionsDisabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isTransitioning || collisionsDisabled)
        {
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }

        if (collision.relativeVelocity.magnitude > 10)
        {
            isTransitioning = true;
            Invoke("ResetLevel", 2f);
            StopEffects();
            audioSource.PlayOneShot(death); //TODO find better death noise
            pDeath.Play();
        }
        else
        {
            switch (collision.gameObject.tag)
            {
                case "Friendly":
                    print("Friendly");
                    break;

                case "Finish":
                    isTransitioning = true;
                    Invoke("LoadNextLevel", 1.5f);
                    StopEffects();
                    audioSource.PlayOneShot(complete);
                    pComplete.Play();
                    break;

                default:
                    isTransitioning = true;
                    Invoke("ResetLevel", 2f);
                    StopEffects();
                    audioSource.PlayOneShot(death); //TODO find better death noise
                    pDeath.Play();
                    break;
            }
        }
    }

    private void LoadNextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;
        if (currentScene == SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }
        SceneManager.LoadScene(nextScene);
    }

    private void ResetLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        //stop audio if not holding space
        else
        {
            StopEffects();
        }
    }

    private void RespondToRotateInput()
    {

        //freeze physics when rotating
        rigidBody.angularVelocity = Vector3.zero;

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust);
        //start playing audio if not playing
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        pMainEngine.Play(); //TODO improve particles
        //currently showing where they would be not what it should be
    }

    private void StopEffects()
    {
        audioSource.Stop();
        pMainEngine.Stop();
    }
}
