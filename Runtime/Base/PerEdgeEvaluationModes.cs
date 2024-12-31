using System;
using UnityEngine;

namespace E7.NotchSolution
{
    [Serializable]
    internal struct PerEdgeEvaluationModes
    {
        [SerializeField] public EdgeEvaluationMode left;
        [SerializeField] public EdgeEvaluationMode bottom;
        [SerializeField] public EdgeEvaluationMode top;
        [SerializeField] public EdgeEvaluationMode right;
    }
}