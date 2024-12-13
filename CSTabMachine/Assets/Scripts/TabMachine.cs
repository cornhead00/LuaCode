using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CompileTabMachine
{
    [MenuItem("TabMachine/Compile Code", false, 1)]
    public static void CompileAll()
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes().Where(t => t.BaseType == typeof(Tab)))
        .ToArray();
        Dictionary<string, int> paramsTypeMap = new Dictionary<string, int>();
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            CompileSingle(type, paramsTypeMap);
        }
        string ouputResultStr = "";
        string ouputMethods = CreateOutputMethodsStr(paramsTypeMap, out ouputResultStr);
        string notifyParentDoNextMethod = CreateNotifyParentDoNextMethodStr(paramsTypeMap);
        string callMethods = CreateCallMethodsStr(paramsTypeMap);
        string starMethods = CreateStarMethodsStr(paramsTypeMap);
        string doNextMethods = CreateDoNextMethodsStr(paramsTypeMap);

        string fileContent = $@"
using System;
public partial class Tab
{{
    {ouputResultStr}
    {notifyParentDoNextMethod}
    {ouputMethods}
    {callMethods}
    {starMethods}
    {doNextMethods}
}}
";
        File.WriteAllText("Assets/Scripts/Tab_warp.cs", fileContent);
    }
    private static string CreateOutputMethodsStr(Dictionary<string, int> paramsTypeMap, out string outputValStr)
    {
        StringWriter stringWriter = new StringWriter();
        StringWriter stringWriter1 = new StringWriter();
        foreach (KeyValuePair<string, int> pairs in paramsTypeMap)
        {
            if (pairs.Key.IndexOf(',') != -1)
            {
                stringWriter.WriteLine($"    private ValueTuple<{pairs.Key}> _result{pairs.Value};");
            }
            string[] typesList = pairs.Key.Split(',');
            string strPrams = "";
            string strInvokePrams = "";
            for (int i = 0; i < typesList.Length; i++)
            {
                strPrams += (typesList[i] + " p" + (i + 1));
                strInvokePrams += ("p" + (i + 1));
                if (i < typesList.Length - 1)
                {
                    strPrams += (",");
                    strInvokePrams += (", ");
                }
            }
            string outputMethod = $@"
    public void Output({strPrams})
    {{
        _resultIndex = {pairs.Value};
        _result{pairs.Value} = ({strInvokePrams});
    }}
";
            stringWriter.Write(outputMethod);
        }

        outputValStr = stringWriter.ToString();
        stringWriter.Dispose();
        string outputMethods = stringWriter1.ToString();
        stringWriter1.Dispose();
        return outputMethods;
    }
    private static string CreateCallMethodsStr(Dictionary<string, int> paramsTypeMap)
    {
        StringWriter stringWriter = new StringWriter();

        string callMethod = $@"
    public void Call(Tab tab, string stepName)
    {{
        CreateMainStep(tab, stepName, true);
        tab.Start(""s1"");
    }}
";
        stringWriter.Write(callMethod);
        foreach (KeyValuePair<string, int> pairs in paramsTypeMap)
        {
            string[] typesList = pairs.Key.Split(',');
            string strPrams = "";
            string strInvokePrams = "";
            for (int i = 0; i < typesList.Length; i++)
            {
                strPrams += (typesList[i] + " p" + (i + 1));
                strInvokePrams += ("p" + (i + 1));
                if (i < typesList.Length - 1)
                {
                    strPrams += (",");
                    strInvokePrams += (", ");
                }
            }
            callMethod = $@"
    public void Call(Tab tab, string stepName, {strPrams})
    {{
        CreateMainStep(tab, stepName, true);
        tab.Start(""s1"", {strInvokePrams});
    }}
";
            stringWriter.Write(callMethod);
        }
        string callMethods = stringWriter.ToString();
        stringWriter.Dispose();
        return callMethods;
    }
    private static string CreateStarMethodsStr(Dictionary<string, int> paramsTypeMap)
    {
        string startMethod = $@"
    public virtual void Start(string stepName)
    {{
    }}
";
        StringWriter stringWriter = new StringWriter();
        stringWriter.Write(startMethod);
        foreach (KeyValuePair<string, int> pairs in paramsTypeMap)
        {
            string[] typesList = pairs.Key.Split(',');
            string strPrams = "";
            string strInvokePrams = "";
            for (int i = 0; i < typesList.Length; i++)
            {
                strPrams += (typesList[i] + " p" + (i + 1));
                strInvokePrams += ("p" + (i + 1));
                if (i < typesList.Length - 1)
                {
                    strPrams += (",");
                    strInvokePrams += (", ");
                }
            }
            startMethod = $@"
    public virtual void Start(string stepName, {strPrams})
    {{
    }}
";
            stringWriter.Write(startMethod);
        }

        string starMethods = stringWriter.ToString();
        stringWriter.Dispose();
        return starMethods;
    }
    private static string CreateDoNextMethodsStr(Dictionary<string, int> paramsTypeMap)
    {
        string doNextMethod = $@"
    public void DoNext(string stepName)
    {{
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName);
    }}
