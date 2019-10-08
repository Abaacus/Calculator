using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  this class initializes all of my cutsom data types used in the calculator.

public interface Inputs { }    // this is an interface called inputs. All of my custom data types parent from this interface so a list can be created that stores these data types. It has no other function

[Serializable]
public class Operator : Inputs  // this data type simply holds an operator type
{
    public enum OperatorType { mul, div, add, sub }   // creates an enum for operator types
    public OperatorType operatorType;   // creates a variable to hold this object's enum value

    public Operator(OperatorType operatorType)  // constructor takes operator type
    {
        this.operatorType = operatorType;
    }
}

[Serializable]
public class CalculationList : Inputs //  this data type is used to hold lists of inputs that the bedmas brain will scroll through 
{
    public List<Inputs> calculationList = new List<Inputs>();   // this list holds a list of Inputs to be calculated
    public List<int> ID;    // this list holds the indexs used when navigating to this particular list
    public string IDcode;   // this string holds the object's name, strictly used in the Debug Log

    public CalculationList(List<int> ID)    // unity constructor, takes an ID list and converts into an IDcode automatically
    {
        this.ID = ID;   // sets the object's ID to the ID inputted
        for (int i = 0; i < ID.Count; i++) // loops through each value in the ID list
        {
            IDcode = ID[i] + IDcode; // adds current value to the front of the current string
        }
    }

    public CalculationList(List<int> ID, string IDcode) // this constructor is specifically made to give an object a special hardcoded name (like "mainMemory")
    {
        this.ID = ID;   // sets the object's ID to the ID inputted
        this.IDcode = IDcode;   // sets the object's IDcode to the IDcode inputted
    }
}

[Serializable]
public class Number : Inputs    // this data type holds a decimal value and the appropiate functions to add and remove digits from it's internal value
{
    public bool isDecimal;  // when adding digits, are we adding to the decimals or adding whole digits? this bool holds that
    public decimal value; // the numerical decimal value that the number has (or the actual number held here)
    public decimal lim0a05 = 0.4999999999999999999999999999m;   // computational representation of lim 0 -> 0.5
    public bool negative; // holds whether or not the number is negative

    public Number(decimal value, bool negative)  // constructor takes in the decimal value the number will have, and whether or not this number should be negative
    {
        this.negative = negative;
        if (negative)
        {   // if this number is to be negative, multiply the inputted value by -1
            value *= -1m;
        }

        this.value = value;
    }

    public Number(decimal value)
    {   // this constructor automatically decides if a number is negative or positive based on the inputted value
        this.value = value;
        if (value < 0)
        {   // if the value is less than 0, the number is negative
            negative = true;
        }
        else
        {   // if it isn't less than 0, than it's not negative
            negative = false;
        }
    }

    public void AddDigit(int x) // method used to add digits to this number's numerical value. x is the new digit we are adding
    {
        if (!isDecimal)
        {   // we are't adding a decimal? run this
            if (negative)
            {   // if the number is negative, multiply the value we will be adding by -1
                x *= -1;
            }
            // now, we multiply the current value by 10, then add the new digit. Simple. (if the number is negative, we will be adding a negative, so the number should decrease accordingly)
            value = (value * 10m) + x; 
        }

        else
        { // we are adding decimals? time for complex algorithms
            int wholeValue = (int)decimal.Round(value - lim0a05);    // this takes the number's current value, subtracts almost 0.5, and then rounds to the nearest int. This returns the whole number (x) by shifting all values from x.00 - x.99 into the rounding range of x (x-1.51 to x.49)
            decimal decimalValue = value - wholeValue;  // now we subtract the whole numbers from the main value, leaving behind 0.xxxxx, our decimal value.                              |
            int decimalPlace;  // this will store what power we need to raise our digit by to add the correct decimal point.                                                               `-> *** SIDE NOTE *** this trick needs the biggest number possible to work, decimals such as 0.3 are too small (x.99 - 0.3 = x.69, which rounds to x+1).
            if (decimalValue == 0)  // if the decimal value is equal to zero, the string we create will lack a decimal place, and we have to account for that                                  0.5 seems to work(x.99 - 0.5 = x.49, which rounds to x), but it falls apart at lower numbers (x.00 - 0.5 = x-1.5, which rounds to x-1), no good.
            {   // if there aren't any decimals, the decimal place we will placing our number at is 1 decimal place                                                                            The solution to this problem is to take the limit as 0 -> 0.5, or get a number as close as possible to 0.5. That's what lim0a05 stands for (the LIMit as 0 Approches 0.5)
                decimalPlace = 1;
            }           
            else
            {   // if there are decimals, the decimal place we will placing our number at is equal to the string's length - 1 (to account for the decimal point in the string). E.G. If we have 3.21, decimalPlace = ("3.21".Length() - 1)  
                decimalPlace = decimalValue.ToString("G29").Length - 1;                                                                                                                                   // decimalPlace = 4 - 1
            }      // *** NOTE *** "G29" is a special parameter for ToString() that removes any trailng and leading 0s, giving us a string we can actually work with                                         decimalPlace = 3

            if (negative)
            {   // if the number is negative, multiply the digit we are adding by  -1 (this will subtract the appropriate value, increasing the number's absolute value)
                x *= -1;
            }
                // here I add the new digit to the current value (finally!) I raise 10^-decimalPlace, giving me a fraction like 1/100, or 0.01, one more decimal place than the number currently has. If we multiply this decimal by the number we are adding, we get the numerical value we need to add
            value += Convert.ToDecimal(x * Mathf.Pow(10f, -decimalPlace)); 
        }
    }

