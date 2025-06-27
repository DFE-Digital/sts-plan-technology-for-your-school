using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class AnswerEntry : ContentfulEntry<CmsAnswerDto>
    {
        public string Text { get; init; } = null!;

        public QuestionEntry? NextQuestion { get; init; }

        public string Maturity { get; init; } = null!;

        protected override CmsAnswerDto CreateDto()
        {
            return new CmsAnswerDto
            {
                Text = Text,
                NextQuestion = NextQuestion?.ToDto(),
                Maturity = Maturity,
            };
        }
    }
}
