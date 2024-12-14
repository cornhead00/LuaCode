using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TabMachine : MonoBehaviour
{
    public static TabMachine instance;
    private GameFlowTab gameFlow;
    private void Awake()
    {
        StarTabMachine();
    }
    public void StarTabMachine()
    {
        instance = this;
        gameFlow = new GameFlowTab();
        gameFlow.Initstall();
        gameFlow.Start("s1", 1, 2);
    }
    private void Update()
    {
        if (gameFlow != null)
        {
            UpdateTab(gameFlow);
        }
    }
    public void UpdateTab(Tab tab)
    {
        for(int i = tab.StepList.Count - 1; i >= 0; i--)
        {
            TabStep tabStep = tab.StepList[i];
            if (tabStep.IsStop)
            {
                tab.StepList.RemoveAt(i);
            }
            else
            {
                tabStep.Update();
                if (tabStep.Tab != tab)
                {
                    UpdateTab(tabStep.Tab);
                }
            }
        }
    }
}

public class TabStep
{
    private Tab _tab;
    public Tab Tab
    {
        get
        {
            return _tab;
        }
    }
    private string _stepName;
    public string StepName
    {
        get {
            return _stepName;
        }
    }
    public bool IsStop;
    public bool IsAutoStop;

    public MethodInfo UpdateMethod;
    public Action UpdateAction;
    public bool InWork;
    public TabStep(Tab tab, string stepName)
    {
        _tab = tab;
        _stepName = stepName;
        IsStop = false;
        InWork = false;
    }
    public void Update()
    {
        if (UpdateAction != null)
        {
            UpdateAction();
        }
        else if (UpdateMethod != null)
        {
            UpdateMethod.Invoke(_tab, null);
        }
    }
}

public partial class Tab
{
    public string Name;
    public Tab ParentTab;
    public TabStep MainStep;
    protected bool IsAutoStop
    {
        set {
            MainStep.IsAutoStop = value;
        }
        get {
            return MainStep.IsAutoStop;
        }
    }
    protected static Dictionary<Type, Dictionary<string, MethodInfo>> _allMethodList = new Dictionary<Type, Dictionary<string, MethodInfo>>();
    protected Dictionary<string, MethodInfo> _methodList;
    public List<Action> method0List = new List<Action>();
    protected List<TabStep> _stepList = new List<TabStep>();
    protected System.Object[] _result = null;
    protected List<ProxyTab> _proxyList;

