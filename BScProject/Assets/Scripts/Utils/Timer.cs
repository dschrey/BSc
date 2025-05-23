using System;
using UnityEngine;

public class Timer
{
    private float _startTime = 0.0f;
    private float _endTime = 0.0f;
    private bool _isActive = false;

    public void StartTimer()
    {
        if (!_isActive)
        {
            _isActive = true;
            _startTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"Timer is already running.");
        }
    }
    public void RestartTimer()
    {
        _isActive = true;
        _startTime = Time.time;
    }
    
    public void StopTimer()
    {
        if (_isActive)
        {
            _endTime = Time.time;
            _isActive = false;
        }
    }

    public void ResetTimer()
    {
        _isActive = false;
        _startTime = 0f;
        _endTime = 0f;
    }

    public string GetTimeFormated()
    {
        float time;
        if (_isActive)
        {
            time =  Time.time - _startTime;
        }
        else
        {
            time =  _endTime - _startTime;
        }

        int sec = (int)Math.Floor(time);

        int hours = sec / 3600;
        int minutes = sec % 3600 / 60;
        int remainingSec = sec % 60;

        if (sec > 3600)
            return  $"{hours:D2}:{minutes:D2}:{remainingSec:D2}";
        else
            return  $"{minutes:D2}:{remainingSec:D2}";
    }

    public float GetTime()
    {
        if (_isActive)
        {
            return Time.time - _startTime;
        }
        else
        {
            return _endTime - _startTime;
        }
    }

}
