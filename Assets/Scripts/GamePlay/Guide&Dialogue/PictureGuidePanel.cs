using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PictureGuidePanel : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    private List<Sprite> picturesGuides => SettingManager.Instance.ArtSettings.PicturesGuides.sprites;
    private int currentIndex = 0;
    void Start()
    {
        currentIndex = 0;
        UpdateView();
    }
    public void Right(){
        currentIndex++;
        if (currentIndex >= picturesGuides.Count){
            currentIndex = picturesGuides.Count - 1;
        }
        UpdateView();
    }
    public void Left(){
        currentIndex--;
        if (currentIndex < 0){
            currentIndex = 0;
        }
        UpdateView();
    }

    private void UpdateView(){
        image.sprite = picturesGuides[currentIndex];
        text.text = $"{currentIndex + 1}/{picturesGuides.Count}";
    }
}