    private int _resultIndex = -1;
    public List<TabStep> StepList
    {
        get {
            return _stepList;
        }
    }
    protected virtual void Compile()
    {
        Type type = GetType();
        if (_allMethodList.TryGetValue(type, out _methodList))
        {
            return;
        }
        _methodList = new Dictionary<string, MethodInfo>();
        var methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in methods)
        {
            string stepName = method.Name;
            _methodList.Add(stepName, method);
        }
        _allMethodList.Add(type, _methodList);
    }
    public Tab()
    {
        Compile();
    }
    public virtual bool GetMethodIndex(string methodName, out int methodIndex)
    {
        methodIndex = -1;
        return false;
    }
    public virtual TabStep AutoCreateStep(Tab tab, string stepName, string updateName, bool force)
    {
        MethodInfo updateMethod = null;
        bool isAutoStop = true;
        if (tab._methodList != null && tab._methodList.TryGetValue(updateName, out updateMethod))
        {
            isAutoStop = false;
        }
        if (!isAutoStop || force)
        {
            TabStep tabStep = new TabStep(tab, stepName);
            tabStep.IsAutoStop = isAutoStop;
            tabStep.UpdateMethod = updateMethod;
            return tabStep;
        }
        return null;
    }
    public void Initstall()
    {
        TabStep tabStep = AutoCreateStep(this, "root", "update", true);
        this.MainStep = tabStep;
        _stepList.Add(tabStep);
    }
    private void CreateMainStep(Tab tab, string stepName)
    {
        TabStep tabStep = AutoCreateStep(tab, stepName, "update", true);
        _stepList.Add(tabStep);
        tab.MainStep = tabStep;
        tab.ParentTab = this;
        tab.Name = stepName;
    }
    public void Call(Tab tab, string stepName, params object[] param)
    {
        if (tab != null)
        {
            CreateMainStep(tab, stepName);
            tab.Start("s1", param);
        }
        else
        {
            DoNext(stepName);
        }
    }
    public void Start(string stepName, params object[] param)
    {
        MethodInfo mainMethod;
        if (_methodList != null && _methodList.TryGetValue(stepName, out mainMethod))
        {
            TabStep tabStep = AutoCreateStep(this, stepName, stepName + "_update", false);
            if (tabStep == null)
            {
                MainStep.InWork = true;
            }
            else
            {
                _stepList.Add(tabStep);
            }
            try
            {
                mainMethod.Invoke(this, param);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message:{0}\nStackTrace:{1}", e.Message, e.StackTrace));
            }
            if (tabStep == null)
            {
                MainStep.InWork = false;
                DoNext(stepName);
            }
        }
        else
        {
            OnStartError();
        }
    }
    protected void OnStartError()
    {
        if (MainStep.InWork)
        {
            return;
        }
        for (int i = _stepList.Count - 1; i >= 0; i--)
        {
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop)
            {
                return;
            }
        }
        if (MainStep.IsAutoStop)
        {
            Stop();
        }
    }
    protected virtual string GetNextStepName(string stepName)
    {
        string result;
        char lastChar = stepName[stepName.Length - 1];
        if (lastChar >= '0' && lastChar <= '9')
        {
            int num = Convert.ToInt32(lastChar);
            char[] stepNameArray = stepName.ToCharArray();
            if (num == 9)
            {
                char[] nextStepNameArray = new char[stepName.Length + 1];
                Array.Copy(stepNameArray, nextStepNameArray, stepName.Length - 1);
                nextStepNameArray[stepName.Length - 1] = '1';
                nextStepNameArray[stepName.Length] = '0';
                result = (new string(nextStepNameArray));
            }
            else
            {
                char[] nextStepNameArray = new char[stepName.Length];
                Array.Copy(stepNameArray, nextStepNameArray, stepName.Length - 1);
                nextStepNameArray[stepName.Length - 1] = Convert.ToChar(num + 1);
                result = new string(nextStepNameArray);
            }
        }
        else
        {
            result = (stepName + "1");
        }
        return result;
    }
    public void DoNext(string stepName, params object[] param)
    {
        NotifyProxyDoNext(stepName);
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName, param);
    }
    protected virtual void NotifyParentDoNext()
    {
        if (ParentTab != null)
        {
            ParentTab.DoNext(Name, _result);
        }
    }
    protected void Output(params object[] result)
    {
        _result = result;
    }
    public void Stop(string stepName = null)
    {
        if (MainStep.IsStop)
        {
            return;
        }
        if (stepName == null)
        {
            MainStep.IsStop = true;
            Final();
            NotifyParentDoNext();
            NotifyProxyDoNext();
        }
        else
        {
            for (int i = 0; i < _stepList.Count; i++)
            {
                TabStep tabStep = _stepList[i];
                if (tabStep.StepName == stepName)
                {
                    tabStep.IsStop = true;
                    break;
                }
            }
            DoNext(stepName);
        }
    }
    public void DoEvent(string eventName, params object[] param)
    {
        MethodInfo eventMethod;
        if (_methodList != null && _methodList.TryGetValue(eventName, out eventMethod))
        {
            try
            {
                eventMethod.Invoke(this, param);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Message:{0}\nStackTrace:{1}", e.Message, e.StackTrace));
            }
        }
    }
    protected void Notify(string eventName, params object[] param)
    {
        eventName = "event_" + eventName;
        for (int i = 0; i < _stepList.Count; i++)
        {
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop && tabStep.Tab != this)
            {
                tabStep.Tab.DoEvent(eventName, param);
            }
        }
    }
    protected void UpwardNotify(string eventName, params object[] param)
    {
        if (ParentTab != null && ParentTab.MainStep != null && !ParentTab.MainStep.IsStop)
        {
            eventName = "event_" + eventName;
            ParentTab.DoEvent(eventName, param);
        }
    }
    private bool HaveTabStep(string stepName)
    {
        for (int i = 0; i < _stepList.Count; i++)
        {
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop && tabStep.StepName == stepName)
            {
                return true;
            }
        }
        return false;
    }
    private void NotifyProxyDoNext(string stepName = null)
    {
        if (_proxyList == null)
        {
            return;
        }
        for(int i = _proxyList.Count - 1; i >= 0; i--)
        {
            ProxyTab tab = _proxyList[i];
            if (stepName == null || (tab.ProxyName != null && !HaveTabStep(tab.ProxyName)))
            {
                tab.Stop();
                _proxyList.Remove(tab);
            }
        }
    }
    public ProxyTab Proxy(string stepName = null)
    {
        if (MainStep.IsStop)
        {
            return null;
        }
        if (stepName != null)
        {
            if (!HaveTabStep(stepName))
            {
                return null;
            }
        }
        ProxyTab tab = new ProxyTab();
        tab.ProxyName = stepName;
        if (_proxyList == null)
        {
            _proxyList = new List<ProxyTab>();
        }
        _proxyList.Add(tab);
        return tab;
    }
    protected virtual void Final()
    {

    }
}

public partial class ProxyTab : Tab
{
    public string ProxyName;
    void s1()
    {
        IsAutoStop = false;
    }
}

public partial class EmptyTab : Tab
{

}

public partial class AliveTab : Tab
{
    void s1()
    {
        IsAutoStop = false;
    }
}

public partial class GameFlowTab : Tab
{
    int count = 0;
    void s1(int a, int b)
    {
        Debug.LogError("1X" + (a + b));
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            Call(new EmptyTab(), "c1");
        }
        stopwatch.Stop();
        Debug.LogError($"Execution time: {stopwatch.ElapsedMilliseconds}");
        GamePlayTab gamePlayTab = new GamePlayTab();
        Call(gamePlayTab, "s2", 1, 2);
        Call(gamePlayTab.Proxy("s1"), "t1");
    }
    void t2()
    {
        Debug.LogError("1t");
    }
    void s3(int a, int b)
    {
        Debug.LogError("2X" + (a + b));
    }
    void update()
    {
        if (count <= 5)
        {
            count++;
            if (count == 5)
            {
                Notify("stop");
            }
        }
    }
    protected override void Final()
    {
        Debug.LogError("3x");
    }
}

public partial class GamePlayTab : Tab
{
    void s1(int a, int b)
    {
        Debug.LogError("1y" + (a + b));
    }
    void s1_update()
    {
        Debug.LogError("1y_update");
    }
    void event_stop()
    {
        Debug.LogError("1y_stop");
        Stop("s1");
    }
    private void s2()
    {
        Output(3, 4);
        Debug.LogError("2y");
    }
    protected override void Final()
    {
        Debug.LogError("3y");
    }
}