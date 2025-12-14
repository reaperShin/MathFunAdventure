using UnityEngine;
using System.Collections.Generic;
using System.Data;

public class Question
{
    public string questionText;
    public string[] options;
    public int correctOptionIndex;

    public Question(string text, string[] opts, int correctIndex)
    {
        questionText = text;
        options = opts;
        correctOptionIndex = correctIndex;
    }
}

public class questionRandomizer : MonoBehaviour
{
    private string[] ops = { "+", "-", "*", "/" };

    private const int MIN_RESULT = 0;
    private const int MAX_RESULT = 100;

    public Question GetRandomQuestion()
    {
        // 90% chance simple, 10% chance advanced (kept mostly simple)
        bool advanced = Random.value < 0.10f;

        if (advanced)
            return GenerateAdvance();
        else
            return GenerateSimple();
    }

    private Question GenerateSimple()
    {
        // Try generating a valid simple question whose result is within [MIN_RESULT, MAX_RESULT]
        int attempts = 0;
        while (attempts < 200)
        {
            attempts++;

            // Weighted operator selection: favor + and -; make * rarer and constrained
            float r = Random.value;
            string op;
            if (r < 0.45f) op = "+";
            else if (r < 0.80f) op = "-";
            else if (r < 0.90f) op = "*"; // 10% multiplication
            else op = "/"; // 10% division

            int a = 0, b = 1;

            if (op == "+" || op == "-")
            {
                a = Random.Range(MIN_RESULT, MAX_RESULT + 1);
                b = Random.Range(MIN_RESULT, MAX_RESULT + 1);
            }
            else if (op == "*")
            {
                // Choose small factors so product stays within bounds
                int af = Random.Range(0, 11);
                int bf = Random.Range(0, 11);
                // avoid zero*something producing 0 (which is acceptable) but allow it
                // retry until product within bounds and not both large
                int mulAttempts = 0;
                while ((af * bf) < MIN_RESULT || (af * bf) > MAX_RESULT)
                {
                    af = Random.Range(0, 11);
                    bf = Random.Range(0, 11);
                    mulAttempts++;
                    if (mulAttempts > 50) break;
                }
                a = af; b = bf;
            }
            else // division
            {
                // pick divisor and quotient so result stays in bounds and is integer
                int divisor = Random.Range(0, 11);
                if (divisor == 0) divisor = 1;
                int quotient = Random.Range(MIN_RESULT, MAX_RESULT + 1);
                a = divisor * quotient;
                b = divisor;
            }

            string expr = $"{a} {op} {b}";
            int correct = Evaluate(expr);
            if (correct >= MIN_RESULT && correct <= MAX_RESULT)
            {
                string[] options = GenerateOptions(correct);
                int correctIndex = System.Array.IndexOf(options, correct.ToString());
                return new Question(expr, options, correctIndex);
            }
        }

        // Fallback safe question
        string fallbackExpr = "1 + 1";
        int fallbackCorrect = Evaluate(fallbackExpr);
        string[] fallbackOpts = GenerateOptions(fallbackCorrect);
        int fallbackIdx = System.Array.IndexOf(fallbackOpts, fallbackCorrect.ToString());
        return new Question(fallbackExpr, fallbackOpts, fallbackIdx);
    }

