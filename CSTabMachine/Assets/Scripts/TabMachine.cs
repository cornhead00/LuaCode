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
        //if (gameFlow != null)
        //{
        //    UpdateTab(gameFlow);
        //}
    }
    private void UpdateTab(Tab tab)
    {
        for (int i = tab.StepList.Count - 1; i >= 0; i--)
        {
            TabStep tabStep = tab.StepList[i];
            if (tabStep.IsStop)
            {
                tab.StepList.RemoveAt(i);
            }
            else
            {
                tabStep.Update();
                if (tabStep.BindTab != null)
                {
                    UpdateTab(tabStep.BindTab);
                }
            }
        }
    }

    private int[] TwoSum(int[] nums, int target)
    {
        int[] result = new int[2];
        
        for (int i = 0; i < nums.Length; i++)
        {
            for (int j = i + 1; j < nums.Length; j++)
            {
                if (i + j == target)
                {
                    result[0] = i;
                    result[1] = j;
                    return result;
                }
            }
        }
        return result;
    }
    private void Sort()
    {
        //�Ȱ���������ϲ�
        int[] nums = { 1, 3, 2 };
        //Ϊ��log n�ĸ��Ӷ��ÿ�������
        int left = 0;
        int length = nums.Length;
        int right = length - 1;
        //QuickSort(nums, left, right);
        InsertSort(nums);
        double result = 0;
        if (length % 2 == 0)
        {
            int mid = length / 2;
            result = (nums[mid] + nums[mid - 1]) * 1.0d / 2;
        }
        else
        {
            int mid = length / 2;
            result = nums[mid];
        }
    }
    private void InsertSort(int[] nums)
    {
        //ѭ������, ��С�����ţ�С����ǰ��
        for (int i = 1; i < nums.Length; i++)
        {
            int j = i - 1;
            int baseVal = nums[i];
            while (j >= 0 && nums[j] > baseVal)
            {
                (nums[j], nums[j + 1]) = (nums[j + 1], nums[j]);
                j--;
            }
        }
    }
    private void QuickSort(int[] nums, int left, int right)
    {
        if (left >= right)
            return;
        // �ڱ�����
        int pivot = Partition(nums, left, right);
        // �ݹ��������顢��������
        QuickSort(nums, left, pivot - 1);
        QuickSort(nums, pivot + 1, right);
    }
    private int Partition(int[] nums, int left, int right)
    {
        int i = left;
        int j = right;
        int baseVal = nums[i];
        while (i < j)
        {
            //������������ҵ���һ����baseС�Ľڵ�
            while (j > i && nums[j] <= baseVal)
            {
                j--;
            }

            //�������ұ����ҵ���һ����base���
            while (j > i && nums[i] >= baseVal)
            {
                i++;
            }
            (nums[j], nums[i]) = (nums[i], nums[j]);
        }
        (nums[i], nums[left]) = (nums[left], nums[i]);
        return i;
    }
    private string LongestPalindrome(string s)
    {
        char[] charArray = s.ToCharArray();
        int maxCount = 0;
        int index = 0;
        int startIndex = 0; // ��ʼ����
        while (charArray.Length - 1 - index > maxCount)
        {
            int count = 1;
            for (int i = index; i < charArray.Length; i++)
            {
                bool meet = true;
                int len = i - index + 1;
                for (int j = index; j <= index + (len - 1) / 2; j++)
                {
                    int revert = i - (j - index);
                    if (charArray[revert] != charArray[j])
                    {
                        meet = false;
                        break;
                    }
                }
                if (meet && count > maxCount)
                {
                    maxCount = count;
                    startIndex = index;
                }
                count++;
            }
            index++;
        }
        char[] subArray = new char[maxCount];
        Array.Copy(charArray, startIndex, subArray, 0, maxCount);
        string result = new string(subArray);
        return result;
    }
}

public class TabStep
{
    public Tab BindTab;
    private Tab _tab;
    private string _stepName;
    public string StepName
    {
        get {
            return _stepName;
        }
    }
    public bool IsStop;
    public bool IsAutoStop;

