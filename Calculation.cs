using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/*  Unity Basics
 *
 *  PUBLIC - functions with the keyword 'public' in front of it can be accessed by code outside of the class it's defined in. For example, number's AddDigit
 *  function can be accessed in any script, such as at line 234
 *
 *  TEXTMESHPRO - TextMeshPro is a Unity addon that makes it easy to display Text by changing an object's internal string, automattically formatted and
 *  fonted to look decent in the program scene
 *
 *  LISTS - lists have many useful built-in functions, such as Add(), RemoveAt(), Count.
 *      -  Add(), extends the list's length by one, and then fills in the new index with the value inputted in the parameter
 *      -  RemoveAt(), removes the value at the inputted index, and moves everthing down in the list. For example, in a list like [0, 152, 29, 43, 726, 95]
 *          running the function .RemoveAt(3) on the list remove the number at the 3rd index, and the list into something like this > [0, 152, 29, 726, 95]
 *      -  Count, returns how many indexs a list has, such as 0 for empty, 10 for 10 indexs.  *** NOTE *** to get the last index in a list, we need to subtract
 *           1 from the length because list's start at 0, which .Count doesn't account for  
 *
 *  OTHER STUFF - some other handy stuff includes all of the Mathf functions, which is a library for complex math so I don't have to program them myself
 *
 *  *** ASK ME IF YOU HAVE ANY QUESTIONS ***
 */

[Serializable]
public class Calculation : MonoBehaviour    // this script is the main calculator
{
    public static Calculation instance;  // this static variable with hold one instance of this script, allowing any script to access this instance
    public TextMeshProUGUI equationDisplay; // this variable holds the Unity object that will display the equation typed in. It will be assigned in the Unity Editor 
    public TextMeshProUGUI resultDisplay;   // this variable holds the Unity object that display the equation's result. It will also be assigned in the Unity Editor
    public BedmasBrain bedmasBrain = new BedmasBrain(); // create an instance to access our bedmas brain's methods
    CalculationList mainMemory;  // a variable that holds our main calculation list, and all of the data that will be calculated
    CalculationList currentCalculationList;  // the current list values are being inputted into 
    public Number previousAns;  // this stores the result of the last equation
    string equationText;    // stores the ACSII characters that represents the equation
    bool rangeMode; // stores whether or not values are being inputted in the random range mode or typical calculation lists
    bool minNotMax; // this bool stores whether or not the user is entering the min value or the max value
    bool decimalJustAdded;  // was a decimal just added? 
    bool operatorJustAdded; // was an operator just added?
    bool nextNumberNegative;    // is the next number negative?

    void Awake()    // this is a built in Unity function that allows you to write code that runs as soon as the program starts
    {
        Screen.SetResolution(550, 400, false);  // the ui doesn't work very well, haven't figured that out yet, so i restrict the app window size
        equationDisplay.text = "";  // sets the equation display text to an empty string
        resultDisplay.text = "";    // sets the result display text to an empty string
        equationText = "";  // sets the equation string to blank
        mainMemory = new CalculationList(new List<int> { }, "mainMemory");  // sets the main memory to a new calculationList with an empty ID and a hard coded name
        currentCalculationList = mainMemory;    // sets the current input list to the main memory

        if (instance != null)
        {   // if if instance has been assigned to, that means that multiples of this script exist, which will cause problems.
            Debug.Log("Multiple instances of Calculation found");
        }
        instance = this;    // set instance to be this script
    }

    void Update()   // this is a built in Unity function that allows you to write code that runs once every frame
    {
        if (Input.GetKeyDown(KeyCode.P))    // if the key "P" is pressed... (only returns true on the frame it was pressed)
        {
            PrintMemory(mainMemory, true);  // print what the main memory contains using a custom function
        }
    }

