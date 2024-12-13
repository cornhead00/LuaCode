using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DataStruct
{
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
        int[] nums = { 1, 3, 2 };
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
        int pivot = Partition(nums, left, right);
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
            while (j > i && nums[j] <= baseVal)
            {
                j--;
            }
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
        int startIndex = 0;
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