    public void RemoveLastDigit()   // method used to remove digits from this number's numerical value
    {
        if (!isDecimal)
        {   // if we aren't dealing with decimals
            decimal lastDigit = Convert.ToDecimal(Mathf.Abs((float)value) % 10);  // save the last digit of the number (found by subtracting ten until it's no longer possible and returning the remaing value)
            if (negative)                                                           // *** SIDE NOTE ***   Mathf.Abs() is used to get the absolute value of our number, ensuring nothing goes screwy with negatives
            {   // if the number is negative, multiply the digit we are subtracting by -1
                lastDigit *= -1;
            }

            value -= lastDigit; // subtract the last digit away
            value /= 10;    // get rid of the 0 left behind by dividing the number by 10
        }
        else
        {
            int wholeValue = (int)decimal.Round(value - lim0a05);    // this takes the number, subtracts almost 0.5, and then rounds to the nearest int
            decimal decimalValue = value - wholeValue;  // now we subtract the whole numbers from the main value, leaving behind 0.xxxxx, our decimal values
            int lastdecimalPlace = decimalValue.ToString("G29").Length - 1;    // the decimal place we will removing our number from is equal to the string's length
            decimal decimalPlace = Convert.ToDecimal(Mathf.Pow(10f, -(lastdecimalPlace - 1))); // this is the decimal place we will be subtracting from
            int lastDigit = (int)(Mathf.Abs((float)(value * Convert.ToDecimal(Mathf.Pow(10, lastdecimalPlace - 1)))) % 10); // save what the last digit is  (a combination of Mathf.Abs() and decimalPlace powers, as used before)
            if (negative)
            {   // if the number is negative, multiply the value we are taking away by -1
                lastDigit *= -1;
            }
                // remove the last digit at the correct decimal place. the computer will automatically remove the trailing 0 because it isn't a sig fig
            value -= lastDigit * decimalPlace;    
        }
    }
}


[Serializable]
public class Function : Inputs  // this data type is used to hold a function type that will be applied to it's internal calculation list 
{
    public enum FunctionType { asin, acos, atan, log, ln, sin, cos, tan, sq, sqrt}; // creating an enum for function types
    public FunctionType functionType;                                               // creates a variable to hold this object's enum value
    public CalculationList embededCalculationList;    // creates a calculation list (see above) that when calculated to one number will be evaluated based on it's funciton type
    public bool failedCalculation;
        // the above bool stores whether or not a function is possible, such as ASIN(ø) when ø > 1, which throw an error in calculating

    public Function(FunctionType functionType, List<int> ID)    // constructor takes an ID list for it's embededCalculationList, and the enum for it's function type
    {
        this.functionType = functionType;   // sets the objects functionType to the enum inputted
        embededCalculationList = new CalculationList(ID);   // creates an embededList with an ID equal to the inputted ID 
    }

    public float Calculate()    // evaluates the value in it's embeded list based of it's function type
    {
        if (embededCalculationList.calculationList.Count == 1)  // it can only evaluate if there is one number in the list
        {
            if (embededCalculationList.calculationList[0].GetType() == typeof(Number))
            {
                float x = (float)(embededCalculationList.calculationList[0] as Number).value;  // sets the variable x to the Number's value at the only index
                float input = x;    // stores x at it's intial state for debugging purposes

                switch (functionType)   // performs something based off of it's function type
                {
                    default:
                        Debug.Log("No function type given. Defualt: x^2");
                        x = Mathf.Pow(x, 2);    // in a default case, raises x to the power of 2 using a built in function
                        break;
                    case FunctionType.asin:
                        x = Mathf.Asin(x);       // if the function type is asin, set x to the asin of x
                        break;

                    case FunctionType.acos:
                        x = Mathf.Acos(x);       // AND ETC
                        break;

                    case FunctionType.atan:
                        x = Mathf.Atan(x);       // AND ETC
                        break;

                    case FunctionType.log:
                        x = Mathf.Log10(x);      // AND ETC
                        break;

                    case FunctionType.ln:
                        x = Mathf.Log(x);        // AND ETC
                        break;

                    case FunctionType.sin:
                        x = Mathf.Sin(x);        // AND ETC
                        break;

                    case FunctionType.cos:
                        x = Mathf.Cos(x);        // AND ETC
                        break;

                    case FunctionType.tan:
                        x = Mathf.Tan(x);        // AND ETC
                        break;

                    case FunctionType.sq:
                        x = Mathf.Pow(x, 2);     // AND ETC
                        break;

                    case FunctionType.sqrt:
                        x = Mathf.Sqrt(x);       // AND ETC
                        break;
                }

                Debug.Log(functionType + "(" + input + ") = " + x); // debug logs the result of the calculation

                if (float.IsNaN(x))
                {   // if the value returned NaN (not a number) due to a mathematical impossibility
                    Debug.LogWarning("Math Error: Cannot calculate " + functionType + " of " + input + ".");
                    failedCalculation = true;   // debug log the error and set failedCalculatuion to true, return 0 (it's easier to just return something, and then print Math Error later)
                    return 0;
                }
                if (float.IsInfinity(x))
                {
                    Debug.LogWarning("Math Error: " + functionType + " of " + input + " returns " + x);
                    failedCalculation = true;   // debug log the error and set failedCalculatuion to true, return 0
                    return 0;
                }
                return x;
            }
            // if the first index isn't a number, debug log an error, set failedCalculation to true, and return a 0 (as before)
            Debug.LogWarning("Value in embededCalculationList[0] != typeof(Number)."); 
            failedCalculation = true;
            return 0;
        }
        // if the length isn't 1, debug log an error, set failedCalculation to true, and return a 0 
        Debug.LogWarning("CalculationList is too long."); 
        failedCalculation = true;
        return 0;
    }
}