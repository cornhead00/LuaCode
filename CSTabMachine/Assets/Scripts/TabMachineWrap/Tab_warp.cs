
using System;
public partial class Tab
{
        private ValueTuple<System.Int32, System.Int32> _result1;

    public void Output(System.Int32 p1, System.Int32 p2)
    {
        _resultIndex = 1;
        _result1 = (p1, p2);
    }

    
    public void NotifyParentDoNextNoConvert()
    {
        if (ParentTab == null)
        {
            return;
        }
        if (_resultIndex == -1)
        {
            ParentTab.DoNext(Name);
            return;
        }
        
            else if (_resultIndex == 1)
            {
                ParentTab.DoNext(Name, _result1.Item1, _result1.Item2);
                return;
            }


    }

    
    
    public void Call(Tab tab, string stepName)
    {
        CreateMainStep(tab, stepName, true);
        tab.Start("s1");
    }

    public void Call(Tab tab, string stepName, System.Int32 p1, System.Int32 p2)
    {
        CreateMainStep(tab, stepName, true);
        tab.Start("s1", p1, p2);
    }

    
    public virtual void Start(string stepName)
    {
    }

    public virtual void Start(string stepName, System.Int32 p1, System.Int32 p2)
    {
    }

    
    public void DoNext(string stepName)
    {
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName);
    }

    public void DoNext(string stepName, System.Int32 p1, System.Int32 p2)
    {
        string nextStepName = GetNextStepName(stepName);
        Start(nextStepName, p1, p2);
    }

}
