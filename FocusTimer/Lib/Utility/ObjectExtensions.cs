﻿namespace FocusTimer.Lib.Utility;

public static class ObjectExtensions
{
    public static log4net.ILog GetLogger(this object obj)
    {
        return log4net.LogManager.GetLogger(obj.GetType());
    }
}