";
        StringWriter stringWriter = new StringWriter();
        stringWriter.Write(doNextMethod);
        foreach (KeyValuePair<string, int> pairs in paramsTypeMap)
        {
            string[] typesList = pairs.Key.Split(',');
            string strPrams = "";
            string strInvokePrams = "";
            for (int i = 0; i < typesList.Length; i++)
            {
                strPrams += (typesList[i] + " p" + (i + 1));
                strInvokePrams += ("p" + (i + 1));
                if (i < typesList.Length - 1)
                {
                    strPrams += (",");
                    strInvokePrams += (", ");
                }
            }
            doNextMethod = $@"
    public void DoNext(string stepName, {strPrams})
    {{
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName, {strInvokePrams});
    }}
";
            stringWriter.Write(doNextMethod);
        }

        string doNextMethods = stringWriter.ToString();
        stringWriter.Dispose();
        return doNextMethods;
    }
    private static string CreateNotifyParentDoNextMethodStr(Dictionary<string, int> paramsTypeMap)
    {
        StringWriter stringWriter = new StringWriter();
        foreach (KeyValuePair<string, int> pairs in paramsTypeMap)
        {
            string[] typesList = pairs.Key.Split(',');
            string conditionInfo = "ParentTab.DoNext(Name, ";
            for(int i = 1; i <= typesList.Length; i++)
            {
                conditionInfo += "_result" + pairs.Value + ".Item" + i;
                if (i != typesList.Length)
                {
                    conditionInfo += ", ";
                }
                else
                {
                    conditionInfo += ");";
                }
            }
            string condition = $@"
            else if (_resultIndex == {pairs.Value})
            {{
                {conditionInfo}
                return;
            }}
";
            stringWriter.WriteLine(condition);
        }

        string notifyParentDoNextNoConvertInfo = stringWriter.ToString();
        stringWriter.Dispose();

        string notifyParentDoNextNoConvert = $@"
    public void NotifyParentDoNextNoConvert()
    {{
        if (ParentTab == null)
        {{
            return;
        }}
        if (_resultIndex == -1)
        {{
            ParentTab.DoNext(Name);
            return;
        }}
        {notifyParentDoNextNoConvertInfo}
    }}
";


        return notifyParentDoNextNoConvert;
    }
    private static string GetNextStepName(string stepName)
    {
        ValueTuple<int, int> a = (1, 0);


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
    private static string GetPreStepName(string stepName)
    {
        string result = "";
        char lastChar = stepName[stepName.Length - 1];
        if (lastChar >= '1' && lastChar <= '9')
        {
            int num = Convert.ToInt32(lastChar);
            char[] stepNameArray = stepName.ToCharArray();
            if (num == 9)
            {
                char[] nextStepNameArray = new char[stepName.Length];
                Array.Copy(stepNameArray, nextStepNameArray, stepName.Length - 1);
                nextStepNameArray[stepName.Length - 1] = Convert.ToChar(num - 1);
                result = (new string(nextStepNameArray));
            }
        }
        return result;
    }
    private static string CreateNextNameStr(MethodInfo[] methods)
    {
        string nextNameInfoStr = "";
        Dictionary<string, string> nameMap = new Dictionary<string, string>();
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            Type returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                string preName = GetPreStepName(method.Name);
                string nextName = GetNextStepName(method.Name);
                if (preName != "" && !nameMap.ContainsKey(preName))
                {
                    nextNameInfoStr += ($"{{\"{preName}\", \"{method.Name}\"}}");
                    nameMap.Add(preName, method.Name);
                    if (i != methods.Length - 1 || !nameMap.ContainsKey(nextName))
                    {
                        nextNameInfoStr += ", ";
                    }
                }
                if (!nameMap.ContainsKey(nextName))
                {
                    nextNameInfoStr += ($"{{\"{method.Name}\", \"{nextName}\"}}");
                    nameMap.Add(method.Name, nextName);
                    if (i != methods.Length - 1)
                    {
                        nextNameInfoStr += ", ";
                    }
                }
            }
        }
        StringWriter stringWriter = new StringWriter();
        stringWriter.WriteLine($"   static Dictionary<string, string> nextNameFindMap = new Dictionary<string, string>(){{{nextNameInfoStr}}};");
        string nextNameStr = stringWriter.ToString();
        stringWriter.Dispose();
        return nextNameStr;
    }
    private static string CreateMethodListStr(Dictionary<string, int> singleParamsTypeMap)
    {
        StringWriter stringWriter = new StringWriter();
        stringWriter.WriteLine($"   static List<Action> method0List = new List<Action>();");
        foreach (KeyValuePair<string, int> pairs in singleParamsTypeMap)
        {
            stringWriter.WriteLine($"   static List<Action<{pairs.Key}>> method{pairs.Value}List = new List<Action<{pairs.Key}>>();");
        }
        string result = stringWriter.ToString();
        stringWriter.Dispose();
        return result;
    }
    private static string CreateMethodFindStr(Dictionary<string, int> singleParamsTypeMap, Dictionary<string, string> methodTypeMap, MethodInfo[] methods, out string compileStr)
    {
        StringWriter stringWriter = new StringWriter();
        string methodFindInfoStr = "";
        Dictionary<int, int> methodMap = new Dictionary<int, int>();
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            Type returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                int paramsTypeIndex;
                string mapName;
                string paramsTypeStr;
                if (methodTypeMap.TryGetValue(method.Name, out paramsTypeStr))
                {
                    if (singleParamsTypeMap.TryGetValue(paramsTypeStr, out paramsTypeIndex))
                    {
                    }
                    else
                    {
                        paramsTypeIndex = 0;
                    }
                }
                else
                {
                    paramsTypeIndex = 0;
                }
                int mothodIndex;
                if (methodMap.TryGetValue(paramsTypeIndex, out mothodIndex))
                {

                }
                else
                {
                    mothodIndex = 0;
                }
                methodFindInfoStr += ($"{{\"{method.Name}\", {mothodIndex}}}");
                if (i != methods.Length - 1)
                {
                    methodFindInfoStr += ", ";
                }
                mothodIndex++;
                methodMap[paramsTypeIndex] = mothodIndex;
                mapName = "method" + paramsTypeIndex + "List";

                stringWriter.WriteLine($"   {mapName}.Add(this.{method.Name});");
            }
        }
        string compileInfoStr = stringWriter.ToString();
        stringWriter.Dispose();

        StringWriter stringWriter2 = new StringWriter();
        stringWriter2.WriteLine($"   static Dictionary<string, int> methodFindMap = new Dictionary<string, int>(){{{methodFindInfoStr}}};");
        string methodFindStr = stringWriter2.ToString();
        stringWriter2.Dispose();

        compileStr = $@"
    protected override void Compile()
    {{
    {compileInfoStr}
    }}
