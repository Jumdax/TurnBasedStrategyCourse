using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Final Project - HOLLOWDEEP start screen: locked objective/instructions,
/// Master Volume slider (silent by default), START button, and one looping
/// background ambience track. No SFX, no AudioMixer, no persistence.
///
/// Freezes CameraController until START is pressed - the only gameplay system
/// this needs to gate explicitly. UnitActionSystem already ignores clicks
/// under a raycast-blocking full-screen panel via its own EventSystem check,
/// and turns/enemy actions never auto-start regardless of this panel.
///
/// Must be attached to a GameObject in GameScene.unity by Britt (scene-owned),
/// with all serialized references wired via the Inspector - this script never
/// edits the scene itself.
/// </summary>
public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Button startButton;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private CameraController cameraController;

    private void Awake()
    {
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = 0f;

            if (!backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Play();
            }
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 0f;
        }

        if (cameraController != null)
        {
            cameraController.enabled = false;
        }

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetBackgroundVolume);
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(SetBackgroundVolume);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    private void SetBackgroundVolume(float value)
    {
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = value;
        }
    }

    private void OnStartButtonClicked()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
}
