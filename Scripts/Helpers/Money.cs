using Godot;
using System;
using System.Collections.Generic;

public class Money
{
    /// <summary>
    /// Key: Update interval, Value[0]: Num intervals, Value[1]: currentTime
    /// </summary>
    public Dictionary<int, double[]> updateInterval = new Dictionary<int, double[]>();

    public Dictionary<int, Dictionary<string, Dictionary<double, double>>> moneyTracking = new Dictionary<int, Dictionary<string, Dictionary<double, double>>>();

    public double assets = 0; //TODO: Calculate the current market cash value of all structures, ships, etc.
    public double moneyTotal { get { return moneyTracking[GDate.Year]["Total"][updateInterval[GDate.Year][1]]; } }

    public static Money Standard { get { return new Money(0, new Dictionary<int, double>() { { GDate.Year, 50 }, { GDate.Month, 12 } }); } }

    public Money() { }

    /// <summary>
    /// Initiates Money
    /// </summary>
    /// <param name="startTime">The start date of the tracking in seconds</param>
    /// <param name="_updateInterval">Key: Update interval, Value: Num intervals</param>
    public Money(double startTime, Dictionary<int, double> _updateInterval)
    {
        //Set update interval
        foreach (KeyValuePair<int, double> update in _updateInterval)
        {
            updateInterval.Add(update.Key, new double[2] { update.Value, startTime });
            moneyTracking.Add(update.Key, new Dictionary<string, Dictionary<double, double>>());
            moneyTracking[update.Key].Add("Total", new Dictionary<double, double>());
            moneyTracking[update.Key]["Total"].Add(startTime, 0);
        }

        //Add total
        
    }

    public virtual void UpdateMoney()
    {
        foreach (KeyValuePair<int, double[]> update in updateInterval)
        {
            while (update.Value[1] + update.Key < World.Instance.Time)
            {
                update.Value[1] += update.Key;
                if (!moneyTracking[update.Key]["Total"].ContainsKey(update.Value[1]))
                {
                    moneyTracking[update.Key]["Total"].Add(update.Value[1], moneyTracking[update.Key]["Total"][update.Value[1] - update.Key]);
                }
            }

            while (true)
            {
                bool doBreak = true;
                foreach (KeyValuePair < string, Dictionary<double, double> > cat in moneyTracking[update.Key])
                {
                    
                    foreach (KeyValuePair< double, double > money in cat.Value)
                    {
                        if (money.Key < update.Value[1] - update.Key * update.Value[0])
                        {
                            moneyTracking[update.Key][cat.Key].Remove(money.Key);
                            doBreak = false;
                            break;
                        }
                    }
                    if (cat.Value.Count == 0)
                    {
                        doBreak = false;
                        moneyTracking[update.Key].Remove(cat.Key);
                        break;
                    }

                    if (!doBreak)
                        break;
                }
                if (doBreak)
                    break;
            }
        }
    }

    public virtual void StartingBalance(double amount)
    {
        foreach (KeyValuePair<int, double[]> update in updateInterval)
        {
            while (update.Value[1] + update.Key < World.Instance.Time)
            {
                update.Value[1] += update.Key;
                if (!moneyTracking[update.Key]["Total"].ContainsKey(update.Value[1]))
                {
                    moneyTracking[update.Key]["Total"].Add(update.Value[1], moneyTracking[update.Key]["Total"][update.Value[1] - update.Key]);
                }
            }
            if (!moneyTracking[update.Key]["Total"].ContainsKey(update.Value[1]))
            {
                moneyTracking[update.Key]["Total"].Add(update.Value[1], amount);
            }
            else
                moneyTracking[update.Key]["Total"][update.Value[1]] += amount;
        }
    }
    public void AddMoney(string cat, double amount)
    {
        foreach (KeyValuePair<int, double[]> update in updateInterval)
        {
            while (update.Value[1] + update.Key < World.Instance.Time)
            {
                update.Value[1] += update.Key;

                if (!moneyTracking[update.Key]["Total"].ContainsKey(update.Value[1]))
                {
                    moneyTracking[update.Key]["Total"].Add(update.Value[1], moneyTracking[update.Key]["Total"][update.Value[1] - update.Key]);
                }
            }

            moneyTracking[update.Key]["Total"][update.Value[1]] += amount;

            if (!moneyTracking[update.Key].ContainsKey(cat))
            {
                moneyTracking[update.Key].Add(cat, new Dictionary<double, double>());
                moneyTracking[update.Key][cat].Add(update.Value[1], 0);
            }
            if (!moneyTracking[update.Key][cat].ContainsKey(update.Value[1]))
            {
                moneyTracking[update.Key][cat].Add(update.Value[1], 0);
            }
            
            if (moneyTracking[update.Key][cat].Count == 0)
                moneyTracking[update.Key][cat][update.Value[1]] = amount;
            else
                moneyTracking[update.Key][cat][update.Value[1]] += amount;
            
        }
    }

    public virtual void SubtractMoney(string cat, double amount)
    {
        foreach (KeyValuePair<int, double[]> update in updateInterval)
        {
            while (update.Value[1] + update.Key < World.Instance.Time)
            {
                update.Value[1] += update.Key;
                if (!moneyTracking[update.Key]["Total"].ContainsKey(update.Value[1]))
                {
                    moneyTracking[update.Key]["Total"].Add(update.Value[1], moneyTracking[update.Key]["Total"][update.Value[1] - update.Key]);
                }
            }
            moneyTracking[update.Key]["Total"][update.Value[1]] -= amount;
            if (!moneyTracking[update.Key].ContainsKey(cat))
            {
                moneyTracking[update.Key].Add(cat, new Dictionary<double, double>());
                moneyTracking[update.Key][cat].Add(update.Value[1], 0);
            }
            if (!moneyTracking[update.Key][cat].ContainsKey(update.Value[1]))
            {
                moneyTracking[update.Key][cat].Add(update.Value[1], 0);
            }
            
            if (moneyTracking[update.Key][cat].Count == 0)
                moneyTracking[update.Key][cat][update.Value[1]] = - amount;
            else
                moneyTracking[update.Key][cat][update.Value[1]] -= amount;
            
        }
    }
}