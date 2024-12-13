
using System;
using System.Collections.Generic;

public partial class GameFlowTab : Tab
{
   static Dictionary<string, string> nextNameFindMap = new Dictionary<string, string>(){{"s1", "s2"}, {"Final", "Final1"}};

   static Dictionary<string, int> methodFindMap = new Dictionary<string, int>(){{"s1", 0}, {"Final", 0}};

   static List<Action> method0List = new List<Action>();
   static List<Action<System.Int32, System.Int32>> method1List = new List<Action<System.Int32, System.Int32>>();


    protected override void Compile()
    {
       method1List.Add(this.s1);
   method0List.Add(this.Final);

    }


    public override void Initstall()
    {
        TabStep tabStep = new TabStep(this, "root", true);
        tabStep.IsAutoStop = AutoStop("update", "event");
        this.MainStep = tabStep;
    }


    protected override void NotifyParentDoNext()
    {
        base.NotifyParentDoNextNoConvert();
    }


    protected override string GetNextStepName(string stepName)
    {
        string name = "";
        if (!nextNameFindMap.TryGetValue(stepName, out name))
        {
            name = base.GetNextStepName(stepName);
        }
        return name;
    }


    public override bool AutoStop(string updateName, string eventName)
    {
        return !methodFindMap.ContainsKey(updateName) && !methodFindMap.ContainsKey(eventName);
    }


    private bool MethodFind(string stepName)
    {
        if(methodFindMap.ContainsKey(stepName))
        {
            return true;
        }
        return false;
    }


    public override void Start(string stepName)
    {
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(stepName, out insertIndex))
        {
            TabStep tabStep = CreateStep(stepName, true);
            Action action = method0List[insertIndex];
            action.Invoke();
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

    public override void Start(string stepName, System.Int32 p1, System.Int32 p2)
    {
        int insertIndex = 0;
        if (methodFindMap.TryGetValue(stepName, out insertIndex))
        {
            TabStep tabStep = CreateStep(stepName, true);            
            Action<System.Int32, System.Int32> action = method1List[insertIndex];
            action.Invoke(p1, p2);
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

}