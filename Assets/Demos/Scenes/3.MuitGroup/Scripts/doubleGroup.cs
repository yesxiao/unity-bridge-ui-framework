﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using BridgeUI.Model;
using BridgeUI;
using BridgeUI.Binding;
using System;

public class doubleGroup : MonoBehaviour
{
    private const string pane01 = "Panel01";
    private IUIFacade uiFacade;
    private MainPanelViewModel mainViewModel;
    private MainPanelViewModel_with_ID mainViewModel_withID;
    private void Awake()
    {
        uiFacade = UIFacade.Instence;
        mainViewModel = new MainPanelViewModel();
        mainViewModel_withID = new MainPanelViewModel_with_ID();
        mainViewModel.titleStr.Value = "我是:MainPanelViewModel";
        mainViewModel_withID.title.Value = "我是:MainPanelViewModel_with_ID";
    }
    private void OnGUI()
    {
        if (GUILayout.Button("Open:MainPanel"))
        {
            var dic = new Dictionary<string, object>();
            dic["title"] = "我是主面板";
            dic["info"] = "当传入IDictionary时，会自动填充绑定好的字段或属性";
            dic["method"] = "可以向方法传递一个参数";
            dic["ondestroy"] = new Action<string>((x)=> { Debug.Log(x); });
            dic["OpenPanel01"]= dic["OpenPanel02"]= dic["OpenPanel03"] = new PanelAction<Button>((context,button) => {
                Debug.Log(button +": onClicked");
            });
      

            uiFacade.Open(PanelNames.MainPanel, dic);
        }
        if (GUILayout.Button("Open:MainPanel with viewModel"))
        {
            uiFacade.Open(PanelNames.MainPanel, mainViewModel);
        }
        if (GUILayout.Button("Open:MainPanel with viewModel with id"))
        {
            uiFacade.Open(PanelNames.MainPanel, mainViewModel_withID);
        }
        for (int i = 0; i < 2; i++)
        {
            if (GUILayout.Button("Open:Panel01  " + i))
            {
                OpenPanel01(i);
            }
        }
        if (GUILayout.Button("Close " + pane01))
        {
            uiFacade.Close(pane01);
        }
        if (GUILayout.Button("Hide " + pane01))
        {
            uiFacade.Hide(pane01);
        }


    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1)){
            mainViewModel.switcherView.Value = !mainViewModel.switcherView.Value;
        }
        mainViewModel.infoStr.Value = UnityEngine.Random.Range(0, 100).ToString();
        var keywordProp = mainViewModel.GetBindableProperty<string>("keyword");
        if (keywordProp != null)
            keywordProp.ValueBoxed = UnityEngine.Random.Range(0, 100).ToString();
    }

    private void OpenPanel01(int index)
    {
        var dic = new Hashtable();
        dic[0] = "我是panel01";
        var handle = uiFacade.Open(pane01, index + "你好panel01");
        Debug.Log(index + "handle:" + handle);
        handle.RegistCallBack((panel, data) =>
        {
            Debug.Log(index + "onCallBack" + panel + ":" + data);
        });
        handle.RegistCreate((panel) =>
        {
            Debug.Log(index + "onCreate:" + panel);
        });
        handle.RegistClose((panel) =>
        {
            Debug.Log(index + "onCloese:" + panel);
        });

        handle.Send(dic);
    }
}
