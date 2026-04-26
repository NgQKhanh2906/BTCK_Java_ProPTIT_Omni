using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : Panel
{
    [Header("Settings UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void OnEnable()
    {
        if (musicSlider != null) 
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        
        if (sfxSlider != null) 
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    
    public void SaveAndClose()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();

        Debug.Log("Đã lưu cấu hình OMNI!");
        Close();
    }
    
    public void BackWithoutSaving()
    {
        Close(); 
    }

    public void OnMusicVolumeChanged(float value)
    {
        Debug.Log("Âm lượng nhạc: " + value);
    }
}