using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "newQuesitonList", menuName = "QuesitonList")]
public class QuestionList : ScriptableObject
{
    public List<QuizQuestion> questionList;
}
