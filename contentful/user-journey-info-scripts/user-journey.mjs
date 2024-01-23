export class UserJourney {
  path;
  maturity;
  section;
  recommendation;

  constructor(path, section) {
    this.path = path;
    this.section = section;

    this.maturity = this.maturityRanking(
      path
        .map((questionAnswer) => questionAnswer.answer.maturity)
        .filter(onlyUnique)
        .map(this.maturityRanking)
        .filter((maturity) => maturity != null)
        .sort()[0]
    );
  }

  setRecommendation(recommendations) {
    const recommendation = recommendations.filter(
      (recommendation) => recommendation.maturity == this.maturity
    );

    if (recommendation == null || recommendation.length == 0) {
      console.error(
        `could not find recommendation for ${this.maturity} in ${this.section.name}`,
        recommendations
      );
      return;
    }

    this.recommendation = recommendation[0];
  }

  maturityRanking(maturity) {
    switch (maturity) {
      case "Low":
        return 0;
      case "Medium":
        return 1;
      case "High":
        return 2;
      case 0:
        return "Low";
      case 1:
        return "Medium";
      case 2:
        return "High";
    }

    return null;
  }

  mapPathToOnlyQuestionAnswerTexts() {
    return this.path.map((pathPart) => {
      return {
        question: pathPart.question.text,
        answer: pathPart.answer.text,
      };
    });
  }
}

const onlyUnique = (value, index, array) => array.indexOf(value) === index;
