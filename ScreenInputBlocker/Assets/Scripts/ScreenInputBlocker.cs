using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



[RequireComponent(typeof(CanvasGroup))]
public class ScreenInputBlocker : MonoBehaviour
{
    [SerializeField] CanvasGroup m_canvGroup;
    [SerializeField] Image m_fullscreenBlockImg;

    [Header("Partial Input blocker")]
    [SerializeField] GameObject Prefab_ImageBlocker;
    [SerializeField] Vector2 referenceInputBoxSize;
    [SerializeField] Vector2 m_referenceResolution;

    [Header("Test case")]
    [SerializeField] RectTransform m_testButton;

    public bool IsBlocking { get { return m_currentBlockRequests > 0; } }
    public bool IsPartiallyBlocking { get { return m_currentPatitalBlockRequests; } }
    public bool IsTemporaryBlock { get { return IsBlocking && m_currentTempBlockRequests > 0; } }

    int m_currentBlockRequests = 0;
    int m_currentTempBlockRequests = 0;
    bool m_currentPatitalBlockRequests = false;

    //Partial Input Blockers
    RectTransform m_fullscreenRectTrans;
    Rect inputRect;
    Vector3[] m_corners;

    List<GameObject> m_partialInputBlockers = new List<GameObject>();

    void Start()
    {
        UnblockInput();
        m_corners = new Vector3[4];
        m_fullscreenRectTrans = GetComponent<RectTransform>();
    }

    /*
     * TestPartialBlocker is a test function to move our test box to a random location and draw blockers around it
    */
    public void TestPartialBlocker()
    {
        Vector3 pos = m_testButton.localPosition;

        Vector3 minPosition = m_fullscreenRectTrans.rect.min - m_testButton.rect.min;
        Vector3 maxPosition = m_fullscreenRectTrans.rect.max - m_testButton.rect.max;

        //Set the button to a random position on the screen
        float xPos = UnityEngine.Random.Range(minPosition.x, maxPosition.x);
        float yPos = UnityEngine.Random.Range(minPosition.y, maxPosition.y);

        m_testButton.localPosition = new Vector2(xPos, yPos);
        Vector3 centerPoint = transform.InverseTransformPoint(m_testButton.position);
        RequestPartialInputBlocker(centerPoint, m_testButton.sizeDelta);
    }

    /*
     * RequestFullScreenBlock is used to block all and and input by simply drawing an image over the canvas to recieve input.
     */
    public void RequestFullScreenBlock()
    {
        m_fullscreenBlockImg.enabled = true;
        m_canvGroup.ToggleAllInteraction(true);
        m_currentBlockRequests++;
    }

    public void RequestPartialInputBlocker(Vector2 screenPoint, Vector2 inputSize)
    {
        m_canvGroup.ToggleAllInteraction(true);
        RemovePartialInputBlockers(); //get rid of the old ones if there are any
        m_currentPatitalBlockRequests = true;

        //if no size is specified, use the default reference box size
        Vector2 inputRectSize = inputSize != Vector2.zero ? inputSize : referenceInputBoxSize;

        Rect midRect = new Rect(screenPoint.x, screenPoint.y, inputRectSize.x, inputRectSize.y);
        midRect.center = screenPoint; //correct the point to be from the corner to the center

        AddPartialInputBlocker(midRect, true); //create the center input 

        m_fullscreenRectTrans.GetLocalCorners(m_corners);
        float totalH = Mathf.Abs(m_corners[0].y - m_corners[1].y);
        float yPos = -totalH / 2;
        float x, y, w, h;
        if (inputRect.xMin > m_corners[0].x)
        {
            w = Mathf.Abs(inputRect.xMin - m_corners[0].x);
            h = totalH;
            x = m_corners[0].x;
            y = yPos;

            AddPartialInputBlocker(new Rect(x, y, w, h));
        }

        if (inputRect.xMax < m_corners[3].x)
        {
            w = Mathf.Abs(inputRect.xMax - m_corners[3].x);
            h = totalH;
            x = inputRect.xMax;
            y = yPos;
            AddPartialInputBlocker(new Rect(x, y, w, h));
        }

        if (inputRect.yMin > m_corners[0].y)
        {
            w = inputRect.width;
            h = Mathf.Abs(inputRect.yMin - m_corners[0].y);
            x = inputRect.xMin;
            y = yPos;
            AddPartialInputBlocker(new Rect(x, y, w, h));
        }

        if (inputRect.yMax < m_corners[1].y)
        {
            w = inputRect.width;
            h = Mathf.Abs(inputRect.yMax - m_corners[1].y);
            x = inputRect.xMin;
            y = inputRect.yMax;
            AddPartialInputBlocker(new Rect(x, y, w, h));
        }
    }

    public void AddPartialInputBlocker(Rect _rect, bool _isCenter = false)
    {
        GameObject imageBlocker = Instantiate(Prefab_ImageBlocker) as GameObject;
        imageBlocker.transform.SetParent(transform);
        imageBlocker.transform.ResetLocal();
        RectTransform blockerRT = imageBlocker.GetComponent<RectTransform>();
        //if its the center rect, we can remove the image and allow input
        blockerRT.SetWidth(_rect.width);
        blockerRT.SetHeight(_rect.height);
        blockerRT.anchoredPosition = new Vector2(_rect.x, _rect.y);
        if (_isCenter)
        {
            var img = imageBlocker.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = Color.clear;
            inputRect = _rect;
            imageBlocker.name = "AllowedInputBox";
        }

        m_partialInputBlockers.Add(imageBlocker);
    }

    public void RemovePartialInputBlockers()
    {
        for (int i = 0; i < m_partialInputBlockers.Count; ++i)
        {
            var partialBlocker = m_partialInputBlockers[i];
            Destroy(partialBlocker);
        }
        m_partialInputBlockers.Clear();
        m_currentPatitalBlockRequests = false;
    }

    public void RemoveRequest()
    {
        --m_currentBlockRequests;

        if (m_currentBlockRequests > 0)
            return;

        UnblockInput();
    }

    void UnblockInput()
    {
        m_currentBlockRequests = 0;
        m_fullscreenBlockImg.enabled = false;
        //m_canvGroup.ToggleAllInteraction(false);
    }

}