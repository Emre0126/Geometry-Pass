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

    public AudioSource musicSource; // M�zik kayna��
    public AudioSource[] soundSources; // Ses kaynaklar� dizisi

    void Start()
    {
        // Ba�lang�� ayarlar�n� y�kle
        isPaused = false;
        Time.timeScale = 1f;

        // T�m UI elemanlar�n� ba�lang��ta gizle
        pausePanel.SetActive(false);
        Resume.SetActive(false);
        MusicOn.SetActive(false);
        MusicOff.SetActive(false);
        exit.SetActive(false);
        Levels.SetActive(false);
        SoundOff.SetActive(false);
        SoundOn.SetActive(false);

        // M�zik durumunu y�kle ve ayarla
        if (PlayerPrefs.GetInt("MusicState", 1) == 1)
        {
            MusicOn.SetActive(false);
            MusicOff.SetActive(false); // �lk durumda gizli kalacak
            musicSource.mute = false;
        }
        else
        {
            MusicOn.SetActive(false);
            MusicOff.SetActive(false); // �lk durumda gizli kalacak
            musicSource.mute = true;
        }

        // Ses durumu y�kle ve ayarla
        if (PlayerPrefs.GetInt("SoundState", 1) == 1)
        {
            SoundOn.SetActive(false);
            SoundOff.SetActive(false); // �lk durumda gizli kalacak
            SetAllSoundSourcesMute(false);
        }
        else
        {
            SoundOn.SetActive(false);
            SoundOff.SetActive(false); // �lk durumda gizli kalacak
            SetAllSoundSourcesMute(true);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Oyunu durdur ve UI elemanlar�n� g�ster
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            Resume.SetActive(true);
            exit.SetActive(true);
            Levels.SetActive(true);
            musicSource.Pause();

            // M�zik durumu g�ncelle
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

            // Ses durumu g�ncelle
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
            // Oyunu devam ettir ve UI elemanlar�n� gizle
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
            PlayerPrefs.SetInt("MusicState", 0); // M�zik kapal� olarak kaydet
        }
        else if (MusicOff.activeSelf)
        {
            MusicOn.SetActive(true);
            MusicOff.SetActive(false);
            musicSource.mute = false;
            PlayerPrefs.SetInt("MusicState", 1); // M�zik a��k olarak kaydet
        }

        PlayerPrefs.Save(); // De�i�iklikleri kaydet
    }

    public void ToggleSound()
    {
        if (SoundOn.activeSelf)
        {
            SoundOn.SetActive(false);
            SoundOff.SetActive(true);
            SetAllSoundSourcesMute(true);
            PlayerPrefs.SetInt("SoundState", 0); // Ses kapal� olarak kaydet
        }
        else if (SoundOff.activeSelf)
        {
            SoundOn.SetActive(true);
            SoundOff.SetActive(false);
            SetAllSoundSourcesMute(false);
            PlayerPrefs.SetInt("SoundState", 1); // Ses a��k olarak kaydet
        }

        PlayerPrefs.Save(); // De�i�iklikleri kaydet
    }

    private void SetAllSoundSourcesMute(bool mute)
    {
        foreach (var source in soundSources)
        {
            source.mute = mute;
        }
    }
}
