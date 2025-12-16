using System;
using System.Collections.Generic;
using QFramework;
using Reflex.Attributes;
using UnityEngine;

public class BBQController : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private Transform BBQViewContainer;
    private Dictionary<string, BBQView> BBQViewDictionary = new Dictionary<string, BBQView>();
    // 正在创建的烧烤视图
    void OnEnable()
    {
        this.RegisterEvent<MoveBBQToRepositoryEvent>(OnMoveBBQToRepository);
        this.RegisterEvent<RemoveBBQFromRepositoryEvent>(OnRemoveBBQFromRepository);
    }

    void OnDisable()
    {
        this.UnRegisterEvent<MoveBBQToRepositoryEvent>(OnMoveBBQToRepository);
        this.UnRegisterEvent<RemoveBBQFromRepositoryEvent>(OnRemoveBBQFromRepository);
    }
    void LateUpdate()
    {
        DetectBBQ();
    }

    // 检测烧烤交互
    private void DetectBBQ()
    {
        if (Input.GetMouseButtonDown(0) && this.GetSystem<IStickSystem>().selectedStick != null){
            if (this.GetSystem<BlackboardSystem>().hoveredCell == null) return;
            Stick selectedStick = this.GetSystem<IStickSystem>().selectedStick;
            BoardCell hoveredCell = this.GetSystem<BlackboardSystem>().hoveredCell;
            List<FoodInstance> foodInstances = selectedStick.strategy.GetFood(hoveredCell.position);
            if (foodInstances.Count == 0){
                Debug.Log("没有食材可烧烤");
                return;
            }
            AudioManager.Instance.AudioService.Play("Score 5");
            this.GetSystem<IBBQSystem>().FinishBBQ(selectedStick, foodInstances);

            // 3. 清空预览
            this.SendEvent(new HideBBQPreviewEvent());
            this.SendEvent(new ResetRecipePreviewViewsEvent());
        }
    }


    private void OnMoveBBQToRepository(MoveBBQToRepositoryEvent e){
        // 1. 创建个插槽
        RectTransform slotTransform = new GameObject("Slot").AddComponent<RectTransform>();
        slotTransform.SetParent(BBQViewContainer, true);
        slotTransform.anchoredPosition = Vector2.zero;
        slotTransform.localScale = Vector3.one;
        slotTransform.localEulerAngles = Vector3.zero;
        slotTransform.sizeDelta = new Vector2(100, 100);
        BBQView bbqView = e.bbqView;

        // 2. 将烧烤视图移动到插槽位置
        bbqView.transform.SetParent(slotTransform, true);
        bbqView.transform.localPosition = Vector3.zero;
        bbqView.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        bbqView.transform.localEulerAngles = Vector3.zero;
        bbqView.IsBuilding = false;

        BBQ bbq = bbqView.GetBBQ();
        bbqView.UpdateVisual();
        BBQViewDictionary.Add(bbq.guid, bbqView);
    }
    // 移除烧烤视图
    private void RemoveBBQView(BBQ bbq){
        if (!BBQViewDictionary.TryGetValue(bbq.guid, out BBQView bbqView)) return;
        this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
            Destroy(bbqView.transform.parent.gameObject);
            BBQViewDictionary.Remove(bbq.guid);
        }));
        this.GetSystem<IAnimationSystem>().Play();
    }

    private void OnRemoveBBQFromRepository(RemoveBBQFromRepositoryEvent e){
        RemoveBBQView(e.bbq);
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
public class MoveBBQToRepositoryEvent : AbstractEvent{
    public BBQView bbqView;
    public MoveBBQToRepositoryEvent(BBQView bbqView){
        this.bbqView = bbqView;
    }
}

public class BBQPreview{
    public int totalRarity;
    public int totalTaste;
    public int totalTimeCost;
    public List<BoardCell> boardCells;
    public BBQPreview(int totalRarity, int totalTaste, int totalTimeCost, List<BoardCell> boardCells){
        this.totalRarity = totalRarity;
        this.totalTaste = totalTaste;
        this.totalTimeCost = totalTimeCost;
        this.boardCells = boardCells;
    }
}