import ValidateHeader from "./header-validator.js";
import ValidateSection from "./section-validator.js";

function ValidateCategory({ fields, sys }) {
  const header = fields.header;

  const headerTest = ValidateHeader(header);

  const progressHeaderTest = headerTest.parent().next().contains("Progress");

  const progressTest = progressHeaderTest
    .next("p")
    .contains("You have completed");

  const appTaskList = progressTest
    .next("dl.govuk-summary-list")
    .should("exist");

    const sections = fields.sections;

    appTaskList.find("div.govuk-summary-list__row").then(($listItems) => {
       expect($listItems).to.have.length(sections.length);

    for (let index = 0; index < sections.length; index++) {
      const section = sections[index];
      const listItem = $listItems[index];

      ValidateSection(section, listItem);
    }
  });
}

export default ValidateCategory;
