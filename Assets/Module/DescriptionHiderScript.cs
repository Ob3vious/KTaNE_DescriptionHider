using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DescriptionHiderScript : MonoBehaviour
{
    private Type _missionDetailPageType;
    private Type _detailPageType;

    private Type _tmptType;
    private FieldInfo _textDescriptionField;
    private PropertyInfo _tmptText;

    KMGameInfo _gameInfo;

    private bool _inSetup;

    void Awake()
    {
        _missionDetailPageType = Type.GetType("MissionDetailPage, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        _detailPageType = Type.GetType("DetailPage, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        _tmptType = Type.GetType("TMPro.TMP_Text, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        _textDescriptionField = _detailPageType.GetField("TextDescription", BindingFlags.Public | BindingFlags.Instance);
        _tmptText = _tmptType.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
    }

    void Start()
    {
        _gameInfo = GetComponent<KMGameInfo>();
        _gameInfo.OnStateChange += (state) =>
        {
            _inSetup = state == KMGameInfo.State.Setup;
        };
    }

    void Update()
    {
        if (!_inSetup)
            return;

        if (GetMissionBinder() == null)
            return;

        foreach (Component textPage in _missionBinder.GetComponentsInChildren(_missionDetailPageType))
            Convert(textPage);
    }

    private void Convert(object textPage)
    {
        object textObj = _textDescriptionField.GetValue(textPage);

        string content = (string)(_tmptText.GetValue(textObj, new object[0]));
        string modifiedContent = ParseHiding(content);

        if (content != modifiedContent)
            Debug.LogFormat("[DescriptionHider] Parsed: \r\n{0}\r\nto\r\n{1}", content, modifiedContent);

        _tmptText.SetValue(textObj, modifiedContent, new object[0]);
    }

    private Transform _missionBinder;
    private Transform GetMissionBinder()
    {
        if (_missionBinder == null)
        {
            Type type = Type.GetType("BombBinder, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            _missionBinder = ((MonoBehaviour)FindObjectOfType(type)).transform;
        }

        return _missionBinder;
    }

    private string ParseHiding(string input)
    {
        List<string> lines = new List<string>();
        bool hideNext = false;
        using (System.IO.StringReader reader = new System.IO.StringReader(input))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("<hiddensingle>"))
                {
                    hideNext = true;
                    continue;
                }
                else if (line.StartsWith("<hidden>"))
                {
                    break;
                }

                if (hideNext)
                {
                    hideNext = false;
                    continue;
                }

                lines.Add(line);
            }
        }

        return lines.Join("\r\n");
    }
}