    private bool _isPreCompile;
    private MethodInfo updateFun;
    public TabStep(Tab tab, string stepName, bool isPreCompile)
    {
        _tab = tab;
        _stepName = stepName;
        _isPreCompile = isPreCompile;
        if (isPreCompile)
        {

        }
        else
        {
            if (BindTab != null)
            {
                BindTab.MethodList.TryGetValue("update", out updateFun);
            }
            else
            {
                tab.MethodList.TryGetValue(stepName + "_update", out updateFun);
            }
        }
        IsStop = false;
    }
    public void Update()
    {
        if (!_isPreCompile && updateFun != null)
        {
            if (BindTab != null)
            {
                updateFun.Invoke(BindTab, null);
            }
            else
            {
                updateFun.Invoke(_tab, null);
            }
        }
    }
}

public partial class Tab
{
    public string Name;
    public Tab ParentTab;
    public TabStep MainStep;
    protected static Dictionary<Type, Dictionary<string, MethodInfo>> _allMethodList = new Dictionary<Type, Dictionary<string, MethodInfo>>();
    protected Dictionary<string, MethodInfo> _methodList;
    protected List<TabStep> _stepList = new List<TabStep>();
    protected System.Object[] _result = null;
    private int _resultIndex = -1;
    public Dictionary<string, MethodInfo> MethodList
    {
        get {
            return _methodList;
        }
    }
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
    public virtual bool AutoStop(string updateName, string eventName)
    {
        return !_methodList.ContainsKey(updateName) && !_methodList.ContainsKey(eventName);
    }
    public virtual void Initstall()
    {
        TabStep tabStep = new TabStep(this, "root", false);
        tabStep.IsAutoStop = AutoStop("update", "event");
        this.MainStep = tabStep;
    }
    private void CreateMainStep(Tab tab, string stepName, bool isPreCompile)
    {
        TabStep tabStep = new TabStep(this, stepName, isPreCompile);
        tabStep.BindTab = tab;
        tabStep.IsAutoStop = AutoStop("update", "event");
        _stepList.Add(tabStep);
        tab.MainStep = tabStep;
        tab.ParentTab = this;
        tab.Name = stepName;
    }
    public void Call(Tab tab, string stepName, params object[] param)
    {
        CreateMainStep(tab, stepName, false);
        tab.Start("s1", param);
    }
    protected TabStep CreateStep(string stepName, bool isPreCompile)
    {
        bool isAutoStop = AutoStop(stepName + "_update", stepName + "_event");
        TabStep tabStep = new TabStep(this, stepName, isPreCompile);
        tabStep.IsAutoStop = isAutoStop;
        _stepList.Add(tabStep);
        return tabStep;
    }
    public void Start(string stepName, params object[] param)
    {
        MethodInfo mainMethod;
        if (_methodList.TryGetValue(stepName, out mainMethod))
        {
            TabStep tabStep = CreateStep(stepName, false);
            mainMethod.Invoke(this, param);
            if (tabStep.IsAutoStop)
            {
                tabStep.IsStop = true;
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
        for (int i = 0; i < _stepList.Count; i++)
        {
            TabStep tabStep = _stepList[i];
            if (!tabStep.IsStop)
            {
                return;
            }
        }
        if (MainStep.IsAutoStop)
        {
            Stop(null);
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
        //Type tupleType = result.GetType();
        //if (tupleType.IsGenericType)
        //{
        //    // ��ȡԪ���Ԫ������
        //    int elementCount = tupleType.GetGenericArguments().Length;

        //    // ����object����
        //    _result = new object[elementCount];

        //    // ��ȡԪ�������
        //    FieldInfo[] fields = tupleType.GetFields();
        //    int index = 0;
        //    foreach (var field in fields)
        //    {
        //        _result[index] = field.GetValue(result);
        //        index++;
        //    }
        //}
    }
    public void Stop(string stepName)
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
    protected virtual void Final()
    {

    }
}

public partial class GameFlowTab : Tab
{
    private void s1(int a, int b)
    {
        Debug.LogError("1X" + (a + b));
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            Call(new GamePlayTab(), "s2", 1, 3);
        }

        stopwatch.Stop();
        Debug.LogError($"Execution time: {stopwatch.ElapsedMilliseconds}");
    }
    protected override void Final()
    {
        Debug.LogError("3x");
    }
}

public partial class GamePlayTab : Tab
{
    private void s1(int a, int b)
    {
        //Debug.LogError("1y" + (a + b));
    }
    //private void s1_update()
    //{
    //    Debug.LogError("1y_update");
    //    Stop("s1");
    //}
    //private void s2()
    //{
    //    Debug.LogError("2y");
    //}
    //protected override void Final()
    //{
    //    Output(1, 3, 4);
    //    Debug.LogError("3y");
    //}
}


//warp
//����call,start,DoNext