    public void EqualsPressed()  // this function runs when the equals button is pressed
    {
        if (!rangeMode) // if the calculator isn't in range mode, do what you would expect an equal button to do
        {
            PrintMemory(mainMemory, true);  // print the entire memory
            bedmasBrain.Calculate(mainMemory);  // uses bedmas brains's calculate method on our main memory (AKA what you use a calculator for)
            if (bedmasBrain.failedCalculation)  // if some part of the bedmas calculation failed
            {
                resultDisplay.text = "Math Error";  // set our result to Math Error, rather than a number
            }
            else
            {
                resultDisplay.text = (mainMemory.calculationList[0] as Number).value.ToString("G29");   // set our result text to bedmasBrain's remaining number
                previousAns = new Number((mainMemory.calculationList[0] as Number).value);  // set our previousAns to this number
                Debug.Log("Previous answer: " + previousAns.value); // debug log this saving feature
            }
            currentCalculationList = mainMemory;    // set the current input list to main memory

        }
        else    // if the calculator is in range mode, run something completely different
        {
            if (minNotMax) // if the user is entering the min
            {
                minNotMax = false;  // set this bool to max, not min
                Debug.Log("Inputing random range max"); // debug log and give the user instructions for inputting max range
                resultDisplay.text = "Enter range max.\nPress equal for number";
                equationText += " - ";  // create a visual barrier for the min value and the max value
                equationDisplay.text = equationText;    // update our text display to our equation string
                mainMemory.calculationList.Add(new Operator(Operator.OperatorType.mul));    // add a place holder operator in the main memory (just in case an operation needs to be done on these numbers, and to seperate the numbers) *** This step isn't really needed, it just makes me feel better ***
            }
            else    // if they are entering the max number...
            {
                float minRange = (float)(mainMemory.calculationList[0] as Number).value;    // set the min to the first number entered
                float maxRange = (float)(mainMemory.calculationList[2] as Number).value;    // set the max to the second number entered
                float randomNumber = UnityEngine.Random.Range(minRange, maxRange);  // generate a random number based off of these ranges
                mainMemory.calculationList.Clear();   // remove all indexs from the main memory
                mainMemory.calculationList.Add(new Number(Convert.ToDecimal(randomNumber)));    // set the first index to our new number
                resultDisplay.text = (mainMemory.calculationList[0] as Number).value.ToString();    // output the new value in our result box
                previousAns = new Number((mainMemory.calculationList[0] as Number).value);  // save our random number in previousAns
                Debug.Log("Previous answer: " + previousAns.value); // debug log the value created
                rangeMode = false;  // exit range mode
            }
        }
    }

    public void EnterRandomRangeRange() // starts the random range process
    {
        ClearEquation();
        Debug.Log("Starting random range");
        minNotMax = true;   // set our input type to min (a min range is being added, not max)
        resultDisplay.text = "Enter range min.\nPress equal for rangeMax";  // give instructions for the user to use range mode
        rangeMode = true;   // enter range mode
    }

    void NavigateToParentList(List<int> listID) // this function takes a calculation ID, and finds the list that holds the object with that ID
    {
        List<int> parentID = listID;    // create variable to hold the new ID that will be created
        parentID.RemoveAt(listID.Count - 1);    // remove the value at the last index and voila! that's the parent ID
        NavigateToList(parentID);   // navigate to this new list
    }

    void NavigateToList(List<int> listID)   // takes in an ID, and sets the currentCalculationList to this ID
    {
        currentCalculationList = mainMemory;    // the porcess start at the origin, the main memory
        for (int i = 0; i < listID.Count; i++)  // for the length of the ID
        {
            int index = listID[i];  // set the index being looked in to the value in the current index of the ID list
            Debug.Log("Checking index " + index);   // debug log what index is being checked
            if (currentCalculationList.calculationList[index].GetType() == typeof(CalculationList))
            {   // if there is a list there, great! debug log this and enter the list
                Debug.Log("Entering list " + (currentCalculationList.calculationList[index] as CalculationList).IDcode);
                currentCalculationList = currentCalculationList.calculationList[index] as CalculationList;
            }
            else if (currentCalculationList.calculationList[index].GetType() == typeof(Function))
            {   // if there is a function there, that's still fine! we'll just enter it's embededList
                Debug.Log("Entering function " + (currentCalculationList.calculationList[index] as Function).embededCalculationList.IDcode);
                currentCalculationList = (currentCalculationList.calculationList[index] as Function).embededCalculationList;
            }
            else
            {   // if neither are present, that means our ID was messed up somewhere. Log this error and stop looking
                Debug.LogWarning("ID error. No valid target at " + index + " in " + currentCalculationList.IDcode);
                break;
            }

        }
        // after it is finished navigating, print what list the function ended in
        Debug.Log("Landed in " + currentCalculationList.IDcode);
    }

