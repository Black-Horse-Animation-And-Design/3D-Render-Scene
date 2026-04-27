using System.Collections;
using System.IO;
using UnityEngine;

public class OfflineRenderer : MonoBehaviour
{
    [SerializeField] int framesToRender, targetFps = 60;
    [SerializeField] float secondsToConverge = 1f;
    [SerializeField] int width = 1920;
    [SerializeField] int height = 1080;
    [SerializeField] string folder = "OfflineFrames";

    [SerializeField] Animator animator;

    [SerializeField] int captureFrame;
    [SerializeField] int animatorFrame;

    int frameIndex;
    Texture2D tex;

    float clipLength;
    float clipFPS = 60f;

    IEnumerator Start()
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        SetupAnimationInfo();
        animator.Play(0, 0, 0f);
        animator.Update(0f);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(secondsToConverge);

        while (frameIndex < framesToRender)
        {
            UpdateAnimatorFrame();

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;


            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(secondsToConverge);
            yield return new WaitForEndOfFrame();

            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            string path = folder + "/frame_" + frameIndex.ToString("D4") + ".png";
            File.WriteAllBytes(path, bytes);

            frameIndex++;
            captureFrame++;

            yield return StartCoroutine(AdvanceOneFrame());
        }

        Debug.Log("Render complete");
    }

    void SetupAnimationInfo()
    {
        if (animator == null) return;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;

        if (controller == null || controller.animationClips.Length == 0) return;

        AnimationClip clip = controller.animationClips[0];

        clipLength = clip.length;
        clipFPS = clip.frameRate;

        framesToRender = Mathf.RoundToInt(clipLength * clipFPS);
    }

    IEnumerator AdvanceOneFrame()
    {
        if (animator == null) yield break;

        float nextFrameTime = captureFrame / clipFPS;
        float normalizedTime = nextFrameTime / clipLength;

        Time.timeScale = 0f;

        animator.Play(0, 0, normalizedTime);
        animator.Update(1f / clipFPS); // <-- THIS is the ke

        yield return null;

        UpdateAnimatorFrame();
    }

    void UpdateAnimatorFrame()
    {
        if (animator == null) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        float normalizedTime = state.normalizedTime % 1f;
        float currentTime = normalizedTime * clipLength;

        animatorFrame = Mathf.FloorToInt(currentTime * clipFPS);
    }
}