using MoreMountains.Feedbacks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class RecipePreviewView : MonoBehaviour{
    [Header("UI Components")]
    [SerializeField] private Image recipeImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI recipeRankText;
    [Header("MM Animation")]
    [SerializeField] private MMF_Player createFeedback;
    [SerializeField] private MMF_Player removeFeedback;
    private LayoutElement layoutElement;
    private RecipeResult recipePreview;
    void Awake()
    {
        layoutElement = gameObject.AddComponent<LayoutElement>();
    }
    void OnEnable()
    {
        createFeedback.PlayFeedbacks();
    }
    public void Bind(RecipeResult recipePreview){
        this.recipePreview = recipePreview;
        UpdateVisual();
    }
    private void UpdateVisual(){
        if (recipePreview == null){
            Debug.LogError("RecipePreview is null");
            return;
        }
        // recipeImage.sprite = Resources.Load<Sprite>("Sprites/" + recipePreview.recipe.icon);
        recipeNameText.text = recipePreview.recipe.name;
        recipeRankText.text = recipePreview.rank.ToString();
    }

    public void Remove(){
        // 结束当前
        createFeedback.SkipToTheEnd();

        layoutElement.ignoreLayout = true;
        removeFeedback.Events.OnComplete.AsObservable().Subscribe(unit => {
            Observable.NextFrame().Subscribe(unit => {
                if (this != null && gameObject != null) {
                    Destroy(gameObject);
                }
            }).AddTo(this);
        }).AddTo(this);

        // 等待一帧后再播放动画
        Observable.NextFrame().Subscribe(unit => {
            removeFeedback.PlayFeedbacks();
        }).AddTo(this);
    }
}