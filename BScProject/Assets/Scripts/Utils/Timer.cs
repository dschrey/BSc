using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float _startTime;
    private float _endTime;
    private bool _isActive;
    
    public void StartTimer() 
    {
        if (! _isActive)
        {
            _isActive = true;
            _startTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"Timer is already running.");
        }
    }
    
    public void Stop()
    {
        if (_isActive)
        {
            _endTime = Time.time;
            _isActive = false;
        }
        else
        {
            Debug.LogWarning("Timer has not been started yet.");
        }
    }

    public void Reset()
    {
        _startTime = 0f;
        _endTime = 0f;
        _isActive = false;
    }

    public string GetElapsedTimeFormated()
    {
        float time;
        if (_isActive)
        {
            Debug.LogWarning("Timer still running. Returning current elapsed time.");
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

    public float GetElapsedTime()
    {
        if (_isActive)
        {
            Debug.LogWarning("Time still running. Returning current elapsed time.");
            return Time.time - _startTime;
        }
        else
        {
            return _endTime - _startTime;
        }
    }

}