    void CheckForNewEquation()  // this checks whether or not the equation should be cleared
    {
        if (resultDisplay.text != "" && !rangeMode)
        {   // if the result display isn't blank and the calculator isn't in range mode
            ClearEquation();    // clear everything (function defined below)
        }
    }

    public void ClearEquation() // this function resets the calculator
    {
        Debug.Log("Clearing equation"); // debug log what is happening
        rangeMode = false;  // set range mode to false
        mainMemory.calculationList.Clear(); // delete everything in the mainMemory
        currentCalculationList = mainMemory;    // set the current calculation list to the main memory
        mainMemory.ID.Clear();
        equationText = "";
        equationDisplay.text = "";  // set the equation string and both displays to blank strings
        resultDisplay.text = "";
    }

    void PrintMemory(CalculationList calculationList, bool firstIteration = false)  // this is a debugging function that steps through the memory, printing what it finds (the firstIteration = false means that unless specified, the function isn't printing in the firstIteration, AKA it's false)
    {
        if (firstIteration)
        {   // if this iteration was started by me pressing "P", debug log when the printing started (this will only happen for the first list being printed)
            Debug.Log("---------------   Pritning time: " + Time.time + "   ---------------");
        }
        // debug log what list is being printedß
        Debug.Log("---------------   Pritning at " + calculationList.IDcode + "   ---------------");   
        for (int i = 0; i < calculationList.calculationList.Count; i++)
        {   // index through every value in the inputted list
            if (calculationList.calculationList[i].GetType() == typeof(Number))
            {   // if a number is found, print what value it has and where it is
                Debug.Log("Number " + (calculationList.calculationList[i] as Number).value + " at index " + i);
            }
            else if (calculationList.calculationList[i].GetType() == typeof(Operator))
            {   // if an operator is found, debug log what type it is and where it is
                Debug.Log("Operator " + (calculationList.calculationList[i] as Operator).operatorType + " at index " + i);
            }
            else if (calculationList.calculationList[i].GetType() == typeof(Function))
            {   // debug log where the function is and what type it is...
                Debug.Log("Function " + (calculationList.calculationList[i] as Function).functionType + " at index " + i);
                PrintMemory((calculationList.calculationList[i] as Function).embededCalculationList);    // and start printing it's contents (the bool is set to false because it isn't the first print)
            }
            else if (calculationList.calculationList[i].GetType() == typeof(CalculationList))
            {   // debug log where the brackets are...
                Debug.Log("CalculationList at index " + i);
                PrintMemory(calculationList.calculationList[i] as CalculationList);  // and start printing it's contents
            }
        }

        if (calculationList.calculationList.Count == 0)
        {
            Debug.Log("List is empty"); // if there isn't anything in the list, debug log that
        }

        Debug.Log("---------------   End of " + calculationList.IDcode + "   ---------------"); // debug log that the end of the inputted list has been reached

        if (firstIteration)
        {       // if wew have reached here, that means that the function is at the end of the printing function on the main memory
            Debug.Log("Current calculation list: " + currentCalculationList.IDcode);    // as extra info, debug log what list values are being inputted into...
            Debug.Log("Current equation text: |" + equationText + "|"); // and debug log what our equationText is, so what are display internally looks like
        }
    }