";
        return methodFindStr;

    }
    private static void CompileSingle(Type type, Dictionary<string, int> paramsTypeMap)
    {
        Dictionary<string, int> singleParamsTypeMap = new Dictionary<string, int>();
        Dictionary<string, string> methodTypeMap = new Dictionary<string, string>();
        var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        int index = 0;
        foreach (var method in methods)
        {
            ParameterInfo[] parameters = method.GetParameters();
            Type returnType = method.ReturnType;
            if (parameters.Length > 0 && returnType == typeof(void))
            {
                string typeKey = "";
                index++;
                for (int i = 0; i < parameters.Length; i++)
                {
                    string strType = parameters[i].ParameterType.ToString();
                    typeKey += strType;
                    if (i != parameters.Length - 1)
                    {
                        typeKey += ", ";
                    }
                }
                if (!paramsTypeMap.ContainsKey(typeKey))
                {
                    paramsTypeMap.Add(typeKey, index);
                }
                if (!singleParamsTypeMap.ContainsKey(typeKey))
                {
                    singleParamsTypeMap.Add(typeKey, index);
                }
                if (!methodTypeMap.ContainsKey(method.Name))
                {
                    methodTypeMap.Add(method.Name, typeKey);
                }
            }
        }


        string nextNameStr = CreateNextNameStr(methods);
        string methodListStr = CreateMethodListStr(singleParamsTypeMap);
        string compileStr = "";
        string methodFindStr = CreateMethodFindStr(singleParamsTypeMap, methodTypeMap, methods, out compileStr);
        string methodFind = $@"
    private bool MethodFind(string stepName)
    {{
        if(methodFindMap.ContainsKey(stepName))
        {{
            return true;
        }}
        return false;
    }}
";

        string autoStop = $@"
    public override bool AutoStop(string updateName, string eventName)
    {{
        return !methodFindMap.ContainsKey(updateName) && !methodFindMap.ContainsKey(eventName);
    }}
";
        string initstall = $@"
    public override void Initstall()
    {{
        TabStep tabStep = new TabStep(this, ""root"", true);
        tabStep.IsAutoStop = AutoStop(""update"", ""event"");
        this.MainStep = tabStep;
    }}
";

        string nextStepName = $@"
    protected override string GetNextStepName(string stepName)
    {{
        string name = """";
        if (!nextNameFindMap.TryGetValue(stepName, out name))
        {{
            name = base.GetNextStepName(stepName);
        }}
        return name;
    }}
";
        
    string notifyParentDoNext = $@"
    protected override void NotifyParentDoNext()
    {{
        base.NotifyParentDoNextNoConvert();
    }}
";

        string methodName = "method0List";
        string startMethod = $@"
    public override void Start(string stepName)
    {{
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(stepName, out insertIndex))
        {{
            TabStep tabStep = CreateStep(stepName, true);
            Action action = {methodName}[insertIndex];
            action.Invoke();
            if (tabStep.IsAutoStop)
            {{
                tabStep.IsStop = true;
                DoNext(stepName);
            }}
        }}
        else
        {{
            OnStartError();
        }}
    }}
";
        StringWriter stringWriter = new StringWriter();
        stringWriter.Write(startMethod);
        foreach (KeyValuePair<string, int> pairs in singleParamsTypeMap)
        {
            methodName = "method" + pairs.Value + "List";
            string[] types = pairs.Key.Split(',');
            string strPrams = "";
            string strInvokePrams = "";
            for (int i = 0; i < types.Length; i++)
            {
                strPrams += (types[i] + " p" + (i + 1));
                strInvokePrams += ("p" + (i + 1));
                if (i < types.Length - 1)
                {
                    strPrams += (",");
                    strInvokePrams += (", ");
                }
            }
            startMethod = $@"
    public override void Start(string stepName, {strPrams})
    {{
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(stepName, out insertIndex))
        {{
            TabStep tabStep = CreateStep(stepName, true);            
            Action<{pairs.Key}> action = {methodName}[insertIndex];
            action.Invoke({strInvokePrams});
            {{
                tabStep.IsStop = true;
                DoNext(stepName);
            }}
        }}
        else
        {{
            OnStartError();
        }}
    }}
";
            stringWriter.Write(startMethod);
        }
        string startMethods = stringWriter.ToString();
        stringWriter.Dispose();

        string className = type.Name;
        string fileContent = $@"
using System;
using System.Collections.Generic;

public partial class {className} : Tab
{{
{nextNameStr}
{methodFindStr}
{methodListStr}
{compileStr}
{initstall}
{notifyParentDoNext}
{nextStepName}
{autoStop}
{methodFind}
{startMethods}
}}";
        File.WriteAllText("Assets/Scripts/"+className+"_warp.cs", fileContent);
        //FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        //for (int i = 0; i < fields.Length; i++)
        //{
        //    FieldInfo field = fields[i];
        //    if (field.Name == "_outPut" && field.FieldType.IsGenericType)
        //    {
        //        Type[] tupleElementTypes = field.FieldType.GetGenericArguments();
        //    }
        //}
    }
}

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
