using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedmasBrain    // this class does all of the bedmass work, you give it a calculation list and it will evaluate it to one number
{
    Function currentFunc;   // this variable holds the current Function instance we will be working with (saves me from having to cast it repeatedly)
    Operator currentOper;   // this variable holds the current Operator instance
    Number currentNum1; // this variable holds the first number we will be using in an operation
    Number currentNum2; // this variable holds the second number we will use in an operation

    public bool failedCalculation;  // this variable stores whether or not an error came up that would cause the calculation to be incorrect

    public void Calculate(CalculationList mainEquation) // this is the public method accessed by other classes to start calculating the memory
    {
        failedCalculation = false;  // sets the bool to true (it can't have failed at the beginning)
        FindBrackets(mainEquation); // calls this custom method using the inputted calculation list
            // by here the calculations should be complete,
        if (mainEquation.calculationList.Count != 1)
        {    // if the main list isn't reduced to one value, then set the calculation to failed
            Debug.Log("Error. Too many Inputs left it list");
            failedCalculation = true;
        }

        if (mainEquation.calculationList[0].GetType() != typeof(Number))
        {   // if the remaing value isn't a number, then set the calculation to failed
            Debug.Log("Error. Final Input isn't a number");
            failedCalculation = true;
        }

        if (!failedCalculation)
        {    // if the calculation didn't fail, print the remaining number
            Debug.Log("Final value: " + (mainEquation.calculationList[0] as Number).value);
        }
        else
        {   // if it did fail, debug log "Error"
            Debug.LogWarning("Math Error");
        }
    } 

    void FindBrackets(CalculationList calculationList)  // this method scrolls through the inputted calculation list, finding other calculation lists inside of it and then entering it
    {
        List<Inputs> currentCalculationList = calculationList.calculationList;  // creates a new variable that holds the list of actual inputs
        Debug.Log("Finding brackets in " + calculationList.IDcode); // debug logs which list we are performing what stage in
        for (int i = 0; i < currentCalculationList.Count; i++)  // loops through all indexes in the list
        {
            if (currentCalculationList[i].GetType() == typeof(CalculationList)) // if the current index is a calculation list, run below
            {
                Debug.Log("Brackets found");    // debug logs that a list was found
                if ((currentCalculationList[i] as CalculationList).calculationList.Count != 0)  // if the list actually has something in it, let's look in it it
                {
                    Debug.Log("Entering list " + (currentCalculationList[i] as CalculationList).IDcode);    // debug log that we are entering it
                    FindBrackets(currentCalculationList[i] as CalculationList); // run this method in the new list we found
                    if ((currentCalculationList[i] as CalculationList).calculationList[0].GetType() == typeof(Number))  // at this point, a proper list will be reduceded to one value. if it has, run this code
                    {
                        currentCalculationList[i] = new Number(((currentCalculationList[i] as CalculationList).calculationList[0] as Number).value);    // replaces the index holding the list with a number equal to the list's remaing number
                        Debug.Log("Compressing list into type(Num) with a value of " + (currentCalculationList[i] as Number).value);    // debug log the new number being created
                    }
                    else
                    {
                        Debug.Log("Math Error. No number available");   // if there isn't one number in the list, something went wrong. Debug log an error
                        failedCalculation = true;   // set failed calculation to true (something went wrong to trigger this)
                        currentCalculationList[i] = new Number(0);  // replace the index with a 0 (to avoid crashes, I explained this in input types)
                    }
                }
                else    // if the list is empty, just replace it with a zero
                {
                    currentCalculationList[i] = new Number(0);
                }
            }
        }

        CalculateFunctions(calculationList);    // if there are no more calculation lists in the current list, start looking for functions in it
    }

    void CalculateFunctions(CalculationList calculationList)    // this method looks for functions in the inputted list, enters them to calculate them into one number, and then applies the function to them
    {
        List<Inputs> currentCalculationList = calculationList.calculationList;  // creates a new variable to hold the list of inputs rather than the object 
        Debug.Log("Finding functions in " + calculationList.IDcode);    // debug logs which method we are performing on what list
        for (int i = 0; i < currentCalculationList.Count; i++)  // index through each value
        {
            if (currentCalculationList[i].GetType() == typeof(Function))    // if the index is a Function
            {
                Debug.Log("Function found");
                if((currentCalculationList[i] as Function).embededCalculationList.calculationList.Count != 0)
                {
                    Debug.Log("Entering embeded list"); // debug log what we found
                    FindBrackets((currentCalculationList[i] as Function).embededCalculationList);   // enter it's embeded calculation list, and look for brackets in it
                }
                else
                {
                    Debug.Log("Function is empty");
                    failedCalculation = true;
                }

                Debug.Log("Calculating function");  // at this stage, the function should have one numnber in it
                currentFunc = currentCalculationList[i] as Function;    // set currentFunc to the our current function (to avoid repeated casting) 
                if (currentFunc.embededCalculationList.calculationList.Count == 1)
                {   // if the function has one value left in it, continue
                    if (currentFunc.embededCalculationList.calculationList[0].GetType() == typeof(Number))
                    {   // if the function has one NUMBER left in it (which it should be) run as normal
                        {
                            float functionValue = currentFunc.Calculate();  // calculate what the value should be, and store it in this variable
                            if ((currentCalculationList[i] as Function).failedCalculation) { failedCalculation = true; }    // if the function's failedCalculation is true, set the bedmas's failedCalculation to true
                            currentCalculationList[i] = new Number(Convert.ToDecimal(functionValue));   // replaces the index containing the function with a number containing the value that was calculated
                        }
                    }
                    else
                    {   // if it isn't a number, print what is going wrong
                        Debug.Log("Math Error. Trying to get " + currentFunc.functionType + " of type " + currentFunc.embededCalculationList.calculationList[0].GetType());
                        failedCalculation = true;   // set failedCalulation to true
                        currentCalculationList[i] = new Number(0);  // replace the value with a 0 (it needs a number)
                    }
                }
                else
                {   // if the embeded list is too long
                    Debug.Log("Math Error. Too many values in " + currentFunc.embededCalculationList.IDcode);
                    failedCalculation = true;   // set failedCalulation to true
                    currentCalculationList[i] = new Number(0);  // replace the value with a 0
                }
            }
        }

        MultiplyDivide(calculationList);    // no more functions in here? move on to DM (dividing and multipling)
    }

    void MultiplyDivide(CalculationList calculationList)    // this method looks for the * and / operator, and applies that operator to the surronding numbers
    {
        List<Inputs> currentCalculationList = calculationList.calculationList;  // same drill, set it to the list...
        Debug.Log("Finding multiplication/division in " + calculationList.IDcode);  // and log what we are doing...
        for (int i = 0; i < currentCalculationList.Count; i++)  // and look at each index
        {
            if (currentCalculationList[i].GetType() == typeof(Operator))
            {    // if we find an operator...
                currentOper = currentCalculationList[i] as Operator;    // set currentOper to the operator we found (to avoid casting)
                if (currentOper.operatorType == Operator.OperatorType.div || currentOper.operatorType == Operator.OperatorType.mul)
                {   // we only care about multiplying and dividing right now, so only do math if the operator is either of those to types
                    if (i != 0 && i != currentCalculationList.Count - 1)
                    {   // if the operator isn't at the beginning of the list ('_+3' what are we adding?) or the end ('3/_' dividing by what), do some math
                        if (currentCalculationList[i + 1].GetType() == typeof(Number) && currentCalculationList[i - 1].GetType() == typeof(Number))
                        {   // if the two numbers surronding the operator are numbers
                            currentNum1 = currentCalculationList[i - 1] as Number;  // the number before the operator is stored here
                            currentNum2 = currentCalculationList[i + 1] as Number;  // the number after the operator is stored here

                            currentCalculationList[i] = new Number(SimpleMath(currentNum1, currentNum2, currentOper));  // we replace the index holding the operator with the operation applied to the 2 stored numbers
                            currentCalculationList.RemoveAt(i + 1); // next we delete the second number
                            currentCalculationList.RemoveAt(i - 1); // and then the first number
                            i--;    // subtract our current index by one so we don't miss a number
                        }
                        else
                        {
                            Debug.Log("No numbers found around operator"); // if there weren't enough numbers on each side of the operator, debug log this
                            failedCalculation = true;   // set the calculation to failed
                        }
                    }
                    else
                    {
                        Debug.Log("Invalid operator location"); // if the operator wasn't in a possible location to do math, debug log this fact
                        failedCalculation = true;   // save that the calculation has failed
                    }
                }
            }
        }

        AddSubtract(calculationList);   // move onto the final step in bedmas, addition and subraction in our calculation list
    }

    void AddSubtract(CalculationList calculationList)   // this method is the exact same as above, but with + and - 
    {
        List<Inputs> currentCalculationList = calculationList.calculationList;
        Debug.Log("Finding addition/subtraction in " + calculationList.IDcode); // if you still can't understand these 3 lines I can't help you
        for (int i = 0; i < currentCalculationList.Count; i++)
        {
            if (currentCalculationList[i].GetType() == typeof(Operator))
            {   // check if the current index is equal to type operator
                if (i != 0 && i != currentCalculationList.Count - 1)
                {   // if the operator isn't on either end of the list
                    currentOper = currentCalculationList[i] as Operator;
                    if (currentOper.operatorType == Operator.OperatorType.add || currentOper.operatorType == Operator.OperatorType.sub)
                    {   // now we only care about addition and subtraction
                        if (currentCalculationList[i + 1].GetType() == typeof(Number) && currentCalculationList[i - 1].GetType() == typeof(Number))
                        {   // we can only do the math if the two numbers surronding the operator are numbers
                            currentNum1 = currentCalculationList[i - 1] as Number;  // the first number in the equation is stored here
                            currentNum2 = currentCalculationList[i + 1] as Number;  // the next number is held here

                            currentCalculationList[i] = new Number(SimpleMath(currentNum1, currentNum2, currentOper));  // we replace the index here with the operator applied to these 2 numbers
                            currentCalculationList.RemoveAt(i + 1); // delete the second number
                            currentCalculationList.RemoveAt(i - 1); // and the first number next
                            i--;    // subract our current index by one (we just moved everything down one, so we need to account for that)
                        }
                        else
                        {
                            Debug.Log("No numbers found around operator"); // if there weren't enough numbers on each side of the operator, debug log this
                            failedCalculation = true;   // set the calculation to failed
                        }
                    }
                    else
                    {
                        Debug.Log("Invalid operator location"); // if the operator wasn't in a possible location to do math, debug log this fact
                        failedCalculation = true;   // saved that the calculation has failed
                    }
                }
            }
        }
    }

    decimal SimpleMath(Number number1, Number number2, Operator oper)   // this method takes an operator type, the numbers we want to apply it to, and calculates it
    {
        decimal float1 = number1.value; // this is the first value in the calculation
        decimal float2 = number2.value; // the second value we will be using
        decimal result;

        switch (oper.operatorType)  // for _____ operator type, do _______
        {
            default:
                Debug.Log("No operator type given. Defualt: multiply"); // if no operator type is given, just assume multiply (it's the least likely to cause problems)
                result = float1 * float2;   // multiply the 2 numbers and print our process
                Debug.Log("" + float1 + " " + "*" + " " + float2 + " = " + result);
                break;

            case Operator.OperatorType.add:
                result = float1 + float2;   // add the 2 numbers and print our process
                Debug.Log("" + float1 + " " + "+" + " " + float2 + " = " + result);
                break;

            case Operator.OperatorType.div:
                result = float1 / float2;   // divide the 2 numbers and print our process
                Debug.Log("" + float1 + " " + "/" + " " + float2 + " = " + result);
                break;

            case Operator.OperatorType.mul:
                result = float1 * float2;   // multiply the 2 numbers and print our process
                Debug.Log("" + float1 + " " + "*" + " " + float2 + " = " + result);
                break;

            case Operator.OperatorType.sub:
                result = float1 - float2;   // subtract the 2 numbers and print our process
                Debug.Log("" + float1 + " " + "-" + " " + float2 + " = " + result);
                break;
        }
        return result;  // return our resulting value
    }
}