    public void AddNumber(int value)    // this function is called by buttons in the Unity Scene which will add numbers to the equation. The parameter 'value' is unique and preset for each button, and it represents the number being added
    {
        operatorJustAdded = false;  // the latest thing added is no longer an operator, so set this to false
        CheckForNewEquation();  // check if a new equation is being created
        Debug.Log("Inputing " + value + " in " + currentCalculationList.IDcode);    // debug log what value is being inputted and where
        if (currentCalculationList.calculationList.Count > 0)
        {   // if the list has actual values in it, the calculator might be adding a decimal, so it will have to check for that
            if (equationText[equationText.Length-1] != 's')
            {   // if the equation text has an 's' on the end, that means that our previous ans was just added, which can't have digits added to it
                Inputs lastElement = currentCalculationList.calculationList[currentCalculationList.calculationList.Count - 1];  // if there are elements in the list, set this variable to the element that is potentially being added too  
                if (lastElement.GetType() == typeof(Number))    // if that element is a number...
                {
                    (lastElement as Number).AddDigit(value);    // add the value as a digit to the number
                }
                else
                { // if it isn't a number, than a new number is being made!
                    currentCalculationList.calculationList.Add(new Number(value, nextNumberNegative));
                    decimalJustAdded = false; // a new number was created, so it doesn't have a decimal
                }
            }
        }
        else
        {   // this runs when the value will be the first value in the list
            currentCalculationList.calculationList.Add(new Number(value, nextNumberNegative));
            decimalJustAdded = false;   // and it didn't just add a decimal
        }

            nextNumberNegative = false; // set the newest number to not be negative
            equationText += value;  // add the number to our equation string
            equationDisplay.text = equationText;    // and update the display
    }

    public void AddOperator(int operatorID) // this function is similar to add number, except it takes an operator type and adds that operator
    {
        if (!rangeMode) // the calculator shouldn't add an operator in range mode, so don't run the code if the calculator is in range mode
        {
            if (!operatorJustAdded) // an operator can't be applied to an operator so the calculator shouldn't double stack operators (such as "++"), so only run this if the last input added isn't an operator
            {
                CheckForNewEquation();  // check to see if a new equation is being created

                Operator.OperatorType operatorType; // this variable will be used in the creation of the operator instance
                switch (operatorID) // cycle through all operator types that could have been inputted
                {
                    default:
                        operatorType = Operator.OperatorType.mul;   // in a default case, set the operator to multiply...
                        equationText += " * "; // add the multiplication symbol to our calculation text (and some spaces to make it look nicer)
                        break;
                    case 0:
                        operatorType = Operator.OperatorType.mul;    // set the operator to multiply
                        equationText += " * "; // add the multiplication symbol to our calculation text
                        break;
                    case 1:
                        operatorType = Operator.OperatorType.div;    // set the operator to divide...
                        equationText += " / "; // add the multiplication symbol to our calculation text
                        break;
                    case 2:
                        operatorType = Operator.OperatorType.add;    // set the operator to add...
                        equationText += " + "; // add the multiplication symbol to our calculation text
                        break;
                    case 3:
                        operatorType = Operator.OperatorType.sub;    // set the operator to subtract
                        equationText += " - "; // add the multiplication symbol to our calculation text
                        break;
                }

                Debug.Log("Inputing " + operatorType + " at " + currentCalculationList.IDcode);   // debug log what is being inputed where
                currentCalculationList.calculationList.Add(new Operator(operatorType)); // input an Operator object with it's type equal to the operator type that was just decided
                operatorJustAdded = true;   // an operator was just added
                equationDisplay.text = equationText;    // update the display
            }
        }
    }

