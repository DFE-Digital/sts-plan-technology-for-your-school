namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IAnswer
{
    public string Text { get; }

    public string Maturity { get; }
}


public interface IAnswer<TQuestion> : IAnswer
where TQuestion : IQuestion
{
    public TQuestion? NextQuestion { get; }
}