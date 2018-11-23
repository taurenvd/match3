using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] GameController m_controller;
    [Space]
    [SerializeField] TextMeshProUGUI m_score;
    [SerializeField] TextMeshProUGUI m_goal;
    [SerializeField] TextMeshProUGUI m_message;
    [Space]
    [SerializeField] Color m_message_color;
    [Space]
    [SerializeField] AnimationCurve m_alpha_curve;

    Action<int> score_del;
    Action<GameController.Combo> combo_del;


    void Awake()
    {
        score_del = (score) => { m_score.text = $"Score: {score}"; };
        combo_del = (combo) =>
        {
            //StopAllCoroutines();          
            
            m_message.text = $"{combo.cell_type.ToString()} combo: x<size=200%>{combo.cells.Count}</size>";

            StartCoroutine(ProgressTimer(1.5f, (prog, delta) =>
                {                   
                    m_message_color.a = m_alpha_curve.Evaluate(prog);
                    m_message.color = m_message_color;
                }));
        };
    }

    void OnEnable()
    {
        m_controller.OnScoreChanged += score_del;
        m_controller.OnComboAppearChanged += combo_del;
    }
    void OnDisable()
    {
        m_controller.OnScoreChanged -= score_del;
        m_controller.OnComboAppearChanged -= combo_del;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="life_time"></param>
    /// <param name="update_call">1) Progress[0..1], 2) Time.deltaTime</param>
    /// <param name="final_actions"></param>
    /// <returns></returns>
    public static IEnumerator ProgressTimer(float life_time, Action<float, float> update_call, params Action[] final_actions)
    {
        var cur_time = 0f;
        while (cur_time < life_time)
        {
            cur_time += Time.deltaTime;
            update_call(cur_time / life_time, Time.deltaTime);
            yield return null;
            //Debug.LogAssertionFormat("Time: {0:0.00} Progress: {1:0.00}",cur_time, cur_time / life_time);
        }
        foreach (var action in final_actions)
        {
            action();
        }
    }
    public static IEnumerator Timer(float delay, Action final_action)
    {
        yield return new WaitForSeconds(delay);
        final_action();
    }
}