    public void AddFunction(int functionID) // this function is similar to add operator, but with a function type instead of a operator type
    {
        if (!rangeMode) // if it isn't in range mode, run the rest of the function
        {
            CheckForNewEquation();  // check to see if this is the start of a new equation
            Function.FunctionType functionType; // create a variable to store the function type we'll be using to create our function objects
            switch (functionID)
            {
                default:    // if an invalid functionID was inputted, just set it to squaring (why? it's the least likely to cause problems) 
                    functionType = Function.FunctionType.sq;    // set the funciton type to squaring
                    equationText += "sq(";  // add the corresponding symbol to the equation text
                    break;
                case 0:
                    functionType = Function.FunctionType.acos;  // set the funciton type to inverse cos
                    equationText += "cos<sup>-1</sup>(";    // add the corresponding symbol to the equation text
                    break;
                case 1:
                    functionType = Function.FunctionType.asin;  // set the funciton type to inverse sin
                    equationText += "sin<sup>-1</sup>(";    // add the corresponding symbol to the equation text
                    break;
                case 2:
                    functionType = Function.FunctionType.atan;  // set the funciton type to inverse tan
                    equationText += "tan<sup>-1</sup>(";    // add the corresponding symbol to the equation text
                    break;
                case 3:
                    functionType = Function.FunctionType.log;   // set the funciton type to a log with a base of 10
                    equationText += "log("; // add the corresponding symbol to the equation text
                    break;
                case 4:
                    functionType = Function.FunctionType.ln;    // set the funciton type to a log with a base if e (a natural logrithm)
                    equationText += "ln(";  // add the corresponding symbol to the equation text
                    break;
                case 5:
                    functionType = Function.FunctionType.cos;   // set the funciton type to cos
                    equationText += "cos("; // add the corresponding symbol to the equation text
                    break;
                case 6:
                    functionType = Function.FunctionType.sin;   // set the funciton type to sin
                    equationText += "sin("; // add the corresponding symbol to the equation text
                    break;
                case 7:
                    functionType = Function.FunctionType.tan;   // set the funciton type to tan
                    equationText += "tan("; // add the corresponding symbol to the equation text
                    break;
                case 8:
                    functionType = Function.FunctionType.sq;    // set the funciton type to squaring
                    equationText += "sq(";  // add the corresponding symbol to the equation text
                    break;
                case 9:
                    functionType = Function.FunctionType.sqrt;  // set the funciton type to square root
                    equationText += "sqrt(";    // add the corresponding symbol to the equation text
                    break;
            }

            Debug.Log("Inputing " + functionType + " at " + currentCalculationList.IDcode); // debug log what is happening and where
            List<int> newID = currentCalculationList.ID;    // create a new ID equal to the currentList's ID
            newID.Add(currentCalculationList.calculationList.Count);    // add a new index that represents where this function is in the currentList
            currentCalculationList.calculationList.Add(new Function(functionType, newID));  // add a new Function object with the newly created ID and the designated function type
            NavigateToList(newID);  // enter the newly created list
            Debug.Log("New list ID: " + currentCalculationList.IDcode); // debug log the new list's ID
            equationDisplay.text = equationText;    // update the equation display
        }
    }

    public void AddBrackets()   // this function adds a calculation list (AKA, brackets) into the currentList and enters it
    {
        if (!rangeMode)
        {   // if the calculator isn't in range mode...
            CheckForNewEquation();  // check to see if calculation needs to be reset
            List<int> newID = currentCalculationList.ID;    // create a new ID equal to the currentList's ID
            newID.Add(currentCalculationList.calculationList.Count);    // add a new index that represents where this bracket is in the currentList
            Debug.Log("Inputing a list at " + currentCalculationList.IDcode);   // debug logs where the list is being inputted
            currentCalculationList.calculationList.Add(new CalculationList(newID)); // adds a new CalculationList object with the newly created ID
            NavigateToList(newID);  // enter this newly created list
            Debug.Log("New list ID: " + currentCalculationList.IDcode); // print the new list ID
            equationText += "(";    // add the bracket character to the equation string
            equationDisplay.text = equationText;    // update the equation display
        }
    }

    public void ExitBrackets()  // this function exits the current calculationList
    {
        if (!rangeMode)
        {   // if the calculator isn't in range mode
            CheckForNewEquation();  // check to see if the equation needs to be reset
            if (currentCalculationList.IDcode != "mainMemory")
            {   // the function can only navigate to a parent list if the list has a parent list (which the main memory doesn't)
                NavigateToParentList(currentCalculationList.ID);    // navigate to the currentList's parent list
                equationText += ")";    // add a bracket character to the equation string
                equationDisplay.text = equationText;    // update the equation display
            }
            else
            {   // if the currentCalculationList is the mainMemory, there isn't a parent list to go to, therefore it can't use a exit bracket
                Debug.Log("Can't go to parentList. Already in mainMemory.");
            }
        }
    }

