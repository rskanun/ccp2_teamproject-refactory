public class LoadingProgressTracker
{
    private float loadAssetProgress;
    private float loadSceneProgress;

    private const float ASSET_WEIGHT = 0.3f;
    private const float SCENE_WEIGHT = 0.7f;

    public float CurrentProgress { get; private set; }

    public void SetAssetProgress(float progress)
    {
        loadAssetProgress = progress;
    }

    public void SetSceneProgress(float progress)
    {
        loadSceneProgress = progress;
    }

    public void UpdateProgress()
    {
        CurrentProgress = (ASSET_WEIGHT * loadAssetProgress) + (SCENE_WEIGHT * loadSceneProgress);
    }
}