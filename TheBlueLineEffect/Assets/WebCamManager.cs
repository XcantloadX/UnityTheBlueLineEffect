using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum ShootType
{
    UnityScene,
    StaticPicture,
    Camera
}

public class WebCamManager : MonoBehaviour
{
    public RectTransform scanLine; //扫描线(蓝线)
    public RawImage image; //显示摄像头或者 Scene 的 Image
    public RenderTexture sceneTexture; //渲染 Scene 的 RenderTexture
    public Texture2D shootTexture; //拍到的照片
    public RawImage coverImage; //用来显示照片
    public Texture2D staticPic; //静态图片

    public ShootType shootType;
    public float speed = 0.05f;
    public float percent = 0;
    private bool started = false;
    private WebCamTexture webcam; //摄像机画面

    // Use this for initialization
    void Start()
    {
        Init();

    }

    private void Init()
    {
        if(shootType == ShootType.Camera)
        {
            webcam = new WebCamTexture();
            Debug.Log(WebCamTexture.devices[0].name);
            webcam.deviceName = WebCamTexture.devices[0].name;
            image.rectTransform.Rotate(0, 0, webcam.videoRotationAngle);
            image.texture = webcam;
            webcam.Play();
        }
        else if (shootType == ShootType.UnityScene && sceneTexture != null)
            image.texture = sceneTexture;
        
    }

    //开始拍照
    public void StartShoot()
    {
        percent = 0;
        started = true;
        string method = "";

        if (shootType == ShootType.StaticPicture)
        {
            shootTexture = new Texture2D(staticPic.width, staticPic.height, TextureFormat.ARGB32, true);
            method = "ShootPicture";
        }
        else if (shootType == ShootType.Camera)
        {
            shootTexture = new Texture2D(webcam.width, webcam.height, TextureFormat.ARGB32, true);
            method = "ShootCam";
        }
        else if (shootType == ShootType.UnityScene)
        {
            shootTexture = new Texture2D(sceneTexture.width, sceneTexture.height, TextureFormat.ARGB32, true);
            method = "ShootUnityScene";
        }

        coverImage.texture = shootTexture;
        MoveLine(0);
        StartCoroutine(method);
    }

    void Update()
    {
    }

    IEnumerator ShootCam()
    {
        while (percent <= 1)
        {
            yield return new WaitForEndOfFrame();

            int y = webcam.height - (int)(webcam.height * percent);
            int index = webcam.width * y;

            Debug.Log(y + "|" + webcam.height);

            Color[] source = webcam.GetPixels();
            Color[] dest = new Color[source.Length];

            dest = shootTexture.GetPixels(); //上一帧拍的照片
            Array.Copy(source, 0, dest, 0, index); //将扫描线以下的部分复制到照片上，覆盖原来的，扫描线以上的部分不变

            shootTexture.SetPixels(dest);
            shootTexture.Apply();

            MoveLine(percent);
            percent += speed;
        }

    }

    IEnumerator ShootPicture()
    {
        while (percent <= 1)
        {
            yield return new WaitForEndOfFrame();

            //拍照部分
            int y = (int)(staticPic.height * (1 - percent));
            int index = staticPic.width * y;

            Color[] source = staticPic.GetPixels();
            Color[] dest = new Color[source.Length];
            Array.Copy(source, index, dest, index, source.Length - index);

            shootTexture.SetPixels(dest);
            shootTexture.Apply();

            //移动进度条
            MoveLine(percent);
            percent += speed;
        }

    }

    IEnumerator ShootUnityScene()
    {
        while(percent <= 1)
        {
            yield return new WaitForEndOfFrame();

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = sceneTexture;

            //拍照部分
            float y = sceneTexture.height * percent;
            float height = sceneTexture.height - y;
            shootTexture.ReadPixels(new Rect(0, y, sceneTexture.width, height), 0, 0);
            shootTexture.Apply();

            //移动进度条
            MoveLine(percent);
            percent += speed;
            RenderTexture.active = prev;
        }
    }

    private void MoveLine(float percent)
    {
        float coverHeight = coverImage.rectTransform.rect.height;
        scanLine.anchoredPosition = new Vector2(scanLine.anchoredPosition.x, -(coverHeight * percent));
    }

    public void Flip()
    {
        //TODO 搞定旋转之后，X Y 轴颠倒的问题
        coverImage.rectTransform.Rotate(0, 0, -90);
        image.rectTransform.Rotate(0, 0, -90);

    }
}