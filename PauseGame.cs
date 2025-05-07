using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject Resume;
    public GameObject MusicOn;
    public GameObject MusicOff;
    public GameObject exit;
    public GameObject Levels;
    public GameObject SoundOn;
    public GameObject SoundOff;

    private bool isPaused = false;

    public AudioSource musicSource;
    public AudioSource[] soundSources;

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Resume.SetActive(false);
        MusicOn.SetActive(false);
        MusicOff.SetActive(false);
        exit.SetActive(false);
        Levels.SetActive(false);
        SoundOff.SetActive(false);
        SoundOn.SetActive(false);
        if (PlayerPrefs.GetInt("MusicState", 1) == 1)
        {
            MusicOn.SetActive(false);
            MusicOff.SetActive(false);
            musicSource.mute = false;
        }
        else
        {
            MusicOn.SetActive(false);
            MusicOff.SetActive(false);
            musicSource.mute = true;
        }
        if (PlayerPrefs.GetInt("SoundState", 1) == 1)
        {
            SoundOn.SetActive(false);
            SoundOff.SetActive(false);
            SetAllSoundSourcesMute(false);
        }
        else
        {
            SoundOn.SetActive(false);
            SoundOff.SetActive(false);
            SetAllSoundSourcesMute(true);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            Resume.SetActive(true);
            exit.SetActive(true);
            Levels.SetActive(true);
            musicSource.Pause();
            if (PlayerPrefs.GetInt("MusicState", 1) == 1)
            {
                MusicOn.SetActive(true);
                MusicOff.SetActive(false);
            }
            else
            {
                MusicOn.SetActive(false);
                MusicOff.SetActive(true);
            }
            if (PlayerPrefs.GetInt("SoundState", 1) == 1)
            {
                SoundOn.SetActive(true);
                SoundOff.SetActive(false);
            }
            else
            {
                SoundOn.SetActive(false);
                SoundOff.SetActive(true);
            }
        }
        else
        {
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            Resume.SetActive(false);
            MusicOn.SetActive(false);
            MusicOff.SetActive(false);
            exit.SetActive(false);
            Levels.SetActive(false);
            SoundOn.SetActive(false);
            SoundOff.SetActive(false);
            musicSource.Play();
        }
    }
    public void ToggleMusic()
    {
        if (MusicOn.activeSelf)
        {
            MusicOn.SetActive(false);
            MusicOff.SetActive(true);
            musicSource.mute = true;
            PlayerPrefs.SetInt("MusicState", 0);
        }
        else if (MusicOff.activeSelf)
        {
            MusicOn.SetActive(true);
            MusicOff.SetActive(false);
            musicSource.mute = false;
            PlayerPrefs.SetInt("MusicState", 1);
        }

        PlayerPrefs.Save();
    }

    public void ToggleSound()
    {
        if (SoundOn.activeSelf)
        {
            SoundOn.SetActive(false);
            SoundOff.SetActive(true);
            SetAllSoundSourcesMute(true);
            PlayerPrefs.SetInt("SoundState", 0);
        }
        else if (SoundOff.activeSelf)
        {
            SoundOn.SetActive(true);
            SoundOff.SetActive(false);
            SetAllSoundSourcesMute(false);
            PlayerPrefs.SetInt("SoundState", 1);
        }

        PlayerPrefs.Save();
    }

    private void SetAllSoundSourcesMute(bool mute)
    {
        foreach (var source in soundSources)
        {
            source.mute = mute;
        }
    }
}