    public void AddDecimal()    // this function adds a decimal to the last number
    {
        if (!decimalJustAdded)
        {   // if this number doesn't already have a decimal
            if (currentCalculationList.calculationList.Count > 0)
            {   // if the list is longer than 0 (AKA does it have something in it?)
                int lastIndex = currentCalculationList.calculationList.Count - 1;   // set the index to the list's length - 1
                if (currentCalculationList.calculationList[lastIndex].GetType() == typeof(Number))  // is this last index a number?
                {   
                    Debug.Log("Adding decimal");    // if it is, debug log that the function is adding a decimal
                    Inputs lastElement = currentCalculationList.calculationList[lastIndex]; // create a variable to hold this number
                    (lastElement as Number).isDecimal = true;   // set the number to takes number inputs as decimals
                    equationText += ".";    // add a decimal to equation string
                    equationDisplay.text = equationText;    // update the display
                    decimalJustAdded = true;    // note that a decimal was just added
                }
            }
        }
    }

    public void AddNegative()   // this function sets the next number to be inputted as a negative
    {
        if (!nextNumberNegative)
        {   // if the next number isn't already negative, continue running the code
            CheckForNewEquation();  // check to see if the equation will be reset
            nextNumberNegative = true;  // set the next number to negative
            Debug.Log("Adding negative");   // debug log that a negative is being added
            equationText += "-";  // add the negative symbol to the equation
            equationDisplay.text = equationText;    // update the display
        }
    }


    public void AddMostRecentAns()  // this function adds the number that was calculated in the last equation
    {
        decimal value = previousAns.value;  // create a variable to hold the numerical value of our last value
        operatorJustAdded = false;  // set that an operator wasn't just added
        CheckForNewEquation();  // check to see if the calculator is creating a new equation
        Debug.Log("Inputing " + value + " in " + currentCalculationList.IDcode);    // debug log what is happening
        currentCalculationList.calculationList.Add(new Number(value, nextNumberNegative));  // add a new number with a value of the number saved
        equationText += "Ans";  // add the word Ans to the equation
        equationDisplay.text = equationText;    // update the display
    }


