using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTips : MonoBehaviour
{
    [SerializeField] private GameObject _droneTips;
    [SerializeField] private GameObject _mapTips;
    
    private GameController _gameController;

    public void Init(GameController gameController)
    {
        _gameController = gameController;

        _gameController.OnMapActive += SwitchInputTipsActive;
    
    }

    private void OnDisable()
    {
        if (_gameController != null)
            _gameController.OnMapActive -= SwitchInputTipsActive;
    }

    private void SwitchInputTipsActive(bool mapActive)
    {
        _droneTips.SetActive(!mapActive);
        _mapTips.SetActive(mapActive);
    }
}
