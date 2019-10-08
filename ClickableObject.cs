using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler
{
    Calculation calculation;    // this variable accesses the main instance of calculation, allowing this script to access the variables and functions in that script
    public enum ButtonType { zero, one, two, three, four, five, six, seven, eight, nine, add, subtract, multiply, divide, cos1, sin1, tan1, log, ln, cos, sin, tan, sq, sqrt, ans, range, negative, lbrackets, rbrackets, dot, equals, delete, clear };
    public ButtonType buttonType;   // an enum is created to hold all of the different things a button can do, and then this variable to hold that enum type. This variable is assigned in the editor

    void Start()
    {
        calculation = Calculation.instance; // assign the var to the main instance
    }

    public void OnPointerClick(PointerEventData eventData)
    {   // the OnPointerClick event runs when the GameObject this script is attached to is clicked
        if (PointerEventData.InputButton.Left == eventData.button)
        {   // if it was LEFT clicked, than we run a function in our main script depending on the individual enum assigned
            switch (buttonType)
            {
                case ButtonType.add:
                    calculation.AddOperator(2);
                    break;

                case ButtonType.ans:
                    calculation.AddMostRecentAns();
                    break;

                case ButtonType.clear:
                    calculation.ClearEquation();
                    break;

                case ButtonType.cos:
                    calculation.AddFunction(5);
                    break;

                case ButtonType.cos1:
                    calculation.AddFunction(0);
                    break;

                case ButtonType.delete:
                    calculation.DeleteMostRecent();
                    break;

                case ButtonType.divide:
                    calculation.AddOperator(1);
                    break;

                case ButtonType.dot:
                    calculation.AddDecimal();
                    break;

                case ButtonType.eight:
                    calculation.AddNumber(8);
                    break;

                case ButtonType.equals:
                    calculation.EqualsPressed();
                    break;

                case ButtonType.five:
                    calculation.AddNumber(5);
                    break;

                case ButtonType.four:
                    calculation.AddNumber(4);
                    break;

                case ButtonType.lbrackets:
                    calculation.AddBrackets();
                    break;

                case ButtonType.ln:
                    calculation.AddFunction(4);
                    break;

                case ButtonType.log:
                    calculation.AddFunction(3);
                    break;

                case ButtonType.multiply:
                    calculation.AddOperator(0);
                    break;

                case ButtonType.negative:
                    calculation.AddNegative();
                    break;

                case ButtonType.nine:
                    calculation.AddNumber(9);
                    break;

                case ButtonType.one:
                    calculation.AddNumber(1);
                    break;

                case ButtonType.range:
                    calculation.EnterRandomRangeRange();
                    break;

                case ButtonType.rbrackets:
                    calculation.ExitBrackets();
                    break;

                case ButtonType.seven:
                    calculation.AddNumber(7);
                    break;

                case ButtonType.sin:
                    calculation.AddFunction(6);
                    break;

                case ButtonType.sin1:
                    calculation.AddFunction(1);
                    break;

                case ButtonType.six:
                    calculation.AddNumber(6);
                    break;

                case ButtonType.sq:
                    calculation.AddFunction(8);
                    break;

                case ButtonType.sqrt:
                    calculation.AddFunction(9);
                    break;

                case ButtonType.subtract:
                    calculation.AddOperator(3);
                    break;

                case ButtonType.tan:
                    calculation.AddFunction(7);
                    break;

                case ButtonType.tan1:
                    calculation.AddFunction(2);
                    break;

                case ButtonType.three:
                    calculation.AddNumber(3);
                    break;

                case ButtonType.two:
                    calculation.AddNumber(2);
                    break;

                case ButtonType.zero:
                    calculation.AddNumber(0);
                    break;
            }
        }
    }
}