    public void DeleteMostRecent()  // this function deletes the most recent input done by the user
    {
        if (currentCalculationList.IDcode != "mainMemory")
        {   // the function does't want to be looking in empty brackets, so first check to see if the current list has a parent list (AKA it's not the main memory)
            if (currentCalculationList.calculationList.Count == 0)
            {   // if the list has a parent and it's empty, then the list needs to be exited, because it'll end up being deleted
                NavigateToParentList(currentCalculationList.ID);
                Debug.Log("Exiting current empty list");    // debug log what the function is doing
            }
        }

        if (currentCalculationList.calculationList.Count != 0)
        {   // if the list has something in it, then it is possible to delete the most recent input
            int lastEquationIndex = equationText.Length - 1;    // and a number to represent the last index in the equationText
            char lastInput = equationText[lastEquationIndex];   // look at the last letter to see what was last inputted
            string lastInputType;   // initialize the string that will hold our answer

            Debug.Log("Last input: " + lastInput);  // log what the last input was

            switch (lastInput)
            {
                default:
                    lastInputType = "IDK";
                    break; // do nothing, I have no idea how it could get here

                case '0':
                    lastInputType = "Number";   // if we last inputted a number, save that fact
                    break;

                case '1':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '2':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '3':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '4':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '5':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '6':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '7':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '8':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '9':
                    lastInputType = "Number";   // same reason as before
                    break;

                case '(':
                    lastInputType = "UndeterminedEnter";    // if we last inputted a function or bracket, write that
                    break;

                case ')':
                    lastInputType = "UndeterminedExit"; // if we last ended a function or bracket, write that
                    break;

                case '.':
                    lastInputType = "Decimal";  // if we just inputted a decimal, write that
                    break;

                case '-':
                    lastInputType = "Negative"; // if we just inputted a negative, remember that
                    break;

                case ' ':
                    lastInputType = "Operator"; // if we just inputted an operator, write that
                    break;

                case 's':
                    lastInputType = "PreviousAns";  // if we used the previousAns button write that
                    break;
            }

            Debug.Log("Last input type: " + lastInputType); // debug log what we found
            int lastCalculationIndex = currentCalculationList.calculationList.Count - 1;    // create a number equal the last index in the calculationList

            switch (lastInputType)
            {
                default:
                    Debug.Log("No value?");
                    break;  // it really shouldn't be able to get here

                case "Number":
                    (currentCalculationList.calculationList[lastCalculationIndex] as Number).RemoveLastDigit(); // remove the last digit in a number...
                    if ((currentCalculationList.calculationList[lastCalculationIndex] as Number).value == 0)
                    {   // and if that number's value is now 0, get rid of it
                        currentCalculationList.calculationList.RemoveAt(lastCalculationIndex);
                    }   // remove the number from the text
                    equationText = equationText.Remove(lastEquationIndex);
                    break;

                case "Operator":
                    currentCalculationList.calculationList.RemoveAt(lastCalculationIndex);  // we remove the last inputted operator...
                    equationText = equationText.Remove(lastEquationIndex - 2);  // and remove it from the text (including the spaces that are inputted with the symbol)
                    break;

                case "Decimal":
                    (currentCalculationList.calculationList[lastCalculationIndex] as Number).isDecimal = false; // set the last number's bool to false
                    equationText = equationText.Remove(lastEquationIndex);  // remove the dot from the text
                    break;

                case "UndeterminedEnter":
                    if (currentCalculationList.calculationList[lastCalculationIndex].GetType() == typeof(Function))
                    {   // if the last thing inputted was a function
                        switch ((currentCalculationList.calculationList[lastCalculationIndex] as Function).functionType)
                        {   // 
                            default:    // just delete a number of character equal to the function's name (so 'ln' deletes two characters, 'sqrt' deletes 4, etc)
                                equationText = equationText.Remove(lastEquationIndex - (currentCalculationList.calculationList[lastCalculationIndex] as Function).functionType.ToString().Length);
                                break;

                                // that works for all function except for acos, asin, or atan. These will have to be deleted a bit differently
                            case Function.FunctionType.acos:    // to make the text look good, the string really looks like 'sin<sup>-1</sup>', so we need to delete way more characters
                                equationText = equationText.Remove(lastEquationIndex - ((currentCalculationList.calculationList[lastCalculationIndex] as Function).functionType.ToString().Length + 12));
                                break;

                            case Function.FunctionType.asin:    // same reason as before
                                equationText = equationText.Remove(lastEquationIndex - ((currentCalculationList.calculationList[lastCalculationIndex] as Function).functionType.ToString().Length + 12));
                                break;

                            case Function.FunctionType.atan:    // same reason as before
                                equationText = equationText.Remove(lastEquationIndex - ((currentCalculationList.calculationList[lastCalculationIndex] as Function).functionType.ToString().Length + 12));
                                break;
                        }
                    }
                    else if (currentCalculationList.calculationList[lastCalculationIndex].GetType() == typeof(CalculationList))
                    {   // if the last thing inputted was a calculationList, just delete the last character in the string
                        equationText = equationText.Remove(lastEquationIndex);
                    }

                    currentCalculationList.calculationList.RemoveAt(lastCalculationIndex);  // and of course, remove the last item from the calculationList
                    break;

                case "UndeterminedExit":
                    if (currentCalculationList.calculationList[lastCalculationIndex].GetType() == typeof(Function))
                    {   // navigate to the last function exited (the list at the end of the calculation list)
                        NavigateToList((currentCalculationList.calculationList[lastCalculationIndex] as Function).embededCalculationList.ID);
                    }
                    else if (currentCalculationList.calculationList[lastCalculationIndex].GetType() == typeof(CalculationList))
                    {   // or the last brackets exited, it doesn't make much of a difference, same concept
                        NavigateToList((currentCalculationList.calculationList[lastCalculationIndex] as CalculationList).ID);
                    }
                    equationText = equationText.Remove(lastEquationIndex);  // remove the latest symbol ')'
                    break;

                case "PreviousAns":
                    currentCalculationList.calculationList.RemoveAt(lastCalculationIndex);  // delete the last index
                    equationText = equationText.Remove(lastEquationIndex - 2);  // remove the last 3 characters (the 'Ans')
                    break;

                case "Negative":
                    nextNumberNegative = false; // set the next number to not be negative
                    equationText = equationText.Remove(lastEquationIndex);  // remove the last character '-'
                    break;
            }

            equationDisplay.text = equationText;    // update the display to match the modified text
        }
    }
}
