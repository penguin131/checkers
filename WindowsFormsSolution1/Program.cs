using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace WindowsFormsSolution1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyForm());
        }
    }
    class MyForm : Form
    {
        // Создание формы и поля для шашек
        public MyForm()
        {
            MaximumSize = new Size(616, 839);
            MinimumSize = MaximumSize;
            int listId = 0;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (i % 2 > 0 && j % 2 == 0 || i % 2 == 0 && j % 2 > 0)
                    {
                        var button = new Button
                        {
                            Location = new Point(0 + i * 75, 0 + j * 75),
                            Size = new Size(75, 75),
                            BackColor = Color.LightGray,
                            Enabled = false //Редактируемое
                        };
                        Controls.Add(button);
                        checkersList.Add(new ListOfFields());
                        checkersList[listId].button = button;
                        checkersList[listId].side = CheckersSide.nobody; //Редактируемое
                        checkersList[listId].x = i;
                        checkersList[listId].y = j;
                        checkersList[listId].king = false;
                        //checkersList[listId].button.Text = checkersList[listId].x.ToString() + "," + checkersList[listId].y.ToString();
                        checkersList[listId].listNumber = listId;
                        //checkersList[listId].button.Text = checkersList[listId].listNumber.ToString();
                        listId++;
                    }
                }
            //Проставляю стартовые позиции
            foreach (var c in checkersList)
            {
                if (c.y < 3)
                {
                    c.side = CheckersSide.white;
                    c.button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\white.png");
                }
                else if (c.y > 4)
                {
                    c.side = CheckersSide.dark;
                    c.button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\black.png");
                }

            }
            //Счетчик ходов
            var button2 = new Button
            {
                Location = new Point(100, 700),
                Size = new Size(400, 75),
                BackColor = Color.LightGray,
                Enabled = false //Редактируемое
            };
            checkersList.Add(new ListOfFields());
            checkersList[listId].button = button2;
            //button2.Text = "Ход " + counterProgress.ToString();
            Controls.Add(button2);
            GetDiagonals();
            GoGame(0, -1);
        }

        // Делаю диагонали. Некрасиво, но как упростить процесс не придумал
        void GetDiagonals()
        {
            leftDiagonals[0] = new ListOfFields[] { checkersList[0], checkersList[4] };
            leftDiagonals[1] = new ListOfFields[] { checkersList[1], checkersList[5], checkersList[8], checkersList[12] };
            leftDiagonals[2] = new ListOfFields[] { checkersList[2], checkersList[6], checkersList[9], checkersList[13], checkersList[16], checkersList[20] };
            leftDiagonals[3] = new ListOfFields[] { checkersList[3], checkersList[7], checkersList[10], checkersList[14], checkersList[17], checkersList[21], checkersList[24], checkersList[28] };
            leftDiagonals[4] = new ListOfFields[] { checkersList[11], checkersList[15], checkersList[18], checkersList[22], checkersList[25], checkersList[29] };
            leftDiagonals[5] = new ListOfFields[] { checkersList[19], checkersList[23], checkersList[26], checkersList[30] };
            leftDiagonals[6] = new ListOfFields[] { checkersList[27], checkersList[31] };
            leftDiagonals[7] = new ListOfFields[0];
            rightDiagonals[0] = new ListOfFields[] { checkersList[29], checkersList[24], checkersList[20] };
            rightDiagonals[1] = new ListOfFields[] { checkersList[30], checkersList[25], checkersList[21], checkersList[16], checkersList[12] };
            rightDiagonals[2] = new ListOfFields[] { checkersList[31], checkersList[26], checkersList[22], checkersList[17], checkersList[13], checkersList[8], checkersList[4] };
            rightDiagonals[3] = new ListOfFields[] { checkersList[27], checkersList[23], checkersList[18], checkersList[14], checkersList[9], checkersList[5], checkersList[0] };
            rightDiagonals[4] = new ListOfFields[] { checkersList[19], checkersList[15], checkersList[10], checkersList[6], checkersList[1] };
            rightDiagonals[5] = new ListOfFields[] { checkersList[11], checkersList[7], checkersList[2] };
            rightDiagonals[6] = new ListOfFields[0];
        }

        // Метод, который по двум координатам находит индекс списка. Думаю можно сделать поумнее!!!!
        int GetIndex(int x, int y)
        {

            for (var i = 0; i < checkersList.Count; i++)
                if (checkersList[i].x == x && checkersList[i].y == y)
                    return i;
            return -1;
        }

        // Всякие стартовые штуки
        static List<ListOfFields> checkersList = new List<ListOfFields>();
        static List<ListOfFields> activeCheckersList = new List<ListOfFields>();
        static List<ListOfFields> eatingCheckersList = new List<ListOfFields>();
        static ListOfFields[][] leftDiagonals = new ListOfFields[8][];
        static ListOfFields[][] rightDiagonals = new ListOfFields[7][];
        static int counterProgress;
        static List<PossibleSteps> possibleStepsList = new List<PossibleSteps>();
        static  bool kingMark = false;

        // Это на белые/черные. true - белые, false - черные 
        static bool step = false;

        // Выключение всего
        void OffAll()
        {
            foreach (var c in checkersList)
                c.button.Enabled = false;
        }

        void SwapStep()
        {
            step = !step;
            counterProgress++;
        }

        // Метод который пихает активные кнопки в отдельный список
        void GetActiveList()
        {
            foreach (var c in checkersList)
                if (c.button.Enabled)
                    activeCheckersList.Add(c);
        }

        // Поиск диагоналей по индексу поля
        ListOfFields[] GetLeftDiagonal(int x)
        {
            int check = 7;
            for (int i = 0; i < 7; i++)
            {
                foreach (var c in leftDiagonals[i])
                    if (c.listNumber == x)
                    {
                        check = i;
                        break;
                    }    
            }
            return leftDiagonals[check];
        }

        ListOfFields[] GetRightDiagonal(int x)
        {
            int check = 6;
            for (int i = 0; i < 6; i++)
            {
                foreach (var c in rightDiagonals[i])
                    if (c.listNumber == x)
                    {
                        check = i;
                        break;
                    } 
            }
            return rightDiagonals[check];
        }

        // Caм процесс игры!!!
        void GoGame(int progress, int index)
        {
            OffAll();
            if (index == -1)
            {
                CheckStepsAndAtivate();
                //checkersList[32].button.Text = "";
                //foreach (PossibleSteps cc in possibleStepsList)
                //    checkersList[32].button.Text += ";" + cc.startIndex + " " + cc.finalIndex;
                foreach (var c in checkersList)
                {
                    c.button.Click += (sender, args) =>
                    {
                        if (progress == counterProgress)
                        {
                            {
                                if (c.king)
                                    kingMark = true;
                                c.button.Image = null;
                                c.side = CheckersSide.nobody;
                                OffAll();
                                EnableButtonsForStep(c.listNumber, progress);
                            }
                        }
                    };
                }
                return;
            }
            else
            {
                CheckKings();
                CheckStepsAndAtivate();
                foreach (var c in checkersList)
                    if (c.listNumber != index)
                        c.button.Enabled = false;
                checkersList[index].button.Click += (sender, args) =>
                {
                    if (progress == counterProgress)
                    {
                        {
                            if (checkersList[index].king)
                                kingMark = true;
                            checkersList[index].button.Image = null;
                            checkersList[index].side = CheckersSide.nobody;
                            OffAll();
                            EnableButtonsForStep(checkersList[index].listNumber, progress);
                        }
                    }
                };
            }

        }

        void DisableExcessPossibleSteps(int startIndex1, int FinalIndex1)
        {
            foreach (var s in possibleStepsList)
                if (!(s.startIndex == startIndex1 && s.finalIndex == FinalIndex1))
                    possibleStepsList.Remove(s);
        }

        void CheckStepsAndAtivate()
        {
            //ЕДА
            for (var i = 0; i < checkersList.Count - 1; i++)
            {
                var leftDiag = GetLeftDiagonal(i);
                var rightDiag = GetRightDiagonal(i);
                var leftIndex = Array.IndexOf(leftDiag, checkersList[i]);
                var rightIndex = Array.IndexOf(rightDiag, checkersList[i]);
                if (CheckEatUp(leftDiag, leftIndex).Count > 0)
                    for (var j = 0; j < CheckEatUp(leftDiag, leftIndex).Count; j++)
                        possibleStepsList.Add(new PossibleSteps(i, checkersList.IndexOf(leftDiag[CheckEatUp(leftDiag, leftIndex)[j]]), StepType.eat));
                if (CheckEatUp(rightDiag, rightIndex).Count > 0)
                    for (var j = 0; j < CheckEatUp(rightDiag, rightIndex).Count; j++)
                        possibleStepsList.Add(new PossibleSteps(i, checkersList.IndexOf(rightDiag[CheckEatUp(rightDiag, rightIndex)[j]]), StepType.eat));
                if (CheckEatDown(leftDiag, leftIndex).Count > 0)
                    for (var j = 0; j < CheckEatDown(leftDiag, leftIndex).Count; j++)
                        possibleStepsList.Add(new PossibleSteps(i, checkersList.IndexOf(leftDiag[CheckEatDown(leftDiag, leftIndex)[j]]), StepType.eat));
                if (CheckEatDown(rightDiag, rightIndex).Count > 0)
                    for (var j = 0; j < CheckEatDown(rightDiag, rightIndex).Count; j++)
                        possibleStepsList.Add(new PossibleSteps(i, checkersList.IndexOf(rightDiag[CheckEatDown(rightDiag, rightIndex)[j]]), StepType.eat));
            }
            //Тут шаги
            if (!SearchEating())
                for (var i = 0; i < checkersList.Count - 1; i++)
                {
                    var leftDiag = GetLeftDiagonal(i);
                    var rightDiag = GetRightDiagonal(i);
                    var leftIndex = Array.IndexOf(leftDiag, checkersList[i]);
                    var rightIndex = Array.IndexOf(rightDiag, checkersList[i]);
                    if (!step && !checkersList[i].king || checkersList[i].king)
                    {
                        if (CheckStepUp(leftDiag, leftIndex).Count > 0)
                            for (var j = 0; j < CheckStepUp(leftDiag, leftIndex).Count; j++)
                                possibleStepsList.Add(new PossibleSteps(i, CheckStepUp(leftDiag, leftIndex)[j], StepType.step));
                        if (CheckStepUp(rightDiag, rightIndex).Count > 0)
                            for (var j = 0; j < CheckStepUp(rightDiag, rightIndex).Count; j++)
                                possibleStepsList.Add(new PossibleSteps(i, CheckStepUp(rightDiag, rightIndex)[j], StepType.step));
                    }
                    if (step && !checkersList[i].king || checkersList[i].king)
                    {
                        if (CheckStepDown(leftDiag, leftIndex).Count > 0)
                            for (var j = 0; j < CheckStepDown(leftDiag, leftIndex).Count; j++)
                                possibleStepsList.Add(new PossibleSteps(i, CheckStepDown(leftDiag, leftIndex)[j], StepType.step));
                        if (CheckStepDown(rightDiag, rightIndex).Count > 0)
                            for (var j = 0; j < CheckStepDown(rightDiag, rightIndex).Count; j++)
                                possibleStepsList.Add(new PossibleSteps(i, CheckStepDown(rightDiag, rightIndex)[j], StepType.step));
                    }
                }
            CheckGameEnd();
            EnableButtonsFromActivityList();
        }
       
        // Проверка, может ли кушать данная шашка. Дамки и не дамки. Возвращает список индексов конечных позиций.
        List<int> CheckEatUp(ListOfFields[] diag, int startPosition)
        {
            List<int> listTruePositions = new List<int>();
            bool mark = false;
            if (diag.Length == 0 || startPosition == -1)
                return listTruePositions;
            if (!diag[startPosition].king)
            {
                if (startPosition + 2 < diag.Length)
                {
                    if (step && diag[startPosition].side == CheckersSide.white && diag[startPosition + 1].side == CheckersSide.dark &&
                        diag[startPosition + 2].side == CheckersSide.nobody ||
                        !step && diag[startPosition].side == CheckersSide.dark && diag[startPosition + 1].side == CheckersSide.white &&
                        diag[startPosition + 2].side == CheckersSide.nobody)
                    {
                        listTruePositions.Add(startPosition + 2);
                        return listTruePositions;
                    }
                    else
                        return listTruePositions;
                }
                else
                    return listTruePositions;
            }
            else
            {
                if (step && startPosition + 2 < diag.Length && diag[startPosition].side == CheckersSide.white ||
                    !step && startPosition + 2 < diag.Length && diag[startPosition].side == CheckersSide.dark)
                {
                    for (var i = startPosition + 1; i < diag.Length - 1; i++)
                    {
                        if (!mark)
                        {
                            if (step && diag[i].side == CheckersSide.white || !step && diag[i].side == CheckersSide.dark)
                                return listTruePositions;
                            else if (step && diag[i].side == CheckersSide.dark && diag[i + 1].side == CheckersSide.dark ||
                                    !step && diag[i].side == CheckersSide.white && diag[i + 1].side == CheckersSide.white)
                                return listTruePositions;
                            else if (step && diag[i].side == CheckersSide.dark && diag[i + 1].side == CheckersSide.nobody ||
                                    !step && diag[i].side == CheckersSide.white && diag[i + 1].side == CheckersSide.nobody)
                            {
                                mark = true;
                                listTruePositions.Add(i + 1);
                                continue;
                            }
                            else
                                continue;
                        }
                        else
                        {
                            if (diag[i + 1].side == CheckersSide.nobody)
                                listTruePositions.Add(i + 1);
                        }
                    }
                    return listTruePositions;
                }
                else
                    return listTruePositions;
            }
        }

        List<int> CheckEatDown(ListOfFields[] diag, int startPosition)
        {
            List<int> listTruePositions = new List<int>();
            bool mark = false;
            if (diag.Length == 0 || startPosition == -1)
                return listTruePositions;
            if (!diag[startPosition].king)
            {
                if (startPosition >= 2)
                {
                    if (step && diag[startPosition].side == CheckersSide.white && diag[startPosition - 1].side == CheckersSide.dark &&
                        diag[startPosition - 2].side == CheckersSide.nobody ||
                        !step && diag[startPosition].side == CheckersSide.dark && diag[startPosition - 1].side == CheckersSide.white &&
                        diag[startPosition - 2].side == CheckersSide.nobody)
                    {
                        listTruePositions.Add(startPosition - 2);
                        return listTruePositions;
                    }
                    else
                        return listTruePositions;
                }
                else
                    return listTruePositions;
            }
            else
            {
                if (startPosition >= 2 && (step && diag[startPosition].side == CheckersSide.white || !step && diag[startPosition].side == CheckersSide.dark))
                {
                    for (var i = startPosition - 1; i > 0; i--)
                    {
                        if (!mark)
                        {
                            if (step && diag[i].side == CheckersSide.white || !step && diag[i].side == CheckersSide.dark)
                                return listTruePositions;
                            else if (step && diag[i].side == CheckersSide.dark && diag[i - 1].side == CheckersSide.dark ||
                                !step && diag[i].side == CheckersSide.white && diag[i - 1].side == CheckersSide.white)
                                return listTruePositions;
                            else if (step && diag[i].side == CheckersSide.dark && diag[i - 1].side == CheckersSide.nobody ||
                                !step && diag[i].side == CheckersSide.white && diag[i - 1].side == CheckersSide.nobody)
                            {
                                mark = true;
                                listTruePositions.Add(i - 1);
                                continue;
                            }
                            else
                                continue;
                        }
                        else
                        {
                            if (diag[i - 1].side == CheckersSide.nobody)
                                listTruePositions.Add(i - 1);
                            else
                                return listTruePositions;
                        }
                    }
                    return listTruePositions;
                }
                else
                    return listTruePositions;
            }
        }

        // Проверка, может ли ходить данная шашка. Возвращает список индексов конечных позиций
        List<int> CheckStepUp(ListOfFields[] diag, int startPosition)
        {
            List<int> listTruePositions = new List<int>();
            if (diag.Length == 0 || startPosition == -1)
                return listTruePositions;
            if (!diag[startPosition].king)
            {
                if (startPosition + 1 >= diag.Length)
                    return listTruePositions;
                if (step && diag[startPosition].side == CheckersSide.white && diag[startPosition + 1].side == CheckersSide.nobody ||
                    !step && diag[startPosition].side == CheckersSide.dark && diag[startPosition + 1].side == CheckersSide.nobody)
                {
                    listTruePositions.Add(checkersList.IndexOf(diag[startPosition + 1]));
                    return listTruePositions;
                }
                else
                    return listTruePositions;
            }
            else if (step && diag[startPosition].side == CheckersSide.white || !step && diag[startPosition].side == CheckersSide.dark)
            {
                if (startPosition + 1 < diag.Length)
                {
                    for (var i = startPosition + 1; i < diag.Length; i++)
                    {
                        if (diag[i].side == CheckersSide.nobody)
                        {
                            listTruePositions.Add(checkersList.IndexOf(diag[i]));
                        }
                        else break;
                    }
                    return listTruePositions;
                }
                else
                    return listTruePositions;
            }
            else
                return listTruePositions;
        }

        List<int> CheckStepDown(ListOfFields[] diag, int startPosition)
        {
            List<int> listTruePositions = new List<int>();
            if (diag.Length == 0 || startPosition == -1)
                return listTruePositions;
            if (!diag[startPosition].king)
            {
                if (startPosition < 1)
                    return listTruePositions;
                if (startPosition >= 1 && (step && diag[startPosition].side == CheckersSide.white && diag[startPosition - 1].side == CheckersSide.nobody ||
                    !step && diag[startPosition].side == CheckersSide.dark && diag[startPosition - 1].side == CheckersSide.nobody))
                {
                    listTruePositions.Add(checkersList.IndexOf(diag[startPosition - 1]));
                    return listTruePositions;
                }
                else
                    return listTruePositions;
            }
            else if (step && diag[startPosition].side == CheckersSide.white || !step && diag[startPosition].side == CheckersSide.dark)
            {
                if (startPosition >= 1)
                {
                    for (var i = startPosition - 1; i >= 0; i--)
                    {
                        if (diag[i].side == CheckersSide.nobody)
                        {
                            listTruePositions.Add(checkersList.IndexOf(diag[i]));
                        }
                        else break;
                    }
                    return listTruePositions;
                }
                else
                    return listTruePositions;
            }
            else
                return listTruePositions;
        }

        // Проверка на окончание игры
        void CheckGameEnd()
        {
            if (possibleStepsList.Count == 0)
            {
                foreach (var c in checkersList)
                    c.button.Enabled = false;
                if (step)
                    checkersList[32].button.Text = "Black won!";
                else
                    checkersList[32].button.Text = "White won!";
            }
        }

        // Проверяет есть ли в списке возможных ходов еда
        bool SearchEating()
        {
            bool mark = false;
            foreach (var c in possibleStepsList)
                if (c.step == StepType.eat)
                {
                    mark = true;
                    break;
                }
            return mark;
        }

        // Активация кнопок из списка активных
        void EnableButtonsFromActivityList()
        {
            if (!SearchEating())
                foreach (var c in possibleStepsList)
                    checkersList[c.startIndex].button.Enabled = true;
            else
            {
                for (var i = 0; i < possibleStepsList.Count; i++)
                    if (possibleStepsList[i].step == StepType.eat)
                        checkersList[possibleStepsList[i].startIndex].button.Enabled = true;
            }
        }

        // Активация кнопок куда может сходить/скушать одна шашка. И продолжение всего этого
        void EnableButtonsForStep(int checkerIndex, int progress)
        {
            for (var i = 0; i < checkersList.Count; i++)
            {
                foreach(var c in possibleStepsList)
                    if (c.startIndex == checkerIndex)
                        checkersList[c.finalIndex].button.Enabled = true;
            }
            GetActiveList();
            foreach (var c in activeCheckersList)
                c.button.Click += (sender, args) =>
                {
                    if (progress == counterProgress)
                    {
                        var eat = false;
                        foreach (var str in possibleStepsList)
                            if (str.step == StepType.step && str.startIndex == checkerIndex && str.finalIndex == c.listNumber)
                                if (!step)
                                {
                                    checkersList[c.listNumber].side = CheckersSide.dark;
                                    if (!kingMark)
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\black.png");
                                        checkersList[c.listNumber].king = false;
                                    }   
                                    else
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\blackKing.png");
                                        checkersList[c.listNumber].king = true;
                                    }  
                                    CheckKings();
                                }
                                else
                                {
                                    checkersList[c.listNumber].side = CheckersSide.white;
                                    if (!kingMark)
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\white.png");
                                        checkersList[c.listNumber].king = false;
                                    }
                                    else
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\whiteKing.png");
                                        checkersList[c.listNumber].king = true;
                                    }
                                    CheckKings();
                                }
                            else if (str.step == StepType.eat && str.startIndex == checkerIndex && str.finalIndex == c.listNumber)
                            {
                                eat = true;
                                if (!step)
                                {
                                    checkersList[c.listNumber].side = CheckersSide.dark;
                                    if (!kingMark)
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\black.png");
                                        checkersList[c.listNumber].king = false;
                                    }
                                    else
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\blackKing.png");
                                        checkersList[c.listNumber].king = true;
                                    }
                                    ClearButtonsForEat(checkerIndex, c.listNumber);
                                    CheckKings();
                                }
                                else
                                {
                                    checkersList[c.listNumber].side = CheckersSide.white;
                                    if (!kingMark)
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\white.png");
                                        checkersList[c.listNumber].king = false;
                                    }
                                    else
                                    {
                                        checkersList[c.listNumber].button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\whiteKing.png");
                                        checkersList[c.listNumber].king = true;
                                    }
                                    ClearButtonsForEat(checkerIndex, c.listNumber);
                                    CheckKings();
                                }
                            } 
                        if (!eat)
                        {
                            step = !step;
                            counterProgress++;
                            possibleStepsList.Clear();
                            CheckKings();
                            kingMark = false;
                            GoGame(counterProgress, -1);
                        }
                        else
                        {
                            CheckKings();
                            counterProgress++;
                            possibleStepsList.Clear();
                            var leftDiag = GetLeftDiagonal(c.listNumber);
                            var rightDiag = GetRightDiagonal(c.listNumber);
                            var leftIndex = Array.IndexOf(leftDiag, checkersList[c.listNumber]);
                            var rightIndex = Array.IndexOf(rightDiag, checkersList[c.listNumber]);
                            if (CheckEatUp(leftDiag, leftIndex).Count > 0)
                                for (var j = 0; j < CheckEatUp(leftDiag, leftIndex).Count; j++)
                                    possibleStepsList.Add(new PossibleSteps(c.listNumber, checkersList.IndexOf(leftDiag[CheckEatUp(leftDiag, leftIndex)[j]]), StepType.eat));
                            if (CheckEatUp(rightDiag, rightIndex).Count > 0)
                                for (var j = 0; j < CheckEatUp(rightDiag, rightIndex).Count; j++)
                                    possibleStepsList.Add(new PossibleSteps(c.listNumber, checkersList.IndexOf(rightDiag[CheckEatUp(rightDiag, rightIndex)[j]]), StepType.eat));
                            if (CheckEatDown(leftDiag, leftIndex).Count > 0)
                                for (var j = 0; j < CheckEatDown(leftDiag, leftIndex).Count; j++)
                                    possibleStepsList.Add(new PossibleSteps(c.listNumber, checkersList.IndexOf(leftDiag[CheckEatDown(leftDiag, leftIndex)[j]]), StepType.eat));
                            if (CheckEatDown(rightDiag, rightIndex).Count > 0)
                                for (var j = 0; j < CheckEatDown(rightDiag, rightIndex).Count; j++)
                                    possibleStepsList.Add(new PossibleSteps(c.listNumber, checkersList.IndexOf(rightDiag[CheckEatDown(rightDiag, rightIndex)[j]]), StepType.eat));
                            if (possibleStepsList.Count == 0)
                            {
                                step = !step;
                                c.button.Enabled = false;
                                CheckKings();
                                kingMark = false;
                                GoGame(counterProgress, -1);
                            }
                            else
                            {
                                CheckKings();
                                kingMark = false;
                                GoGame(counterProgress, c.listNumber);
                            }   
                        }
                    }
                };
        }

        // Чистка кнопок между 1 и 2 во время еды
        void ClearButtonsForEat(int start, int end)
        {
            var leftDiag = GetLeftDiagonal(start);
            var rightDiag = GetRightDiagonal(start); 
            var mark = false;
            foreach (var c in leftDiag)
                if (c.listNumber == end)
                {
                    mark = true;
                    if (Array.IndexOf(leftDiag, checkersList[start]) < Array.IndexOf(leftDiag, checkersList[end]))
                        for (var i = Array.IndexOf(leftDiag, checkersList[start]) + 1; i < Array.IndexOf(leftDiag, checkersList[end]); i++)
                        {
                            leftDiag[i].button.Image = null;
                            leftDiag[i].side = CheckersSide.nobody;
                        }
                    else
                        for (var i = Array.IndexOf(leftDiag, checkersList[end]) + 1; i < Array.IndexOf(leftDiag, checkersList[start]); i++)
                        {
                            leftDiag[i].button.Image = null;
                            leftDiag[i].side = CheckersSide.nobody;
                        }
                }
            if (!mark)
                foreach (var c in rightDiag)
                    if (c.listNumber == end)
                    {
                        if (Array.IndexOf(rightDiag, checkersList[start]) < Array.IndexOf(rightDiag, checkersList[end]))
                            for (var i = Array.IndexOf(rightDiag, checkersList[start]) + 1; i < Array.IndexOf(rightDiag, checkersList[end]); i++)
                            {
                                rightDiag[i].button.Image = null;
                                rightDiag[i].side = CheckersSide.nobody;
                            }
                        else
                            for (var i = Array.IndexOf(rightDiag, checkersList[end]) + 1; i < Array.IndexOf(rightDiag, checkersList[start]); i++)
                            {
                                rightDiag[i].button.Image = null;
                                rightDiag[i].side = CheckersSide.nobody;
                            }
                    }
        }

        void CheckKings()
        {
            foreach (var c in checkersList)
                if (c.king == false &&
                    (!step && c.side == CheckersSide.dark && (c.listNumber == 4 || c.listNumber == 12 || c.listNumber == 20 || c.listNumber == 28) ||
                    step && c.side == CheckersSide.white && (c.listNumber == 3 || c.listNumber == 11 || c.listNumber == 19 || c.listNumber == 27)))
                {
                    c.king = true;
                    if (step)
                        c.button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\whiteKing.png");
                    else
                        c.button.Image = Image.FromFile(Directory.GetCurrentDirectory().Replace(@"\", @"\\") + "\\blackKing.png");
                }
        }
    }
    
    public enum CheckersSide { nobody, dark, white };
    public enum StepType { step, eat };
    public class ListOfFields
    {
        public Button button;
        public CheckersSide side;
        public int x;
        public int y;
        public bool king;
        public int listNumber;
    }
    public class PossibleSteps
    {
        public int startIndex;
        public int finalIndex;
        public StepType step;
        public PossibleSteps(int startIndex1, int finalIndex1, StepType step1)
        {
            startIndex = startIndex1;
            finalIndex = finalIndex1;
            step = step1;
        }

    }
}
