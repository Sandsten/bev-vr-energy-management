using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/*
    Adjust the initial position of the indicators inside the differentiated driving range visualization
    In order to get an even spaceing
    TODO: Include the range indicators here too since they won't be updated during play mode
*/
[ExecuteAlways]
public class DiffDrivingRangeSpeedStepsSettings : MonoBehaviour
{
    public GameObject speedRow;
    
    [Header("Settings for speed indicator steps")]
    public float marginLeft = 30;
    public float spacing = 40;
    public int fontSize = 20;
    public GameObject[] speedSteps;
    public GameObject[] rangeBars;

    float m_dashboardWidth;

    // Update is called once per frame
    void Update()
    {
        UpdateSpeedIndicatorSteps();
    }

    // Update the positions of the speed indicator steps
    void UpdateSpeedIndicatorSteps()
    {
        m_dashboardWidth = GetComponent<RectTransform>().sizeDelta.x;   

        int i = 0;
        foreach (GameObject t in speedSteps)
        {
            float newXPos = -m_dashboardWidth / 2 + marginLeft + i * spacing;
            float yPos = t.GetComponent<RectTransform>().anchoredPosition.y;
            t.GetComponent<RectTransform>().anchoredPosition = new Vector2(newXPos, yPos);

            RectTransform rangeBar = rangeBars[i].GetComponent<RectTransform>();
            rangeBar.anchoredPosition = new Vector2(newXPos, rangeBar.anchoredPosition.y);

            Text tex = t.GetComponentInChildren<Text>();
            tex.fontSize = fontSize;
            tex.text = tex.text.Replace("\\n", "");
            tex.alignment = TextAnchor.MiddleCenter;
            tex.horizontalOverflow = HorizontalWrapMode.Overflow;

            i++;
        }
    }
}