    private Question GenerateAdvance()
    {
        // Try generating an advanced (two-operator) expression whose result is within bounds
        int attempts = 0;
        while (attempts < 300)
        {
            attempts++;

            // pick ops (weighted like simple)
            float r1 = Random.value;
            string op1 = r1 < 0.45f ? "+" : (r1 < 0.80f ? "-" : (r1 < 0.90f ? "*" : "/"));
            float r2 = Random.value;
            string op2 = r2 < 0.45f ? "+" : (r2 < 0.80f ? "-" : (r2 < 0.90f ? "*" : "/"));

            int a = 0, b = 0, c = 0;

            // generate operands with simple heuristics; divisions handled to ensure integer results
            a = Random.Range(MIN_RESULT, MAX_RESULT + 1);
            b = Random.Range(MIN_RESULT, MAX_RESULT + 1);
            c = Random.Range(MIN_RESULT, MAX_RESULT + 1);

            // adjust for division operators to ensure integer division
            if (op1 == "/")
            {
                int divisor = Random.Range(0, 11);
                if (divisor == 0) divisor = 1;
                int quotient = Random.Range(MIN_RESULT, MAX_RESULT + 1);
                b = divisor; a = divisor * quotient;
            }
            if (op2 == "/")
            {
                int divisor = Random.Range(0, 11);
                if (divisor == 0) divisor = 1;
                int quotient = Random.Range(MIN_RESULT, MAX_RESULT + 1);
                c = divisor; b = divisor * quotient;
            }

            // constrain multiplication operands to avoid huge products
            if (op1 == "*")
            {
                int af = Random.Range(0, 11); int bf = Random.Range(0, 11);
                int mulAttempts = 0;
                while ((af * bf) < MIN_RESULT || (af * bf) > MAX_RESULT)
                {
                    af = Random.Range(0, 11); bf = Random.Range(0, 11);
                    mulAttempts++; if (mulAttempts > 50) break;
                }
                a = af; b = bf;
            }
            if (op2 == "*")
            {
                int af = Random.Range(0, 11); int bf = Random.Range(0, 11);
                int mulAttempts = 0;
                while ((af * bf) < MIN_RESULT || (af * bf) > MAX_RESULT)
                {
                    af = Random.Range(0, 11); bf = Random.Range(0, 11);
                    mulAttempts++; if (mulAttempts > 50) break;
                }
                b = af; c = bf;
            }

            string expr = $"{a} {op1} {b} {op2} {c}";
            int correct = Evaluate(expr);
            if (correct >= MIN_RESULT && correct <= MAX_RESULT)
            {
                string[] options = GenerateOptions(correct);
                int correctIndex = System.Array.IndexOf(options, correct.ToString());
                return new Question(expr, options, correctIndex);
            }
        }

        // Fallback safe advanced question
        string fallbackExpr = "1 + 2 + 3";
        int fallbackCorrect = Evaluate(fallbackExpr);
        string[] fallbackOpts = GenerateOptions(fallbackCorrect);
        int fallbackIdx = System.Array.IndexOf(fallbackOpts, fallbackCorrect.ToString());
        return new Question(fallbackExpr, fallbackOpts, fallbackIdx);
    }

    private int Evaluate(string expr)
    {
        DataTable dt = new DataTable();
        var value = dt.Compute(expr, "");
        // Clamp weird results and avoid overflow
        float f = 0f;
        if (!float.TryParse(value.ToString(), out f)) f = 0f;
        // Round to nearest int and clamp to a safe range
        int result = Mathf.RoundToInt(f);
        result = Mathf.Clamp(result, 0000, 10000);
        return result;
    }

    private string[] GenerateOptions(int correct)
    {
        List<int> opts = new List<int>();
        opts.Add(correct);

        int attempts = 0;
        while (opts.Count < 4 && attempts < 200)
        {
            int wrong = correct + Random.Range(0, 11);
            // Keep options within reasonable bounds and avoid accidental huge values
            wrong = Mathf.Clamp(wrong, 000, 1000);
            if (!opts.Contains(wrong))
                opts.Add(wrong);
            attempts++;
        }

        // If for some reason we couldn't generate enough unique options, fill with safe defaults
        int filler = -50;
        while (opts.Count < 4)
        {
            if (!opts.Contains(filler)) opts.Add(filler);
            filler++;
        }

        // shuffle
        for (int i = 0; i < opts.Count; i++)
        {
            int r = Random.Range(i, opts.Count);
            (opts[i], opts[r]) = (opts[r], opts[i]);
        }

        string[] final = new string[4];
        for (int i = 0; i < 4; i++) final[i] = opts[i].ToString();
        return final;
    }
}
