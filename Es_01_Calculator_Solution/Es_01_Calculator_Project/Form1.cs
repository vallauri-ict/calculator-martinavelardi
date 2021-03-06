﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Es_01_Calculator_Project
{
    public partial class FormMain : Form
    {
        //private char[,] bottoni = new char[6,4];
        public struct ButtonStruct
        {
            public char Content;
            public bool IsBold;
            public bool IsNumber;
            public bool IsDecimalSeparator;
            public bool IsPlusMinusSign;
            public bool IsOperator;
            public bool IsEqualSign;
            public bool IsSpecialOperator;
            public ButtonStruct(char content, bool isBold, bool isNumber = false, bool isDecimalSeparator = false, bool isPlusMinusSign = false, bool isOperator = false, bool isEqualSign = false, bool isSpecialOperator = false)
            {
                this.Content = content;
                this.IsBold = isBold;
                this.IsNumber = isNumber;
                this.IsDecimalSeparator = isDecimalSeparator;
                this.IsPlusMinusSign = isPlusMinusSign;
                this.IsOperator = isOperator;
                this.IsEqualSign = isEqualSign;
                this.IsSpecialOperator = isSpecialOperator;
            }
            public override string ToString()
            {
                return Content.ToString();
            }

        };
        private ButtonStruct[,] buttons =
        {
            {new ButtonStruct('%',false),new ButtonStruct('ɶ',false),new ButtonStruct('C',false),new ButtonStruct('⩤',false) },
            {new ButtonStruct('⅟',false,false,false,false,true,false,true),new ButtonStruct(' ',false),new ButtonStruct(' ',false),new ButtonStruct('÷',false, false, false, false, true) },
            {new ButtonStruct('7',true, true),new ButtonStruct('8',true, true),new ButtonStruct('9',true, true),new ButtonStruct('x',false, false, false, false, true) },
            {new ButtonStruct('4',true, true),new ButtonStruct('5',true, true),new ButtonStruct('6',true, true),new ButtonStruct('-',false, false, false, false, true) },
            {new ButtonStruct('1',true, true),new ButtonStruct('2',true, true),new ButtonStruct('3',true, true),new ButtonStruct('+',false ,false, false, false, true) },
            {new ButtonStruct('±',false, false, false, true),new ButtonStruct('0',true, true),new ButtonStruct(',',false, false, true),new ButtonStruct('=',false ,false, false, false, true, true) },

        };
        private RichTextBox resultBox;
        private Font baseFont = new Font("Segoe UI", 22, FontStyle.Bold);

        private const char ASCIIZERO = '\x0000';
        private double operand1, operand2, result;
        private char lastOperator;
        private ButtonStruct lastButtonClicked;
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Makebuttons(buttons);
            MakeResultsBox();
        }
        private void MakeResultsBox()
        {
            resultBox = new RichTextBox();
            resultBox.ReadOnly = true;
            resultBox.SelectionAlignment = HorizontalAlignment.Right;
            resultBox.Font = baseFont;
            resultBox.Width = this.Width - 16;
            resultBox.Height = 50;
            resultBox.Top = 20;
            resultBox.Text = "0";
            resultBox.TabStop = false;
            resultBox.TextChanged += ResultBox_TextChanged; // per capire quando devo andare a ridurre il carattere
            this.Controls.Add(resultBox);
        }

        private void ResultBox_TextChanged(object sender, EventArgs e)
        {
            if (resultBox.Text.Length == 1)
            {
                resultBox.Font = baseFont;
            }
            else
            {
                int delta = 17 - resultBox.Text.Length;
                if (delta % 2 == 0)
                {
                    float newSize = baseFont.Size + delta;
                    if (newSize > 8 && newSize < 23)
                    {
                        resultBox.Font = new Font(baseFont.FontFamily, newSize, baseFont.Style);
                    }
                }
            }
        }

        private void Makebuttons(ButtonStruct[,] bottoni)
        {
            int buttonWidth = 82;
            int buttonHeight = 60;
            int posX = 0;
            int posY = 101;

            for (int i = 0; i < bottoni.GetLength(0); i++)
            {
                for (int j = 0; j < bottoni.GetLength(1); j++)
                {
                    Button newButton = new Button();
                    newButton.Font = new Font("Segoe UI", 16);
                    ButtonStruct bs = buttons[i, j];
                    //newButton.Text = bottoni[i, j].Content.ToString();
                    newButton.Text = bs.ToString();
                    if (bs.IsBold)
                    {
                        newButton.Font = new Font(newButton.Font, FontStyle.Bold);
                    }
                    newButton.Width = buttonWidth;
                    newButton.Height = buttonHeight;
                    newButton.Left = posX;
                    newButton.Top = posY;
                    newButton.Tag = bs;
                    newButton.Click += Button_Click;
                    this.Controls.Add(newButton);
                    posX += buttonWidth;
                }
                posX = 0;
                posY += buttonHeight;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            ButtonStruct bs = (ButtonStruct)clickedButton.Tag;
            if (bs.IsNumber)
            {
                if (lastButtonClicked.IsEqualSign)
                {
                    clearAll();
                }
                if (resultBox.Text == "0" || lastButtonClicked.IsOperator)
                {
                    resultBox.Text = "";    // tolgo lo 0
                }
                if (resultBox.Text.Length < 20)
                {
                    resultBox.Text += clickedButton.Text;
                }
            }
            else
            {
                if (bs.IsDecimalSeparator)
                {
                    if (!resultBox.Text.Contains(bs.Content.ToString()))
                    {
                        if (resultBox.Text.Length < 20)
                        {
                            resultBox.Text += clickedButton.Text;
                        }
                    }
                }
                if (bs.IsPlusMinusSign)
                {
                    if (!resultBox.Text.Contains("-"))
                    {
                        resultBox.Text = "-" + resultBox.Text;
                    }
                    else
                    {
                        resultBox.Text = resultBox.Text.Replace("-", "");
                    }
                }
                else
                {
                    switch (bs.Content)
                    {
                        case 'C':
                            clearAll();
                            break;
                        case '⩤':
                            resultBox.Text = resultBox.Text.Remove(resultBox.Text.Length - 1);  // tolgo l'ultimo elemento
                            if (resultBox.Text.Length == 0 || resultBox.Text == "-0" || resultBox.Text == "-")
                            {
                                resultBox.Text = "0";
                            }
                            break;
                        default:
                            if (bs.IsOperator) manageOperators(bs);
                            break;
                    }
                }
            }
            lastButtonClicked = bs;
        }

        /// <summary>
        /// Format the number using thousend separator and 16 decimal digits
        /// </summary>
        /// <param name="number">The number to format</param>
        /// <returns>A string with thousend separator and a maximum 16 digit after the decimal point</returns>
        private string getFormattedNumber(double number)
        {
            //return String.Format("{0:0,0.0000000000}", number);
            char decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            return number.ToString("N16").TrimEnd('0').TrimEnd(decimalSeparator);
        }

        private void clearAll(double numberToWrite = 0)
        {
            operand1 = 0;
            operand2 = 0;
            result = 0;
            lastOperator = ASCIIZERO;
            resultBox.Text = getFormattedNumber(numberToWrite);
        }

        private void manageOperators(ButtonStruct bs)
        {
            if (bs.IsSpecialOperator)
            {
                switch (bs.Content)
                {
                    case '⅟':
                        double specialOperatorResult = 1 / (double.Parse(resultBox.Text));
                        if (lastOperator == ASCIIZERO)
                        {
                            result = specialOperatorResult;
                            resultBox.Text = getFormattedNumber(result);
                        }
                        else
                        {
                            operand2 = specialOperatorResult;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (lastOperator == ASCIIZERO) // o all'inizio o dopo un =
            {
                operand1 = double.Parse(resultBox.Text);
                lastOperator = bs.Content;
            }
            else
            {
                if (lastButtonClicked.IsOperator && !lastButtonClicked.IsEqualSign && !bs.IsSpecialOperator)
                {
                    if (bs.IsSpecialOperator)
                    {
                        result = operand2;
                        resultBox.Text = getFormattedNumber(result);
                    }
                    lastOperator = bs.Content;
                }
                else
                {
                    if (!lastButtonClicked.IsEqualSign)
                    {
                        operand2 = double.Parse(resultBox.Text);
                    }
                    switch (lastOperator)
                    {
                        case '+':
                            result = operand1 + operand2;
                            break;
                        case '-':
                            result = operand1 - operand2;
                            break;
                        case 'x':
                            result = operand1 * operand2;
                            break;
                        case '÷':
                            result = operand1 / operand2;
                            break;
                        default:
                            break;
                    }
                    if (!bs.IsEqualSign)
                    {
                        lastOperator = bs.Content;
                        operand2 = 0;
                    }
                    else
                    {
                        lastOperator = ASCIIZERO;
                    }
                    resultBox.Text = getFormattedNumber(result);
                }
            }
        }
    }
}
